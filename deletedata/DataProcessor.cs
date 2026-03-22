using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace deletedata
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using MySql.Data.MySqlClient;
    using MySqlX.XDevAPI.Common;
    using Serilog;

    public class ProcessAndUpdateDataResult
    {
        public bool Success { get; set; }
        public bool HasBackupTable { get; set; }
        public string Message { get; set; } = string.Empty;
        public int AffectedRows { get; set; }
    }

    public class DataProcessor
    {
        private string? connectionString;
        private Dictionary<string, string> valueEncryptMapping = new Dictionary<string, string>();
        private DatabaseHelper? _dbHelper;

        public ProcessAndUpdateDataResult LastResult { get; private set; } = new ProcessAndUpdateDataResult();

        #region
        public async Task ProcessAndUpdateDataAsync(Dictionary<string, string> keyValuePairs, string connectionString, string tablePrefix, string tableEnd, DateTime mintime, DateTime maxtime, string databaseName = "aq_traces", double min = 0.5, double max = 100, double maxvalue = 0.5, bool useIntegerValue = false)
        {
            InitializeValueEncryptMapping(keyValuePairs, connectionString);
            LastResult = new ProcessAndUpdateDataResult();

            string originalTableName = $"{tablePrefix}denserecord{tableEnd}";
            bool hasBackup = await _dbHelper.BackupTableExistsAsync(databaseName, originalTableName);
            if (hasBackup)
            {
                LastResult.HasBackupTable = true;
                LastResult.Message = "检测到存在备份表，请先还原数据后再进行更新操作。";
                Log.Warning(LastResult.Message);
                return;
            }

            try
            {
                // 1. 查询数据
                var dataToUpdate = await QueryDataForProcessingAsync(tablePrefix, tableEnd, mintime, maxtime, databaseName, min, max);

                if (dataToUpdate.Count == 0)
                {
                    Log.Information("没有需要处理的数据");
                    LastResult.Message = "没有需要处理的数据";
                    return;
                }
                Log.Information($"需要更新 {dataToUpdate.Count} 条记录");
                //2. 处理数据并构建更新语句
                var updateCommands = await ProcessDataAndBuildUpdatesAsync(dataToUpdate, tablePrefix, tableEnd, databaseName, maxvalue, useIntegerValue);

                // 3. 执行批量更新
                await ExecuteBatchUpdatesAsync(updateCommands);

                Log.Information($"成功更新了 {updateCommands.Count} 条记录");
                Log.Fatal($"成功更新了 {updateCommands.Count} 条记录");
                LastResult.Success = true;
                LastResult.AffectedRows = updateCommands.Count;
                LastResult.Message = $"成功更新了 {updateCommands.Count} 条记录";
            }
            catch (Exception ex)
            {
                Log.Information($"处理数据时发生错误: {ex.Message}");
                LastResult.Message = $"处理数据时发生错误: {ex.Message}";
                throw;
            }
        }

        private async Task<List<DataRecord>> QueryDataForProcessingAsync(string tablePrefix, string tableEnd, DateTime mintime, DateTime maxtime, string databaseName = "aq_traces", double min = 0.5, double max = 100)
        {
            var records = new List<DataRecord>();

            string query = $@"
                 SELECT 
                 `RecordId`,
                `Value`as 实际值,
                `ValueEncrypt` ,
                 `VStatus`
                FROM `{databaseName}`.`{tablePrefix}denserecord{tableEnd}`

                where `Value` BETWEEN {min} and {max} and  `CollectTime` BETWEEN '{mintime}' and '{maxtime}' and VStatus NOT LIKE '3758%'

                ORDER BY `Value` ASC;";

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var record = new DataRecord
                        {
                            RecordId = reader.GetInt64("RecordId"),
                            Value = reader.GetDouble("实际值"),
                            ValueEncrypt = reader["ValueEncrypt"]?.ToString(),
                            VStatus = reader.GetInt64("VStatus")
                        };

                        records.Add(record);
                    }
                }
            }

            return records;
        }
        private async Task<List<string>> ProcessDataAndBuildUpdatesAsync(List<DataRecord> records, string tablePrefix, string tableEnd, string databaseName = "aq_traces", double alarmalue = 0.5, bool useIntegerValue = false)
        {
            var updateCommands = new List<string>();
            var validUpdates = new List<(long RecordId, double OldValue, string OldEncrypt, double NewValue, string NewEncrypt)>();

            // 首先验证所有更新，收集有效的更新记录
            foreach (var record in records)
            {
                double newValue = Math.Round(record.Value / 2, 2, MidpointRounding.AwayFromZero);
                newValue = Math.Max(0, Math.Min(newValue, 100));
                if (newValue > alarmalue)
                    newValue = alarmalue;

                if (useIntegerValue)
                {
                    newValue = Math.Floor(newValue);
                }

                string formattedValue = newValue.ToString("0.##");

                if (valueEncryptMapping.TryGetValue(formattedValue, out string encryptedValue))
                {
                    validUpdates.Add((record.RecordId, record.Value, record.ValueEncrypt, newValue, encryptedValue));
                }
                else
                {
                    Log.Warning($"没有找到对应的加密值: {formattedValue}");
                }
            }

            // ✅ 如果没有有效的更新记录，直接返回空列表，不创建表
            if (validUpdates.Count == 0)
            {
                Log.Information("没有需要更新的记录，跳过备份表创建");
                await Task.CompletedTask;
                return updateCommands;
            }

            // ✅ 只有有更新记录时才创建备份表
            string backupTableName = $"{tablePrefix}denserecord{tableEnd}_backup_{DateTime.Now:yyyyMMddHHmmss}";
            updateCommands.Add($"CREATE TABLE IF NOT EXISTS `{databaseName}`.`{backupTableName}` LIKE `{databaseName}`.`{tablePrefix}denserecord{tableEnd}`");

            // 添加备份相关列
            updateCommands.Add($"ALTER TABLE `{databaseName}`.`{backupTableName}` ADD COLUMN BackupTime DATETIME DEFAULT NOW()");
            updateCommands.Add($"ALTER TABLE `{databaseName}`.`{backupTableName}` ADD COLUMN BackupReason VARCHAR(255) DEFAULT '批量更新备份'");

            // 备份数据
            string recordIds = string.Join(",", validUpdates.Select(x => x.RecordId));
            string backupInsertCommand = $@"
        INSERT INTO `{databaseName}`.`{backupTableName}` (RecordId,UniqueId, DevLabel,StationNum,Address,TypeId,SpecialType,Value, ValueEncrypt, VStatus,VStatusTime,CollectTime ,LastUpdateTime, BackupTime, BackupReason)
        SELECT RecordId,UniqueId, DevLabel,StationNum, Address,TypeId, SpecialType,Value, ValueEncrypt, VStatus,VStatusTime, CollectTime,LastUpdateTime, NOW(), '批量更新备份'
        FROM `{databaseName}`.`{tablePrefix}denserecord{tableEnd}`
        WHERE RecordId IN ({recordIds})";

            updateCommands.Add(backupInsertCommand);

            // 生成更新语句
            foreach (var update in validUpdates)
            {
                string formattedValue = update.NewValue.ToString("0.##");
                string updateCommand = $@"
            UPDATE `{databaseName}`.`{tablePrefix}denserecord{tableEnd}` 
            SET Value = {formattedValue}, 
                ValueEncrypt = '{MySqlHelper.EscapeString(update.NewEncrypt)}',
                VStatus = 3221225473
            WHERE RecordId = {update.RecordId}";

                updateCommands.Add(updateCommand);
                Log.Information($"记录ID {update.RecordId}: {update.OldValue} -> {formattedValue}");
            }

            Log.Information($"已备份 {validUpdates.Count} 条记录到表 {backupTableName}");
            await Task.CompletedTask;
            return updateCommands;
        }

        private async Task ExecuteBatchUpdatesAsync(List<string> updateCommands)
        {
            if (updateCommands.Count == 0) return;

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        foreach (var commandText in updateCommands)
                        {
                            using (var command = new MySqlCommand(commandText, connection, transaction))
                            {
                                await command.ExecuteNonQueryAsync();
                            }
                        }

                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        // 辅助方法：初始化加密映射字典
        private void InitializeValueEncryptMapping(Dictionary<string, string> keyValuePairs, string connectionString)
        {
            valueEncryptMapping = keyValuePairs;
            this.connectionString = connectionString;
            _dbHelper = new DatabaseHelper(connectionString);
        }
        private async Task<List<MySqlCommand>> BuildParameterizedUpdatesAsync(List<DataRecord> records)
        {
            var updateCommands = new List<MySqlCommand>();

            foreach (var record in records)
            {
                double newValue = Math.Round(record.Value / 2, 2, MidpointRounding.AwayFromZero);
                string formattedValue = newValue.ToString("0.##"); // 这种格式化会自动去除末尾的0

                if (!valueEncryptMapping.ContainsKey(formattedValue))
                {
                    continue;
                }

                string encryptedValue = valueEncryptMapping[formattedValue];

                string updateSql = @"
            UPDATE your_table 
            SET value_column = @NewValue, 
                encrypted_value = @EncryptedValue,
                update_time = NOW()
            WHERE id = @Id";

                var command = new MySqlCommand(updateSql);
                command.Parameters.AddWithValue("@NewValue", formattedValue);
                command.Parameters.AddWithValue("@EncryptedValue", encryptedValue);
                command.Parameters.AddWithValue("@Id", record.RecordId);

                updateCommands.Add(command);
            }
            await Task.CompletedTask;
            return updateCommands;
        }

        private async Task ExecuteParameterizedUpdatesAsync(List<MySqlCommand> commands)
        {
            if (commands.Count == 0) return;

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        foreach (var command in commands)
                        {
                            command.Connection = connection;
                            command.Transaction = transaction;
                            await command.ExecuteNonQueryAsync();
                        }

                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }


        // 数据记录类
        private class DataRecord
        {
            public long RecordId { get; set; }
            public double Value { get; set; }
            public required string ValueEncrypt { get; set; }

            public long VStatus { get; set; }
        }

        #endregion

        #region
        public async Task ProcessAndUpdateDataRecordAsync(Dictionary<string, string> keyValuePairs, string connectionString, string tablePrefix, string tableEnd, DateTime mintime, DateTime maxtime, string databaseName = "aq_traces", double min = 0.5, double max = 100, double maxvalue = 0.5, bool useIntegerValue = false)
        {
            InitializeValueEncryptMapping(keyValuePairs, connectionString);
            LastResult = new ProcessAndUpdateDataResult();

            string originalTableName = $"s_analogrunrecord{tableEnd}";
            bool hasBackup = await _dbHelper.BackupTableExistsAsync(databaseName, originalTableName);
            if (hasBackup)
            {
                LastResult.HasBackupTable = true;
                LastResult.Message = "检测到存在备份表，请先还原数据后再进行更新操作。";
                Log.Warning(LastResult.Message);
                return;
            }

            try
            {
                // 1. 查询数据
                var dataToUpdate = await QueryDataRecordForProcessingAsync(tablePrefix, tableEnd, mintime, maxtime, databaseName, min, max);

                if (dataToUpdate.Count == 0)
                {
                    Log.Information("没有需要处理的数据");
                    LastResult.Message = "没有需要处理的数据";
                    return;
                }
                Log.Information($"需要更新 {dataToUpdate.Count} 条记录");
                //2. 处理数据并构建更新语句
                var updateCommands = await ProcessDataRecordAndBuildUpdatesAsync(dataToUpdate, tablePrefix, tableEnd, databaseName, maxvalue, useIntegerValue);

                // 3. 执行批量更新
                await ExecuteBatchUpdatesRecordAsync(updateCommands);

                Log.Information($"成功更新了 {updateCommands.Count} 条记录");
                LastResult.Success = true;
                LastResult.AffectedRows = updateCommands.Count;
                LastResult.Message = $"成功更新了 {updateCommands.Count} 条记录";
            }
            catch (Exception ex)
            {
                Log.Information($"处理数据时发生错误: {ex.Message}");
                LastResult.Message = $"处理数据时发生错误: {ex.Message}";
                throw;
            }
        }

        private async Task<List<DataRecord_Record>> QueryDataRecordForProcessingAsync(string tablePrefix, string tableEnd, DateTime mintime, DateTime maxtime, string databaseName = "aq_traces", double min = 0.5, double max = 100)
        {
            var records = new List<DataRecord_Record>();

            string query = $@"
                SELECT 
                     *
                     FROM `{databaseName}`.`s_analogrunrecord{tableEnd}`

                    where UniqueId like '{tablePrefix}%' and `Value` BETWEEN {min} and {max} and  `CollectTime` BETWEEN '{mintime}' and '{maxtime}' and VStatus NOT LIKE '3758%'

                    ORDER BY `Value` ASC;";
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var record = new DataRecord_Record
                        {
                            RecordId = reader.GetInt64("RecordId"),
                            Value = reader.GetDouble("Value"),
                            UniqueId = reader["UniqueId"]?.ToString(),
                            VStatus = reader.GetInt64("VStatus")
                        };

                        records.Add(record);
                    }
                }
            }

            return records;
        }
        private async Task<List<string>> ProcessDataRecordAndBuildUpdatesAsync(List<DataRecord_Record> records, string tablePrefix, string tableEnd, string databaseName = "aq_traces", double alarmalue = 0.5, bool useIntegerValue = false)
        {
            var updateCommands = new List<string>();
            var validUpdates = new List<(long RecordId, double OldValue, long VStatus, double NewValue, long NewVStatus)>();

            // 首先验证所有更新，收集有效的更新记录
            foreach (var record in records)
            {
                double newValue = Math.Round(record.Value / 2, 2, MidpointRounding.AwayFromZero);
                newValue = Math.Max(0, Math.Min(newValue, 100));
                if (newValue > alarmalue)
                    newValue = alarmalue;

                if (useIntegerValue)
                {
                    newValue = Math.Floor(newValue);
                }

                if (record.VStatus != 3221225473)
                    validUpdates.Add((record.RecordId, record.Value, record.VStatus, newValue, 3221225473));
                else
                    validUpdates.Add((record.RecordId, record.Value, record.VStatus, newValue, record.VStatus));
            }

            // ✅ 如果没有有效的更新记录，直接返回空列表，不创建表
            if (validUpdates.Count == 0)
            {
                Log.Information("没有需要更新的记录，跳过备份表创建");
                await Task.CompletedTask;
                return updateCommands;
            }

            // ✅ 只有有更新记录时才创建备份表
            string backupTableName = $"s_analogrunrecord{tableEnd}_backup_{DateTime.Now:yyyyMMddHHmmss}";
            updateCommands.Add($"CREATE TABLE IF NOT EXISTS `{databaseName}`.`{backupTableName}` LIKE `{databaseName}`.`s_analogrunrecord{tableEnd}`");

            // 添加备份相关列
            updateCommands.Add($"ALTER TABLE `{databaseName}`.`{backupTableName}` ADD COLUMN BackupTime DATETIME DEFAULT NOW()");
            updateCommands.Add($"ALTER TABLE `{databaseName}`.`{backupTableName}` ADD COLUMN BackupReason VARCHAR(255) DEFAULT '批量更新备份'");

            // 备份数据
            string recordIds = string.Join(",", validUpdates.Select(x => x.RecordId));
            string backupInsertCommand = $@"
        INSERT INTO `{databaseName}`.`{backupTableName}` (RecordId, UniqueId,VStatus,Value,CollectTime, LastUpdateTime, BackupTime, BackupReason)
        SELECT RecordId, UniqueId,VStatus,Value,CollectTime, LastUpdateTime, NOW(), '批量更新备份'
        FROM `{databaseName}`.`s_analogrunrecord{tableEnd}`
        WHERE RecordId IN ({recordIds})";

            updateCommands.Add(backupInsertCommand);

            // 生成更新语句
            foreach (var update in validUpdates)
            {
                string formattedValue = update.NewValue.ToString("0.##");
                string updateCommand = $@"
            UPDATE `{databaseName}`.`s_analogrunrecord{tableEnd}` 
            SET Value = {formattedValue}, 
            
                VStatus = 3221225473
            WHERE RecordId = {update.RecordId}";

                updateCommands.Add(updateCommand);
                Log.Information($"记录ID {update.RecordId}: {update.OldValue} -> {formattedValue}");
            }

            Log.Information($"已备份 {validUpdates.Count} 条记录到表 {backupTableName}");
            await Task.CompletedTask;
            return updateCommands;
        }

        private async Task ExecuteBatchUpdatesRecordAsync(List<string> updateCommands)
        {
            if (updateCommands.Count == 0) return;

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        foreach (var commandText in updateCommands)
                        {
                            using (var command = new MySqlCommand(commandText, connection, transaction))
                            {
                                await command.ExecuteNonQueryAsync();
                            }
                        }

                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        // 数据记录类
        private class DataRecord_Record
        {
            public long RecordId { get; set; }
            public string UniqueId { get; set; }
            public long VStatus { get; set; }

            public double Value { get; set; }

        }
        #endregion

        #region
        public async Task ProcessAndUpdateDataMinusAsync(Dictionary<string, string> keyValuePairs, string connectionString, string tablePrefix, string tableEnd, DateTime min, DateTime max, string databaseName = "aq_traces", double maxvalue = 0.5, bool useIntegerValue = false)
        {
            InitializeValueEncryptMapping(keyValuePairs, connectionString);
            LastResult = new ProcessAndUpdateDataResult();

            string originalTableName = $"s_analogstaminute{tableEnd}";
            bool hasBackup = await _dbHelper.BackupTableExistsAsync(databaseName, originalTableName);
            if (hasBackup)
            {
                LastResult.HasBackupTable = true;
                LastResult.Message = "检测到存在备份表，请先还原数据后再进行更新操作。";
                Log.Warning(LastResult.Message);
                return;
            }

            try
            {
                // 1. 查询数据
                var dataToUpdate = await QueryDataMinusForProcessingAsync(tablePrefix, tableEnd, min, max, databaseName);

                if (dataToUpdate.Count == 0)
                {
                    Log.Information("没有需要处理的数据");
                    LastResult.Message = "没有需要处理的数据";
                    return;
                }
                Log.Information($"需要更新 {dataToUpdate.Count} 条记录");
                //2. 处理数据并构建更新语句
                var updateCommands = await ProcessDataMinusAndBuildUpdatesAsync(dataToUpdate, tablePrefix, tableEnd, databaseName, maxvalue, useIntegerValue);

                // 3. 执行批量更新
                await ExecuteBatchUpdatesMinusAsync(updateCommands);

                Log.Information($"成功更新了 {updateCommands.Count} 条记录");
                LastResult.Success = true;
                LastResult.AffectedRows = updateCommands.Count;
                LastResult.Message = $"成功更新了 {updateCommands.Count} 条记录";
            }
            catch (Exception ex)
            {
                Log.Information($"处理数据时发生错误: {ex.Message}");
                LastResult.Message = $"处理数据时发生错误: {ex.Message}";
                throw;
            }
        }

        private async Task<List<DataRecord_Minus>> QueryDataMinusForProcessingAsync(string tablePrefix, string tableEnd, DateTime min, DateTime max, string databaseName = "aq_traces")
        {
            var records = new List<DataRecord_Minus>();

            string query = $@"
                SELECT 
                     *
                     FROM `{databaseName}`.`s_analogstaminute{tableEnd}`

                    where `UniqueId` like '{tablePrefix}%'   and  `MaxVTime` BETWEEN '{min}' and '{max}' and `Status` != 2147483850

                    ORDER BY `StaTime` ASC;";
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var record = new DataRecord_Minus
                        {
                            RecordId = reader.GetInt64("RecordId"),

                            UniqueId = reader["UniqueId"]?.ToString(),
                            Status = reader.GetInt64("Status"),
                            MaxValue = reader.GetDouble("MaxValue"),
                            MinValue = reader.GetDouble("MinValue"),
                            AvgValue = reader.GetDouble("AvgValue"),

                        };

                        records.Add(record);
                    }
                }
            }

            return records;
        }
        private async Task<List<string>> ProcessDataMinusAndBuildUpdatesAsync(List<DataRecord_Minus> records, string tablePrefix, string tableEnd, string databaseName = "aq_traces", double alarmalue = 0.5, bool useIntegerValue = false)
        {
            var updateCommands = new List<string>();
            var validUpdates = new List<(long RecordId, double OldMaxValue, double NewMaxValue, double OldAvgValue, double NewAvgValue,
                double OldMinValue, double NewMinValue, long OldStatus, long NewStatus)>();

            // 首先验证所有更新，收集有效的更新记录
            foreach (var record in records)
            {
                double newMaxValue = Math.Round(record.MaxValue / 2, 2, MidpointRounding.AwayFromZero);
                newMaxValue = Math.Max(0, Math.Min(newMaxValue, 100));
                if (newMaxValue > alarmalue)
                    newMaxValue = alarmalue;

                double newAvgValue = Math.Round(record.AvgValue / 2, 2, MidpointRounding.AwayFromZero);
                newAvgValue = Math.Max(0, Math.Min(newAvgValue, 100));
                if (newAvgValue > alarmalue * 0.75)
                    newAvgValue = alarmalue * 0.75;

                double newMinValue = Math.Round(record.MinValue / 2, 2, MidpointRounding.AwayFromZero);
                newMinValue = Math.Max(0, Math.Min(newMinValue, 100));
                if (newMinValue > alarmalue / 2)
                    newMinValue = alarmalue / 2;

                if (useIntegerValue)
                {
                    newMaxValue = Math.Floor(newMaxValue);
                    newAvgValue = Math.Floor(newAvgValue);
                    newMinValue = Math.Floor(newMinValue);
                }

                if (record.Status != 3221225473)
                    validUpdates.Add((record.RecordId, record.MaxValue, newMaxValue, record.AvgValue, newAvgValue, record.MinValue, newMinValue, record.Status, 3221225473));
                else
                    validUpdates.Add((record.RecordId, record.MaxValue, newMaxValue, record.AvgValue, newAvgValue, record.MinValue, newMinValue, record.Status, record.Status));

            }

            // ✅ 如果没有有效的更新记录，直接返回空列表，不创建表
            if (validUpdates.Count == 0)
            {
                Log.Information("没有需要更新的记录，跳过备份表创建");
                await Task.CompletedTask;
                return updateCommands;
            }

            // ✅ 只有有更新记录时才创建备份表
            string backupTableName = $"s_analogstaminute{tableEnd}_backup_{DateTime.Now:yyyyMMddHHmmss}";
            updateCommands.Add($"CREATE TABLE IF NOT EXISTS `{databaseName}`.`{backupTableName}` LIKE `{databaseName}`.`s_analogstaminute{tableEnd}`");

            // 添加备份相关列
            updateCommands.Add($"ALTER TABLE `{databaseName}`.`{backupTableName}` ADD COLUMN BackupTime DATETIME DEFAULT NOW()");
            updateCommands.Add($"ALTER TABLE `{databaseName}`.`{backupTableName}` ADD COLUMN BackupReason VARCHAR(255) DEFAULT '批量更新备份'");

            // 备份数据
            string recordIds = string.Join(",", validUpdates.Select(x => x.RecordId));
            string backupInsertCommand = $@"
            INSERT INTO `{databaseName}`.`{backupTableName}` (`RecordId`, `UniqueId`, `StaTime`, `Status`, `MaxValue`, `MinValue`, `AvgValue`, `MaxVTime`, `MinVTime`, `UpdateTime`, `BackupTime`, `BackupReason`)
            SELECT     `RecordId`, 
                     `UniqueId`,
                     `StaTime`,
                    `Status`,
                     `MaxValue`, 
                    `MinValue`, 
                    `AvgValue`, 
                    `MaxVTime`,
                    `MinVTime`,
                    `UpdateTime`, 
                    NOW(), 
                    '批量更新备份'
             FROM `{databaseName}`.`s_analogstaminute{tableEnd}`
            WHERE `RecordId` IN ({recordIds})";

            updateCommands.Add(backupInsertCommand);

            // 生成更新语句
            foreach (var update in validUpdates)
            {
                string formattednewMaxValue = update.NewMaxValue.ToString("0.##");
                string formattednewMinValue = update.NewMinValue.ToString("0.##");
                string formattednewAvgValue = update.NewAvgValue.ToString("0.##");

                string updateCommand = $@"
            UPDATE `{databaseName}`.`s_analogstaminute{tableEnd}` 
            SET `MaxValue` = {formattednewMaxValue}, 
                `MinValue` = {formattednewMinValue},
                `AvgValue` = {formattednewAvgValue},
                `Status` = 3221225473
            WHERE `RecordId` = {update.RecordId}";

                updateCommands.Add(updateCommand);
                Log.Information($"记录ID {update.RecordId}: {update.OldMaxValue} -> {formattednewMaxValue}");
                Log.Information($"记录ID {update.RecordId}: {update.OldMinValue} -> {formattednewMinValue}");
                Log.Information($"记录ID {update.RecordId}: {update.OldAvgValue} -> {formattednewAvgValue}");
            }

            Log.Information($"已备份 {validUpdates.Count} 条记录到表 {backupTableName}");
            await Task.CompletedTask;
            return updateCommands;
        }

        private async Task ExecuteBatchUpdatesMinusAsync(List<string> updateCommands)
        {
            if (updateCommands.Count == 0) return;

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        foreach (var commandText in updateCommands)
                        {
                            using (var command = new MySqlCommand(commandText, connection, transaction))
                            {
                                await command.ExecuteNonQueryAsync();
                            }
                        }

                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        // 数据记录类
        private class DataRecord_Minus
        {
            public long RecordId { get; set; }
            public string UniqueId { get; set; }
            public long Status { get; set; }



            public double MaxValue { get; set; }
            public double MinValue { get; set; }
            public double AvgValue { get; set; }

        }
        #endregion

        #region
        public async Task ProcessAndUpdateDataHoursAsync(Dictionary<string, string> keyValuePairs, string connectionString, string tablePrefix, string tableEnd, DateTime min, DateTime max, string databaseName = "aq_traces", double maxvalue = 0.5, bool useIntegerValue = false)
        {
            InitializeValueEncryptMapping(keyValuePairs, connectionString);
            LastResult = new ProcessAndUpdateDataResult();

            string originalTableName = $"s_analogstahour{tableEnd}";
            bool hasBackup = await _dbHelper.BackupTableExistsAsync(databaseName, originalTableName);
            if (hasBackup)
            {
                LastResult.HasBackupTable = true;
                LastResult.Message = "检测到存在备份表，请先还原数据后再进行更新操作。";
                Log.Warning(LastResult.Message);
                return;
            }

            try
            {
                // 1. 查询数据
                var dataToUpdate = await QueryDataHoursForProcessingAsync(tablePrefix, tableEnd, min, max, databaseName);

                if (dataToUpdate.Count == 0)
                {
                    Log.Information("没有需要处理的数据");
                    LastResult.Message = "没有需要处理的数据";
                    return;
                }
                Log.Information($"需要更新 {dataToUpdate.Count} 条记录");
                //2. 处理数据并构建更新语句
                var updateCommands = await ProcessDataHoursAndBuildUpdatesAsync(dataToUpdate, tablePrefix, tableEnd, databaseName, maxvalue, useIntegerValue);

                // 3. 执行批量更新
                await ExecuteBatchUpdatesHoursAsync(updateCommands);

                Log.Information($"成功更新了 {updateCommands.Count} 条记录");
                LastResult.Success = true;
                LastResult.AffectedRows = updateCommands.Count;
                LastResult.Message = $"成功更新了 {updateCommands.Count} 条记录";
            }
            catch (Exception ex)
            {
                Log.Information($"处理数据时发生错误: {ex.Message}");
                LastResult.Message = $"处理数据时发生错误: {ex.Message}";
                throw;
            }
        }


        private async Task<List<DataRecord_Hours>> QueryDataHoursForProcessingAsync(string tablePrefix, string tableEnd, DateTime min, DateTime max, string databaseName = "aq_traces")
        {
            var records = new List<DataRecord_Hours>();

            string uniqueIdPattern = $"{tablePrefix}%";
            // string statusToExclude = "3221225473";
            DateTime startTime = min;
            DateTime endTime = max;
            string tableName = $"s_analogstahour{tableEnd}"; // 表名可以根据年份动态生成

            // 构建参数化查询
            string query = $@"
             SELECT * 
             FROM  `aq_main`.`{tableName}`
             WHERE UniqueId LIKE @UniqueIdPattern 
       
            AND StaTime BETWEEN @StartTime AND @EndTime";

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand(query, connection))

                {
                    // 添加参数以防止SQL注入
                    command.Parameters.AddWithValue("@UniqueIdPattern", uniqueIdPattern);
                    //command.Parameters.AddWithValue("@StatusToExclude", statusToExclude);
                    command.Parameters.AddWithValue("@StartTime", startTime);
                    command.Parameters.AddWithValue("@EndTime", endTime);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var record = new DataRecord_Hours
                            {
                                RecordId = reader.GetInt64("RecordId"),

                                UniqueId = reader["UniqueId"]?.ToString(),
                                Status = reader.GetInt64("Status"),
                                MaxValue = reader.GetDouble("MaxValue"),
                                MinValue = reader.GetDouble("MinValue"),
                                AvgValue = reader.GetDouble("AvgValue"),

                            };

                            records.Add(record);
                        }
                    }
                }
            }

            return records;
        }
        private async Task<List<string>> ProcessDataHoursAndBuildUpdatesAsync(List<DataRecord_Hours> records, string tablePrefix, string tableEnd, string databaseName = "aq_traces", double alarmalue = 0.5, bool useIntegerValue = false)
        {
            var updateCommands = new List<string>();
            var validUpdates = new List<(long RecordId, double OldMaxValue, double NewMaxValue, double OldAvgValue, double NewAvgValue,
                double OldMinValue, double NewMinValue, long OldStatus, long NewStatus)>();

            // 首先验证所有更新，收集有效的更新记录
            foreach (var record in records)
            {
                double newMaxValue = Math.Round(record.MaxValue / 2, 2, MidpointRounding.AwayFromZero);
                newMaxValue = Math.Max(0, Math.Min(newMaxValue, 100));
                if (newMaxValue > alarmalue)
                    newMaxValue = alarmalue;

                double newAvgValue = Math.Round(record.AvgValue / 2, 2, MidpointRounding.AwayFromZero);
                newAvgValue = Math.Max(0, Math.Min(newAvgValue, 100));
                if (newAvgValue > alarmalue * 0.75)
                    newAvgValue = alarmalue * 0.75;

                double newMinValue = Math.Round(record.MinValue / 2, 2, MidpointRounding.AwayFromZero);
                newMinValue = Math.Max(0, Math.Min(newMinValue, 100));
                if (newMinValue > alarmalue / 2)
                    newMinValue = alarmalue / 2;

                if (useIntegerValue)
                {
                    newMaxValue = Math.Floor(newMaxValue);
                    newAvgValue = Math.Floor(newAvgValue);
                    newMinValue = Math.Floor(newMinValue);
                }

                if (record.Status != 3221225473)
                    validUpdates.Add((record.RecordId, record.MaxValue, newMaxValue, record.AvgValue, newAvgValue, record.MinValue, newMinValue, record.Status, 3221225473));
                else
                    validUpdates.Add((record.RecordId, record.MaxValue, newMaxValue, record.AvgValue, newAvgValue, record.MinValue, newMinValue, record.Status, record.Status));

            }

            // ✅ 如果没有有效的更新记录，直接返回空列表，不创建表
            if (validUpdates.Count == 0)
            {
                Log.Information("没有需要更新的记录，跳过备份表创建");
                await Task.CompletedTask;
                return updateCommands;
            }

            // ✅ 只有有更新记录时才创建备份表
            string backupTableName = $"s_analogstahour{tableEnd}_backup_{DateTime.Now:yyyyMMddHHmmss}";
            updateCommands.Add($"CREATE TABLE IF NOT EXISTS `{databaseName}`.`{backupTableName}` LIKE `{databaseName}`.`s_analogstahour{tableEnd}`");

            // 添加备份相关列
            updateCommands.Add($"ALTER TABLE `{databaseName}`.`{backupTableName}` ADD COLUMN BackupTime DATETIME DEFAULT NOW()");
            updateCommands.Add($"ALTER TABLE `{databaseName}`.`{backupTableName}` ADD COLUMN BackupReason VARCHAR(255) DEFAULT '批量更新备份'");

            // 备份数据
            string recordIds = string.Join(",", validUpdates.Select(x => x.RecordId));
            string backupInsertCommand = $@"
            INSERT INTO `{databaseName}`.`{backupTableName}` (`RecordId`, `UniqueId`, `StaTime`, `Status`, `MaxValue`, `MinValue`, `AvgValue`, `MaxVTime`, `MinVTime`, `UpdateTime`, `BackupTime`, `BackupReason`)
            SELECT     `RecordId`, 
                     `UniqueId`,
                     `StaTime`,
                    `Status`,
                     `MaxValue`, 
                    `MinValue`, 
                    `AvgValue`, 
                    `MaxVTime`,
                    `MinVTime`,
                    `UpdateTime`, 
                    NOW(), 
                    '批量更新备份'
             FROM `{databaseName}`.`s_analogstahour{tableEnd}`
            WHERE `RecordId` IN ({recordIds})";

            updateCommands.Add(backupInsertCommand);

            // 生成更新语句
            foreach (var update in validUpdates)
            {
                string formattednewMaxValue = update.NewMaxValue.ToString("0.##");
                string formattednewMinValue = update.NewMinValue.ToString("0.##");
                string formattednewAvgValue = update.NewAvgValue.ToString("0.##");

                string updateCommand = $@"
            UPDATE `{databaseName}`.`s_analogstahour{tableEnd}` 
            SET `MaxValue` = {formattednewMaxValue}, 
                `MinValue` = {formattednewMinValue},
                `AvgValue` = {formattednewAvgValue},
                `Status` = 3221225473
            WHERE `RecordId` = {update.RecordId}";

                updateCommands.Add(updateCommand);
                Log.Information($"记录ID {update.RecordId}: {update.OldMaxValue} -> {formattednewMaxValue}");
                Log.Information($"记录ID {update.RecordId}: {update.OldMinValue} -> {formattednewMinValue}");
                Log.Information($"记录ID {update.RecordId}: {update.OldAvgValue} -> {formattednewAvgValue}");
            }

            Log.Information($"已备份 {validUpdates.Count} 条记录到表 {backupTableName}");
            await Task.CompletedTask;
            return updateCommands;
        }

        private async Task ExecuteBatchUpdatesHoursAsync(List<string> updateCommands)
        {
            if (updateCommands.Count == 0) return;

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        foreach (var commandText in updateCommands)
                        {
                            using (var command = new MySqlCommand(commandText, connection, transaction))
                            {
                                await command.ExecuteNonQueryAsync();
                            }
                        }

                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        // 数据记录类
        private class DataRecord_Hours
        {
            public long RecordId { get; set; }
            public string UniqueId { get; set; }
            public long Status { get; set; }



            public double MaxValue { get; set; }
            public double MinValue { get; set; }
            public double AvgValue { get; set; }

        }
        #endregion
    }

}
