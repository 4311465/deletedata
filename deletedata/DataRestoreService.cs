using Mysqlx.Crud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using System.Windows.Forms;
using Google.Protobuf.WellKnownTypes;
using MySqlX.XDevAPI.Common;
using System.Data;
using Serilog;



namespace deletedata
{
    public class DataRestoreService
    {
        private readonly DatabaseHelper _dbHelper;

        public DataRestoreService(string connectionString)
        {
            _dbHelper = new DatabaseHelper(connectionString);
        }

        /// <summary>
        /// 完整的复原流程
        /// </summary>
        public async Task<(bool, DataTable?)> RestoreDataAsync(string databaseName, string originalTable)
        {
            try
            {
                // 1. 获取备份表名（您提到的用法）
                string backupTableName = await _dbHelper.GetBackupTableNameAsync(databaseName, originalTable);
                var backupTablesName = await _dbHelper.GetBackupTablesNameAsync(databaseName, originalTable);


                if (string.IsNullOrEmpty(backupTableName))
                {
                    Log.Information("找不到备份表");
                    return (false, null);
                }
                Log.Information($"找到备份表: {backupTableName}");

                // 2. 检查表是否存在
                if (!await _dbHelper.TableExistsAsync(databaseName, originalTable) ||
                    !await _dbHelper.TableExistsAsync(databaseName, backupTableName))
                {
                    Log.Information("源表或备份表不存在");
                    return (false, null);
                }

                // 3. 获取列信息
                var sourceColumns = await _dbHelper.GetTableColumnInfoAsync(databaseName, originalTable);
                var backupColumns = await _dbHelper.GetTableColumnInfoAsync(databaseName, backupTableName);

                // 4. 找出共同列（排除自增列）
                var commonColumns = sourceColumns
                    .Where(col => !col.IsAutoIncrement)
                    .Select(col => col.Name)
                    .Intersect(backupColumns.Select(col => col.Name))
                    .ToList();

                if (commonColumns.Count == 0)
                {
                    Log.Information("没有找到共同的列");
                    return (false, null);
                }

                // 5. 构建复原SQL
                string setClause = string.Join(", ",
                    commonColumns.Select(col => $"target.{col} = backup.{col}"));

                string restoreSql = $@"
                UPDATE `{databaseName}`.`{originalTable}` AS target
                INNER JOIN `{databaseName}`.`{backupTableName}` AS backup 
                    ON target.RecordId = backup.RecordId
                SET {setClause}";

                // 6. 执行复原
                int affectedRows = await _dbHelper.ExecuteNonQueryAsync(restoreSql);
                Log.Information($"成功复原 {affectedRows} 条记录");

                // 7. 删除备份表
                bool dropSuccess = await _dbHelper.DropBackupTablesAsync(databaseName, originalTable);
                if (dropSuccess)
                {
                    Log.Information($"已删除备份表");
                }
                else
                {
                    Log.Warning($"删除备份表失败，请手动删除");
                }

                return (affectedRows > 0, backupTablesName);
            }
            catch (Exception ex)
            {
                Log.Information($"复原失败: {ex.Message}");
                return (false, null);
            }
        }
        public async Task<bool> RestoreInsertDataAsync(string databaseName, string originalTable)
        {
            try
            {
                // 1. 获取备份表名（您提到的用法）
                string backupTableName = await _dbHelper.GetBackupTableNameAsync(databaseName, originalTable);

                if (string.IsNullOrEmpty(backupTableName))
                {
                    Log.Information("找不到备份表");
                    return false;
                }

                Log.Information($"找到备份表: {backupTableName}");

                // 2. 检查表是否存在
                if (!await _dbHelper.TableExistsAsync(databaseName, originalTable) ||
                    !await _dbHelper.TableExistsAsync(databaseName, backupTableName))
                {
                    Log.Information("源表或备份表不存在");
                    return false;
                }

                // 3. 获取列信息
                var sourceColumns = await _dbHelper.GetTableColumnInfoAsync(databaseName, originalTable);
                var backupColumns = await _dbHelper.GetTableColumnInfoAsync(databaseName, backupTableName);

                // 4. 找出共同列（排除自增列）
                var commonColumns = sourceColumns
                    .Where(col => !col.IsAutoIncrement)
                    .Select(col => col.Name)
                    .Intersect(backupColumns.Select(col => col.Name))
                    .ToList();

                if (commonColumns.Count == 0)
                {
                    Log.Information("没有找到共同的列");
                    return false;
                }

                // 5. 构建复原SQL
                string setClause = string.Join(", ",
                    commonColumns.Select(col => $"target.{col} = backup.{col}"));


                //string restoreSql = $@"
                //    INSERT INTO `{databaseName}`.`{originalTable}` (RecordId, UniqueId, Value,VStatus,VStatusTime,BTime,ETime,MaxValue,MinValue,
                //            AvgValue,MaxVTime,MinVTime,DataSign,UpdateTime)
                //        SELECT RecordId, UniqueId, Value,VStatus,VStatusTime,BTime,ETime,MaxValue,MinValue,AvgValue,MaxVTime,MinVTime,DataSign,UpdateTime
                //            FROM `{databaseName}`.`{backupTableName}`";
                string restoreSql = $@"
                INSERT INTO `{databaseName}`.`{originalTable}`
                (`RecordId`, `UniqueId`, `Value`, `VStatus`, `VStatusTime`, `BTime`, `ETime`, 
             `MaxValue`, `MinValue`, `AvgValue`, `MaxVTime`, `MinVTime`, `DataSign`,`UpdateTime`)
            SELECT
            `RecordId`, `UniqueId`, `Value`, `VStatus`, `VStatusTime`, `BTime`, `ETime`,
             `MaxValue`, `MinValue`, `AvgValue`, `MaxVTime`, `MinVTime`, `DataSign`,`UpdateTime`
                FROM `{databaseName}`.`{backupTableName}`";

                // 6. 执行复原
                int affectedRows = await _dbHelper.ExecuteNonQueryAsync(restoreSql);
                Log.Information($"成功复原 {affectedRows} 条记录");

                // 7. 删除备份表
                bool dropSuccess = await _dbHelper.DropBackupTablesAsync(databaseName, originalTable);
                if (dropSuccess)
                {
                    Log.Information($"已删除备份表");
                }
                else
                {
                    Log.Warning($"删除备份表失败，请手动删除");
                }

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                Log.Information($"复原失败: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// 创建备份表
        /// </summary>
        public async Task<bool> CreateBackupTableAsync(string databaseName, string tableName)
        {
            string backupTableName = $"{tableName}_backup_{DateTime.Now:yyyyMMdd_HHmmss}";

            string sql = $@"
            CREATE TABLE `{databaseName}`.`{backupTableName}` 
            AS SELECT * FROM `{databaseName}`.`{tableName}`";

            int result = await _dbHelper.ExecuteNonQueryAsync(sql);
            return result >= 0; // CREATE TABLE 返回 -1，但执行成功
        }
    }

}
