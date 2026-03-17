using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace deletedata
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// 执行查询并返回单个值
        /// </summary>
        public async Task<T> ExecuteScalarAsync<T>(string sql, object parameters = null)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        AddParameters(command, parameters);
                    }

                    var result = await command.ExecuteScalarAsync();
                    return result is DBNull ? default(T) : (T)Convert.ChangeType(result, typeof(T));
                }
            }
        }
        /// <summary>
        /// 执行查询并返回DataTable
        /// </summary>
        public async Task<DataTable> ExecuteDataTableAsync(string sql, object parameters = null)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        AddParameters(command, parameters);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var dataTable = new DataTable();
                        dataTable.Load(reader);
                        return dataTable;
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询并返回对象列表
        /// </summary>
        public async Task<List<T>> ExecuteQueryAsync<T>(string sql, object parameters = null) where T : new()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        AddParameters(command, parameters);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var results = new List<T>();
                        var properties = typeof(T).GetProperties();

                        while (await reader.ReadAsync())
                        {
                            var item = new T();

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var columnName = reader.GetName(i);
                                var property = properties.FirstOrDefault(p =>
                                    p.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                                if (property != null && property.CanWrite && !reader.IsDBNull(i))
                                {
                                    var value = reader.GetValue(i);
                                    property.SetValue(item, Convert.ChangeType(value, property.PropertyType));
                                }
                            }

                            results.Add(item);
                        }

                        return results;
                    }
                }
            }
        }

        
        /// <summary>
        /// 执行非查询操作（INSERT/UPDATE/DELETE）
        /// </summary>
        public async Task<int> ExecuteNonQueryAsync(string sql, object parameters = null)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        AddParameters(command, parameters);
                    }

                    return await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// 执行查询并返回DataReader
        /// </summary>
        public async Task<DbDataReader> ExecuteReaderAsync(string sql, object parameters = null)
        {
            var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new MySqlCommand(sql, connection);
            if (parameters != null)
            {
                AddParameters(command, parameters);
            }

            return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// 获取表列信息
        /// </summary>
        public async Task<List<ColumnInfo>> GetTableColumnInfoAsync(string databaseName, string tableName)
        {
            var columns = new List<ColumnInfo>();

            string sql = @"
            SELECT 
                COLUMN_NAME, 
                EXTRA,
                COLUMN_KEY,
                IS_NULLABLE,
                COLUMN_DEFAULT
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_SCHEMA = @databaseName 
            AND TABLE_NAME = @tableName
            ORDER BY ORDINAL_POSITION";

            using (var reader = await ExecuteReaderAsync(sql, new
            {
                databaseName,
                tableName
            }))
            {
                while (await reader.ReadAsync())
                {
                    var columnInfo = new ColumnInfo
                    {
                        Name = reader.GetString("COLUMN_NAME"),
                        Extra = reader.GetString("EXTRA"),
                        ColumnKey = reader.GetString("COLUMN_KEY"),
                        IsNullable = reader.GetString("IS_NULLABLE") == "YES",
                        DefaultValue = reader["COLUMN_DEFAULT"] is DBNull ? null : reader["COLUMN_DEFAULT"].ToString()
                    };

                    columns.Add(columnInfo);
                }
            }

            return columns;
        }

        /// <summary>
        /// 获取备份表名（您提到的用法）
        /// </summary>
        public async Task<string> GetBackupTableNameAsync(string databaseName,string originalTableName)
        {
            // 示例：查询最新的备份表
            string sql = @"
            SELECT TABLE_NAME 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_SCHEMA = @databaseName 
            AND TABLE_NAME LIKE @pattern 
            ORDER BY CREATE_TIME DESC 
            LIMIT 1";

            string pattern = $"{originalTableName}_backup_%";

            return await ExecuteScalarAsync<string>(sql, new { databaseName, pattern });
        }
        /// <summary>
        /// 获取备份表名（您提到的用法）
        /// </summary>
        public async Task<DataTable> GetBackupTablesNameAsync(string databaseName, string originalTableName)
        {
            // 示例：查询最新的备份表
            string sql = @"
            SELECT TABLE_NAME 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_SCHEMA = @databaseName 
            AND TABLE_NAME LIKE @pattern 
            ORDER BY CREATE_TIME DESC 
             ";

            string pattern = $"{originalTableName}_backup_%";

            return await ExecuteDataTableAsync(sql, new { databaseName, pattern });
        }

        /// <summary>
        /// 检查表是否存在
        /// </summary>
        public async Task<bool> TableExistsAsync(string databaseName, string tableName)
        {
            string sql = @"
            SELECT COUNT(*) 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_SCHEMA = @databaseName 
            AND TABLE_NAME = @tableName";

            int count = await ExecuteScalarAsync<int>(sql, new { databaseName, tableName });
            return count > 0;
        }

        /// <summary>
        /// 添加参数到命令（辅助方法）
        /// </summary>
        private void AddParameters(MySqlCommand command, object parameters)
        {
            if (parameters != null)
            {
                var properties = parameters.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(parameters);
                    command.Parameters.AddWithValue("@" + prop.Name, value ?? DBNull.Value);
                }
            }
        }
    }

    /// <summary>
    /// 列信息模型
    /// </summary>
    public class ColumnInfo
    {
        public string Name { get; set; }
        public string Extra { get; set; }
        public string ColumnKey { get; set; }
        public bool IsNullable { get; set; }
        public string DefaultValue { get; set; }

        public bool IsPrimaryKey => ColumnKey == "PRI";
        public bool IsAutoIncrement => Extra.Contains("auto_increment");

        public override string ToString()
        {
            return $"{Name} (PK: {IsPrimaryKey}, AutoInc: {IsAutoIncrement}, Nullable: {IsNullable})";
        }
    }
}
