using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static deletedata.Form1;

namespace deletedata
{
    public class LicenseService
    {
        public async Task<DateTime?> GetExpireTimeFromDatabaseAsync(string connectionString)
        {
            const string sql = "SELECT NOW();";

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        var result = await command.ExecuteScalarAsync();

                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToDateTime(result);
                        }
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录日志
                Console.WriteLine($"获取过期时间失败: {ex.Message}");
                return null;
            }
        }
    }


    //public static class AppConfig
    //{
    //    public static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(10);
    //    public static readonly string ConnectionString = "Server=localhost;Database=YourDB;Uid=root;Pwd=123456;";
    //    public static readonly DateTime HardcodedExpireTime = new DateTime(2025, 12, 31); // 硬编码过期时间
    //}


}
