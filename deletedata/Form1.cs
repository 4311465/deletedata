
//using Microsoft.Data.SqlClient;  // 移除SQL Server命名空间
using MySql.Data.MySqlClient;    // 添加MySQL命名空间
using System.Data;
using Newtonsoft.Json;
//using Serilog; // 添加JSON解析支持
// The error CS1061 indicates that the method `WinFormRichTextBox` is not recognized as part of the `LoggerSinkConfiguration` class.  
// This suggests that the `Serilog.Sinks.WinForms` package might not be correctly installed or referenced.  
// To fix this issue, ensure the following steps are completed:  

// 1. Install the required NuGet package:  
// Open the NuGet Package Manager in Visual Studio and install the `Serilog.Sinks.WinForms` package.  
// Command: Install-Package Serilog.Sinks.WinForms  

// 2. Add the correct `using` directive:  
using Serilog.Sinks.WinForms;

using Serilog;
using Serilog.Sinks.WinForms.Base;
using Serilog.Formatting.Display;
using Serilog.Sinks.WinForms.Core;
using System.Windows.Forms;
using static Mysqlx.Expect.Open.Types;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using System.Net.NetworkInformation;
using MySqlX.XDevAPI.Common;
using Microsoft.VisualBasic;
using System.Configuration;
using Microsoft.Extensions.Configuration;

// 3. Ensure the `Serilog.Sinks.WinForms` assembly is referenced in your project.  

// After completing these steps, the `WinFormRichTextBox` method should be recognized.  
// If the issue persists, verify that the package version is compatible with your .NET target framework (.NET 8).

namespace deletedata
{



    public partial class Form1 : Form
    {
        private readonly LicenseService _licenseService = new LicenseService();
        private readonly DateTime _hardcodedExpireTime = new DateTime(2026, 10, 1);
        private Dictionary<string, string> valueEncryptMapping = new Dictionary<string, string>();
        private string connectionString = "Server=localhost,1433;Database=UWB_DataTable;User Id=sa;Password=hnma@1972;TrustServerCertificate=True;";
        private void LoadConfiguration()
        {

            //IConfiguration configuration = new ConfigurationBuilder()
            //    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            //    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            //    .Build();
            try
            {
                // 读取配置文件
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (!File.Exists(configPath))
                {
                    MessageBox.Show($"配置文件未找到: {configPath}");
                    return;
                }

                var configJson = File.ReadAllText(configPath);
                var config = JsonConvert.DeserializeObject<AppConfig>(configJson);

                // 获取MySQL连接字符串
                var mysqlConnection = config.appConfig.DbConns
                    .FirstOrDefault(c => c.Type.Equals("MySQL", StringComparison.OrdinalIgnoreCase));

                if (mysqlConnection != null)
                {
                    connectionString = mysqlConnection.Conn;
                    this.Text += $" - 已连接: {mysqlConnection.Key}"; // 在窗口标题显示
                }
                else
                {
                    MessageBox.Show("配置文件中未找到MySQL连接字符串");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载配置失败: {ex.Message}");
            }
        }


        public Form1()
        {
            InitializeComponent();
            LoadConfiguration();
            this.Load += async (sender, e) =>
            {
                try
                {
                    if (checkBox1.Checked)
                    {
                        await InitializeComboBoxAsync(102001);
                    }
                    else
                    {
                        await InitializeComboBoxAsync(101003);
                    }
                }
                catch (Exception ex)
                {
                    // 必须捕获异常以防止 async void 导致程序崩溃
                    // 建议在此处添加日志记录或用户错误提示
                    // System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }; // Attach async event handler
            toolTip1.SetToolTip(delte_2mius, "此处一定要确认删除是否为标校数据");
            // 设置全局样式
            toolTip1.ToolTipTitle = "操作提示";
            toolTip1.IsBalloon = true;
            toolTip1.BackColor = Color.LightYellow;
            toolTip1.ForeColor = Color.DarkBlue;
            comboBox1.SelectedIndex = 4; // 选中第二个选项（索引从0开始）
            startDateTimePicker.Format = DateTimePickerFormat.Custom;
            startDateTimePicker.CustomFormat = "yyyy-MM-dd HH:mm:ss"; // 自定义格式  

            endDateTimePicker.Format = DateTimePickerFormat.Custom;
            endDateTimePicker.CustomFormat = "yyyy-MM-dd HH:mm:ss"; // 自定义格式  

            startDateTimePicker.Value = DateTime.Now.AddDays(-1);
            endDateTimePicker.Value = DateTime.Now;
        }




        /// <summary>
        /// 执行UNION查询，获取实际值和加密值映射
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        private async Task<DataTable> ExecuteUnionQueryWithDatabaseAsync(string databaseName = "aq_traces")
        {
            DataTable resultTable = new DataTable();

            string query = $@"
SELECT 
  `Value`,
  MAX(ValueEncrypt) AS ValueEncrypt
FROM `{databaseName}`.`009a16denserecord202509`
GROUP BY `Value`

UNION 

SELECT 
  `Value`,
  MAX(ValueEncrypt) AS ValueEncrypt
FROM `{databaseName}`.`009a16denserecord202508`
GROUP BY `Value`

UNION 

SELECT 
  `Value`,
  MAX(ValueEncrypt) AS ValueEncrypt
FROM `{databaseName}`.`009a16denserecord202507`
GROUP BY `Value`

UNION 

SELECT 
  `Value`,
  MAX(ValueEncrypt) AS ValueEncrypt
FROM `{databaseName}`.`009a16denserecord202506`
GROUP BY `Value`

UNION 

SELECT 
  `Value`,
  MAX(ValueEncrypt) AS ValueEncrypt
FROM `{databaseName}`.`007a03denserecord202509`
GROUP BY `Value`

UNION 

SELECT 
  `Value`,
  MAX(ValueEncrypt) AS ValueEncrypt
FROM `{databaseName}`.`007a03denserecord202508`
GROUP BY `Value`

UNION 

SELECT 
  `Value`,
  MAX(ValueEncrypt) AS ValueEncrypt
FROM `{databaseName}`.`007a03denserecord202507`
GROUP BY `Value`



ORDER BY `Value` ASC;";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            // 使用Task.Run将同步的Fill方法包装为异步
                            await Task.Run(() => adapter.Fill(resultTable));
                        }
                    }
                }
                Log.Fatal("查询保存密码成功，返回 {resultTable.Rows.Count} 条记录");
                Log.Information($"查询成功，返回 {resultTable.Rows.Count} 条记录");
                return resultTable;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "执行UNION查询时发生错误");
                MessageBox.Show($"查询失败: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// 根据测点号和时间查询实际值和加密值映射
        /// </summary>
        /// <param name="tablePrefix"></param>
        /// <param name="tableEnd"></param>
        /// <param name="databaseName"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private async Task<DataTable> ExecuteQueryWithDatabaseCedianAsync(string tablePrefix, string tableEnd, DateTime mintime, DateTime maxtime, string databaseName = "aq_traces", double min = 0.5, double max = 100)
        {
            //tablePrefix= texboxID.Text;
            //tableEnd = startDateTimePicker.Value.ToString("yyyyMM"); ;

            DataTable resultTable = new DataTable();

            string query = $@"
                SELECT 
                    `RecordId`,
                    `Value`as 实际值,
                    `ValueEncrypt` ,
                    `VStatus`,
                    `LastUpdateTime` 
                     FROM `{databaseName}`.`{tablePrefix}denserecord{tableEnd}`

                    where `Value` BETWEEN {min} and {max}  and  `CollectTime` BETWEEN '{mintime}' and '{maxtime}' and VStatus NOT LIKE '3758%'

                    ORDER BY `Value` ASC;";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            // 使用Task.Run将同步的Fill方法包装为异步
                            await Task.Run(() => adapter.Fill(resultTable));
                        }
                    }
                }
                Log.Fatal("查询测点成功，返回 {resultTable.Rows.Count} 条记录");
                Log.Information($"查询成功，返回 {resultTable.Rows.Count} 条记录");
                return resultTable;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "执行UNION查询时发生错误");
                MessageBox.Show($"查询失败: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// 查询保存密码映射
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnExecuteQuery_Click(object sender, EventArgs e)
        {
            try
            {
                // 显示加载状态
                btnExecuteQuery.Enabled = false;
                btnExecuteQuery.Text = "查询中...";

                Log.Information("开始执行UNION查询...");

                // 执行异步查询
                DataTable result = await ExecuteUnionQueryWithDatabaseAsync("aq_traces");


                if (result != null && result.Rows.Count > 0)
                {

                    // 显示结果
                    Log.Information("查询结果:");
                    Log.Information("══════════════════════════════════");
                    Log.Information("Value\t\tValueEncrypt");
                    Log.Information("──────────────────────────────────");

                    foreach (DataRow row in result.Rows)
                    {
                        string value = row["Value"].ToString();
                        string valueEncrypt = row["ValueEncrypt"].ToString();

                        if (!valueEncryptMapping.ContainsKey(value))
                        {
                            valueEncryptMapping.Add(value, valueEncrypt);
                        }
                        else
                        {
                            Log.Warning($"重复的Value值: {value}");
                        }
                        Log.Information($"{row["Value"]}\t\t{row["ValueEncrypt"]}");
                    }
                    Log.Information($"成功加载 {valueEncryptMapping.Count} 个映射关系");
                    Log.Information($"总共 {result.Rows.Count} 条记录");

                    // 如果有DataGridView，绑定数据
                    if (dataGridView1 != null)
                    {
                        dataGridView1.DataSource = result;
                        Log.Fatal($"查询密采成功，返回 {result.Rows.Count} 条记录");
                    }
                }
                else
                {
                    Log.Information("未找到匹配的记录");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "查询操作失败");
                MessageBox.Show($"操作失败: {ex.Message}");
            }
            finally
            {
                // 恢复按钮状态
                btnExecuteQuery.Enabled = true;
                btnExecuteQuery.Text = "存储密码映射";
            }
        }

        /// <summary>
        /// 删除按钮删除密采数据点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btndelete_Click(object sender, EventArgs e)
        {
            string condition = cedianhao.Text; // 从文本框获取删除条件
            DateTime startDate = startDateTimePicker.Value;
            DateTime endDate = endDateTimePicker.Value;
            int dayOfYear = endDateTimePicker.Value.Day;


            double maxAlarmValue = double.Parse(comboBox1.Text);
            if (!minValue.TryParseDouble(out double min, "最小值") ||
                !maxValue.TryParseDouble(out double max, "最大值"))
            {
                return;
            }

            // 或者带范围验证
            if (!minValue.TryParseDouble(out min, 0, 100, "最小值") ||
                !maxValue.TryParseDouble(out max, min, 100, "最大值"))
            {
                return;
            }

            if (min >= max)
            {
                MessageBox.Show("最小值必须小于最大值");
                minValue.Focus();
                minValue.SelectAll();
                return;
            }

            if (string.IsNullOrEmpty(condition))
            {
                MessageBox.Show("请输入删除条件！");
                return;
            }
            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("数据库连接未配置!");
                return;
            }
            try
            {
                query_value_with_cedian.Enabled = false;
                Restore_btn.Enabled = false;
                btndelete_micai.Enabled = false;
                btndelete_micai.Text = "更新中...";
                var DataProcessor = new DataProcessor();
                await DataProcessor.ProcessAndUpdateDataAsync(valueEncryptMapping, connectionString, (cedianhao.SelectedItem as ComboBoxItem).Value, startDateTimePicker.Value.ToString("yyyyMM"), startDateTimePicker.Value, endDateTimePicker.Value, "aq_traces", double.Parse(minValue.Text), double.Parse(maxValue.Text), maxAlarmValue);
                Log.Fatal($"更新完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "查询操作失败");
                MessageBox.Show($"操作失败: {ex.Message}");
            }
            finally
            {
                // 恢复按钮状态
                query_value_with_cedian.Enabled = true;
                Restore_btn.Enabled = true;
                btndelete_micai.Enabled = true;
                btndelete_micai.Text = "更新密采数据";
            }

        }
        /// <summary>
        /// 查询指定测点号和时间范围内的实际值和加密值映射
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void query_value_with_cedian_Click(object sender, EventArgs e)
        {
            string condition = cedianhao.Text; // 从文本框获取删除条件
            double max;
            if (!minValue.TryParseDouble(out double min, "最小值") ||
                !maxValue.TryParseDouble(out max, "最大值"))
            {
                return;
            }

            // 或者带范围验证
            if (!minValue.TryParseDouble(out min, 0, 100, "最小值") ||
                !maxValue.TryParseDouble(out max, min, 100, "最大值"))
            {
                return;
            }

            if (min >= max)
            {
                MessageBox.Show("最小值必须小于最大值");
                minValue.Focus();
                minValue.SelectAll();
                return;
            }
            if (string.IsNullOrEmpty(condition))
            {
                MessageBox.Show("请输入删除条件！");
                return;
            }
            try
            {
                // 显示加载状态
                query_value_with_cedian.Enabled = false;
                Restore_btn.Enabled = false;
                btndelete_micai.Enabled = false;
                query_value_with_cedian.Text = "查询中...";

                Log.Information("开始执行条件查询...");

                // 执行异步查询
                DataTable result = await ExecuteQueryWithDatabaseCedianAsync((cedianhao.SelectedItem as ComboBoxItem).Value, startDateTimePicker.Value.ToString("yyyyMM"), startDateTimePicker.Value, endDateTimePicker.Value, "aq_traces", double.Parse(minValue.Text), double.Parse(maxValue.Text));


                if (result != null && result.Rows.Count > 0)
                {

                    // 显示结果
                    Log.Information("查询结果:");
                    Log.Information("══════════════════════════════════");
                    Log.Information("Value\t\tValueEncrypt");
                    Log.Information("──────────────────────────────────");

                    foreach (DataRow row in result.Rows)
                    {
                        string value = row["实际值"].ToString();
                        string valueEncrypt = row["ValueEncrypt"].ToString();
                        //string vstatus = row["VStatus"].ToString();
                        if (!valueEncryptMapping.ContainsKey(value))
                        {
                            valueEncryptMapping.Add(value, valueEncrypt);
                        }
                        else
                        {
                            Log.Warning($"重复的Value值: {value}");
                        }
                        Log.Information($"{row["实际值"]}\t\t{row["ValueEncrypt"]}");
                    }
                    Log.Information($"成功加载 {valueEncryptMapping.Count} 个映射关系");
                    Log.Information($"总共 {result.Rows.Count} 条记录");

                    // 如果有DataGridView，绑定数据
                    if (dataGridView1 != null)
                    {
                        dataGridView1.DataSource = result;
                    }
                }
                else
                {
                    Log.Information("未找到匹配的记录");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "查询操作失败");
                MessageBox.Show($"操作失败: {ex.Message}");
            }
            finally
            {
                // 恢复按钮状态
                query_value_with_cedian.Enabled = true;
                Restore_btn.Enabled = true;
                btndelete_micai.Enabled = true;
                query_value_with_cedian.Text = "执行查询";
            }
        }
        /// <summary>
        /// 恢复密采数据按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Restore_btn_Click(object sender, EventArgs e)
        {
            try
            { // 禁用按钮防止重复点击
                query_value_with_cedian.Enabled = false;
                Restore_btn.Enabled = false;
                btndelete_micai.Enabled = false;
                Restore_btn.Text = "恢复中...";
                var selectedTable = (cedianhao.SelectedItem as ComboBoxItem)?.Value;
                // 组合表名格式：测点号_denserecord_年月
                string tableName = $"{selectedTable}denserecord{startDateTimePicker.Value.ToString("yyyyMM")}";
                var restoreService = new DataRestoreService(connectionString);

                var restoreSuccess = await restoreService.RestoreDataAsync("aq_traces", tableName);


                if (restoreSuccess.Item1)
                {
                    dataGridView1.DataSource = restoreSuccess.Item2;
                    MessageBox.Show($"数据恢复成功！\n受影响记录已从备份表  恢复",
                                   "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // 可选：记录恢复日志
                    Log.Information(selectedTable, "成功");
                }
                else
                {

                    MessageBox.Show("数据恢复失败，请检查日志", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    Log.Information(selectedTable, "失败");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show($"恢复过程中发生错误：{ex.Message}", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // 记录异常日志
                Log.Information("Restore_btn_Click", ex);
            }
            finally
            {
                // 恢复按钮状态
                query_value_with_cedian.Enabled = true;
                Restore_btn.Enabled = true;
                btndelete_micai.Enabled = true;
                Restore_btn.Text = "恢复数据";

            }


        }
        /// <summary>
        /// 初始化ComboBox，加载测点号和地址
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        private async Task InitializeComboBoxAsync(int typeId = 101003)
        {
            try
            {
                // 异步检查许可
                bool isValid = await CheckLicenseAsync();

                if (!isValid)
                {
                    // 许可无效，关闭程序
                    Application.Exit();
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"检查许可时发生错误: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            finally
            {
                // 恢复界面状态
                this.Enabled = true;
                this.Cursor = Cursors.Default;
            }

            try
            {
                // 清空ComboBox
                cedianhao.Items.Clear();
                cedianhao.DisplayMember = "DisplayText";
                cedianhao.ValueMember = "Value";

                // 查询数据
                //string query = "SELECT DevLabel, Address FROM `aq_main`.`f_deviceinfo` WHERE TypeId = 101003";
                string query = $@"
        SELECT DevLabel, Address
        FROM `aq_main`.`f_deviceinfo`
        WHERE TypeId = {typeId}
        
        UNION ALL
        
        SELECT 
            dmd.DevLabel,
            dmd.Address
        FROM (
            SELECT 
                DevLabel,
                Address,
                ROW_NUMBER() OVER (PARTITION BY DevLabel ORDER BY CreateTime DESC) as rn
            FROM `aq_main`.`f_devicemodifydetail` dm
            WHERE TypeId = {typeId}
            AND NOT EXISTS (
                SELECT 1 
                FROM `aq_main`.`f_deviceinfo` di 
                WHERE di.DevLabel = dm.DevLabel 
                AND di.TypeId = {typeId}
            )
        ) dmd
        WHERE dmd.rn = 1";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string devLabel = reader["DevLabel"].ToString();
                                string address = reader["Address"].ToString();

                                // 创建显示文本和值
                                string displayText = $"{devLabel} - {address}";

                                // 添加到ComboBox
                                cedianhao.Items.Add(new ComboBoxItem
                                {
                                    DisplayText = displayText,
                                    Value = devLabel
                                });
                            }
                        }
                    }
                }

                // 如果有数据，设置默认选择
                if (cedianhao.Items.Count > 0)
                {
                    cedianhao.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化ComboBox时发生错误: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private class ComboBoxItem
        {
            public string DisplayText { get; set; }
            public string Value { get; set; }

            public override string ToString()
            {
                return DisplayText;
            }
        }



        public class AppConfig
        {
            public AppConfigSection appConfig { get; set; }
        }
        public class AppConfigSection
        {
            public List<DbConn> DbConns { get; set; }
            // 其他配置项可以根据需要添加
        }
        public class DbConn
        {
            public string Key { get; set; }
            public string Type { get; set; }
            public string Conn { get; set; }
        }
        private async Task<DataTable> ExecuteQueryWithDatabaseCedianRecordAsync(string tablePrefix, string tableEnd, DateTime mintime, DateTime maxtime, string databaseName = "aq_main", double min = 0.5, double max = 100)
        {
            //tablePrefix= texboxID.Text;
            //tableEnd = startDateTimePicker.Value.ToString("yyyyMM"); ;

            DataTable resultTable = new DataTable();

            string query = $@"
                SELECT 
                     *
                     FROM `{databaseName}`.`s_analogrunrecord{tableEnd}`

                    where UniqueId like '{tablePrefix}%'  and `Value` BETWEEN {min} and {max} and  `CollectTime` BETWEEN '{mintime}' and '{maxtime}'and VStatus NOT LIKE '3758%'

                    ORDER BY `Value` ASC;";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            // 使用Task.Run将同步的Fill方法包装为异步
                            await Task.Run(() => adapter.Fill(resultTable));
                        }
                    }
                }

                Log.Information($"查询成功，返回 {resultTable.Rows.Count} 条记录");
                return resultTable;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "执行UNION查询时发生错误");
                MessageBox.Show($"查询失败: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// 删除曲线数据按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void delete_redord_Click(object sender, EventArgs e)
        {
            string condition = cedianhao.Text; // 从文本框获取删除条件
            DateTime startDate = startDateTimePicker.Value;
            DateTime endDate = endDateTimePicker.Value;
            int dayOfYear = endDateTimePicker.Value.Day;
            double maxAlarmValue;

            maxAlarmValue = double.Parse(comboBox1.Text);

            if (!minValue.TryParseDouble(out double min, "最小值") ||
                !maxValue.TryParseDouble(out double max, "最大值"))
            {
                return;
            }

            // 或者带范围验证
            if (!minValue.TryParseDouble(out min, 0, 100, "最小值") ||
                !maxValue.TryParseDouble(out max, min, 100, "最大值"))
            {
                return;
            }

            if (min >= max)
            {
                MessageBox.Show("最小值必须小于最大值");
                minValue.Focus();
                minValue.SelectAll();
                return;
            }

            if (string.IsNullOrEmpty(condition))
            {
                MessageBox.Show("请输入删除条件！");
                return;
            }
            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("数据库连接未配置!");
                return;
            }
            try
            {
                delete_redord.Enabled = false;
                query_record_btn.Enabled = false;
                Restore_record.Enabled = false;
                delete_redord.Text = "更新中...";
                var DataProcessor = new DataProcessor();
                await DataProcessor.ProcessAndUpdateDataRecordAsync(valueEncryptMapping, connectionString, (cedianhao.SelectedItem as ComboBoxItem).Value, startDateTimePicker.Value.ToString("yyyyMMdd"), startDateTimePicker.Value, endDateTimePicker.Value, "aq_main", double.Parse(minValue.Text), double.Parse(maxValue.Text), maxAlarmValue);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "查询操作失败");
                MessageBox.Show($"操作失败: {ex.Message}");
            }
            finally
            {
                // 恢复按钮状态
                delete_redord.Enabled = true;
                query_record_btn.Enabled = true;
                Restore_record.Enabled = true;
                delete_redord.Text = "更新曲线数据";
            }

        }
        /// <summary>
        /// 查询曲线数据按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void query_record_btn_Click(object sender, EventArgs e)
        {
            string condition = cedianhao.Text; // 从文本框获取删除条件
            double max;
            if (!minValue.TryParseDouble(out double min, "最小值") ||
                !maxValue.TryParseDouble(out max, "最大值"))
            {
                return;
            }

            // 或者带范围验证
            if (!minValue.TryParseDouble(out min, 0, 100, "最小值") ||
                !maxValue.TryParseDouble(out max, min, 100, "最大值"))
            {
                return;
            }

            if (min >= max)
            {
                MessageBox.Show("最小值必须小于最大值");
                minValue.Focus();
                minValue.SelectAll();
                return;
            }
            if (string.IsNullOrEmpty(condition))
            {
                MessageBox.Show("请输入删除条件！");
                return;
            }
            try
            {
                // 显示加载状态
                delete_redord.Enabled = false;
                query_record_btn.Enabled = false;
                Restore_record.Enabled = false;
                query_record_btn.Text = "查询中...";

                Log.Information("开始执行条件查询...");

                // 执行异步查询
                DataTable result = await ExecuteQueryWithDatabaseCedianRecordAsync((cedianhao.SelectedItem as ComboBoxItem).Value, startDateTimePicker.Value.ToString("yyyyMMdd"), startDateTimePicker.Value, endDateTimePicker.Value, "aq_main", double.Parse(minValue.Text), double.Parse(maxValue.Text));


                if (result != null && result.Rows.Count > 0)
                {

                    // 显示结果
                    Log.Information("查询结果:");
                    Log.Information("══════════════════════════════════");
                    Log.Information("Value\t\tValueEncrypt");
                    Log.Information("──────────────────────────────────");


                    Log.Information($"总共 {result.Rows.Count} 条记录");

                    // 如果有DataGridView，绑定数据
                    if (dataGridView1 != null)
                    {
                        dataGridView1.DataSource = result;
                    }
                }
                else
                {
                    Log.Information("未找到匹配的记录");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "查询操作失败");
                MessageBox.Show($"操作失败: {ex.Message}");
            }
            finally
            {
                // 恢复按钮状态
                delete_redord.Enabled = true;
                query_record_btn.Enabled = true;
                Restore_record.Enabled = true;
                query_record_btn.Text = "执行查询";
            }
        }
        /// <summary>
        /// 还原曲线数据按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Restore_record_Click(object sender, EventArgs e)
        {
            try
            { // 禁用按钮防止重复点击
                delete_redord.Enabled = false;
                query_record_btn.Enabled = false;
                Restore_record.Enabled = false;
                Restore_record.Text = "恢复中...";
                var selectedTable = (cedianhao.SelectedItem as ComboBoxItem)?.Value;
                // 组合表名格式：测点号_denserecord_年月
                string tableName = $"s_analogrunrecord{startDateTimePicker.Value.ToString("yyyyMMdd")}";
                var restoreService = new DataRestoreService(connectionString);

                var restoreSuccess = await restoreService.RestoreDataAsync("aq_main", tableName);

                if (restoreSuccess.Item1)
                {
                    dataGridView1.DataSource = restoreSuccess.Item2;

                    MessageBox.Show($"数据恢复成功！\n受影响记录已从备份表  恢复",
                                   "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // 可选：记录恢复日志
                    Log.Information(selectedTable, "成功");
                }
                else
                {

                    MessageBox.Show("数据恢复失败，请检查日志", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    Log.Information(selectedTable, "失败");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show($"恢复过程中发生错误：{ex.Message}", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // 记录异常日志
                Log.Information("Restore_btn_Click", ex);
            }
            finally
            {
                // 恢复按钮状态
                delete_redord.Enabled = true;
                query_record_btn.Enabled = true;
                Restore_record.Enabled = true;
                Restore_record.Text = "恢复数据";

            }
        }

        /// <summary>
        /// 报警记录按年算法查询
        /// </summary>
        /// <param name="tablePrefix"></param>
        /// <param name="tableEnd"></param>
        /// <param name="databaseName"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private async Task<DataTable> ExecuteQueryWithDatabaseCedianAlarmAsync(string tablePrefix, string tableEnd, DateTime min, DateTime max, string databaseName = "aq_main")
        {
            //tablePrefix= texboxID.Text;
            //tableEnd = startDateTimePicker.Value.ToString("yyyyMM"); ;
            //SELECT* from s_analogalarmrecord2025 WHERE VStatus in (613, 1125)  UniqueId like '006a01%'  and  StaTime >'2025-9--09
            DataTable resultTable = new DataTable();

            string query = $@"
                SELECT 
                     *
                     FROM `{databaseName}`.`s_analogalarmrecord{tableEnd}`

                    where UniqueId like '{tablePrefix}%' and `VStatus` in (613,1125) and  `UpdateTime` BETWEEN '{min}' and '{max}'

                    ORDER BY `UpdateTime` ASC";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            // 使用Task.Run将同步的Fill方法包装为异步
                            await Task.Run(() => adapter.Fill(resultTable));
                        }
                    }
                }

                Log.Information($"查询成功，返回 {resultTable.Rows.Count} 条记录");
                return resultTable;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "执行UNION查询时发生错误");
                MessageBox.Show($"查询失败: {ex.Message}");
                return null;
            }
        }

        private async void query_alarm_btn_Click(object sender, EventArgs e)
        {
            //SELECT* from s_analogalarmrecord2025 WHERE VStatus in (613, 1125)  UniqueId like '006a01%'  and  StaTime >'2025-9--09
            string condition = cedianhao.Text; // 从文本框获取删除条件
            if (string.IsNullOrEmpty(condition))
            {
                MessageBox.Show("请输入删除条件！");
                return;
            }
            query_alarm_btn.Text = "查询中...";
            query_alarm_btn.Enabled = false;
            try
            {
                // 显示加载状态
                query_alarm_btn.Text = "查询中...";
                query_alarm_btn.Enabled = false;


                Log.Information("开始执行条件查询...");

                // 执行异步查询
                DataTable result = await ExecuteQueryWithDatabaseCedianAlarmAsync((cedianhao.SelectedItem as ComboBoxItem).Value, startDateTimePicker.Value.ToString("yyyy"), startDateTimePicker.Value, endDateTimePicker.Value, "aq_main");


                if (result != null && result.Rows.Count > 0)
                {

                    // 显示结果
                    Log.Information("查询结果:");
                    Log.Information("══════════════════════════════════");
                    Log.Information("Value\t\tValueEncrypt");
                    Log.Information("──────────────────────────────────");


                    Log.Information($"总共 {result.Rows.Count} 条记录");

                    // 如果有DataGridView，绑定数据
                    if (dataGridView1 != null)
                    {
                        dataGridView1.DataSource = result;
                    }
                }
                else
                {
                    Log.Information("未找到匹配的记录");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "查询操作失败");
                MessageBox.Show($"操作失败: {ex.Message}");
            }
            finally
            {
                // 恢复按钮状态
                query_alarm_btn.Text = "查询报警";
                query_alarm_btn.Enabled = true;
            }



        }
        /// <summary>
        /// 查询5分钟统计数据按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>


        private async void delete_alarm_Click(object sender, EventArgs e)
        {
            string condition = cedianhao.Text; // 从文本框获取删除条件
            string tableName = $"s_analogalarmrecord{startDateTimePicker.Value:yyyy}";
            string uniqueIdValue = (cedianhao.SelectedItem as ComboBoxItem)?.Value ?? string.Empty;
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // 1. 先备份要删除的数据
                    string backupTableName = $"{tableName}_backup_{DateTime.Now:yyyyMMddHHmmss}";

                    string backupQuery = $@"
                        CREATE TABLE `aq_main`.`{backupTableName}` AS
                         SELECT *, NOW() as BackupTime, CURRENT_USER() as BackupUser
                         FROM `aq_main`.`{tableName}`
                         WHERE 
                    `UniqueId` LIKE @uniqueId 
                    AND `VStatus` IN (613, 1125) 
                    AND `UpdateTime` BETWEEN @startTime AND @endTime";

                    using (MySqlCommand backupCommand = new MySqlCommand(backupQuery, connection))
                    {
                        backupCommand.Parameters.AddWithValue("@uniqueId", uniqueIdValue + "%");
                        backupCommand.Parameters.AddWithValue("@startTime", startDateTimePicker.Value);
                        backupCommand.Parameters.AddWithValue("@endTime", endDateTimePicker.Value);

                        await backupCommand.ExecuteNonQueryAsync();
                        Log.Information($"备份成功，创建备份表: {backupTableName}");
                    }
                    string deleteQuery = $@"
                         DELETE FROM `aq_main`.`{tableName}`
                         WHERE 
                         `UniqueId` LIKE @uniqueId 
                         AND `VStatus` IN (613, 1125) 
                        AND `UpdateTime` BETWEEN @startTime AND @endTime";
                    using (MySqlCommand command = new MySqlCommand(deleteQuery, connection))
                    {
                        // 所有参数都使用参数化
                        command.Parameters.AddWithValue("@tableName", tableName);
                        command.Parameters.AddWithValue("@uniqueId", (cedianhao.SelectedItem as ComboBoxItem)?.Value + "%");
                        command.Parameters.AddWithValue("@startTime", startDateTimePicker.Value);
                        command.Parameters.AddWithValue("@endTime", endDateTimePicker.Value);
                        int affectedRows = await command.ExecuteNonQueryAsync();
                        Log.Information($"删除成功，影响 {affectedRows} 条记录");
                    }

                }



            }
            catch (Exception ex)
            {
                Log.Error(ex, "删除失败");
                MessageBox.Show($"删除失败: {ex.Message}");

            }

        }
        private async void Restore_alarm_Click(object sender, EventArgs e)
        {
            try
            { // 禁用按钮防止重复点击
                Restore_alarm.Enabled = false;

                Restore_alarm.Text = "恢复中...";
                var selectedTable = (cedianhao.SelectedItem as ComboBoxItem)?.Value;
                // 组合表名格式：测点号_denserecord_年月
                string tableName = $"s_analogalarmrecord{startDateTimePicker.Value:yyyy}";
                var restoreService = new DataRestoreService(connectionString);

                bool restoreSuccess = await restoreService.RestoreInsertDataAsync("aq_main", tableName);

                if (restoreSuccess)
                {

                    MessageBox.Show($"数据恢复成功！\n受影响记录已从备份表  恢复",
                                   "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // 可选：记录恢复日志
                    Log.Information(selectedTable, "成功");
                }
                else
                {

                    MessageBox.Show("数据恢复失败，请检查日志", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    Log.Information(selectedTable, "失败");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show($"恢复过程中发生错误：{ex.Message}", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // 记录异常日志
                Log.Information("Restore_btn_Click", ex);
            }
            finally
            {
                // 恢复按钮状态
                Restore_alarm.Enabled = true;

                Restore_alarm.Text = "恢复数据";
            }
        }
        private async Task<DataTable> ExecuteQueryWithDatabaseCedianMinuteAsync(string tablePrefix, string tableEnd, DateTime min, DateTime max, string databaseName = "aq_main")
        {
            // SELECT* from s_analogstaminute20250901 WHERE `Status`  != '3221225473' and UniqueId like '006a01%'  
            //tablePrefix= texboxID.Text;
            //tableEnd = startDateTimePicker.Value.ToString("yyyyMM"); ;
            //SELECT* from s_analogalarmrecord2025 WHERE VStatus in (613, 1125)  UniqueId like '006a01%'  and  StaTime >'2025-9--09

            DataTable resultTable = new DataTable();

            string query = $@"
                SELECT 
                     *
                     FROM `{databaseName}`.`s_analogstaminute{tableEnd}`

                      WHERE  UniqueId like '{tablePrefix}%' and  `UpdateTime` BETWEEN '{min}' and '{max}' 


                    ORDER BY `UpdateTime` ASC";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            // 使用Task.Run将同步的Fill方法包装为异步
                            await Task.Run(() => adapter.Fill(resultTable));
                        }
                    }
                }

                Log.Information($"查询成功，返回 {resultTable.Rows.Count} 条记录");
                return resultTable;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "执行UNION查询时发生错误");
                MessageBox.Show($"查询失败: {ex.Message}");
                return null;
            }
        }
        private async void query_5mins_btn_Click(object sender, EventArgs e)
        {
            // SELECT* from s_analogstaminute20250901 WHERE `Status`  != '3221225473' and UniqueId like '006a01%'  
            //这里的数据库分不清是标校报警还是正常报警  必须严格输入起始时间
            string condition = cedianhao.Text; // 从文本框获取删除条件
            if (string.IsNullOrEmpty(condition))
            {
                MessageBox.Show("请输入删除条件！");
                return;
            }
            query_alarm_btn.Text = "查询中...";
            query_alarm_btn.Enabled = false;
            try
            {
                // 显示加载状态
                query_alarm_btn.Text = "查询中...";
                query_alarm_btn.Enabled = false;


                Log.Information("开始执行条件查询...");

                // 执行异步查询
                DataTable result = await ExecuteQueryWithDatabaseCedianMinuteAsync((cedianhao.SelectedItem as ComboBoxItem).Value, startDateTimePicker.Value.ToString("yyyyMMdd"), startDateTimePicker.Value, endDateTimePicker.Value, "aq_main");


                if (result != null && result.Rows.Count > 0)
                {

                    // 显示结果
                    Log.Information("查询结果:");
                    Log.Information("══════════════════════════════════");
                    Log.Information("Value\t\tValueEncrypt");
                    Log.Information("──────────────────────────────────");


                    Log.Information($"总共 {result.Rows.Count} 条记录");

                    // 如果有DataGridView，绑定数据
                    if (dataGridView1 != null)
                    {
                        dataGridView1.DataSource = result;
                    }
                }
                else
                {
                    Log.Information("未找到匹配的记录");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "查询操作失败");
                MessageBox.Show($"操作失败: {ex.Message}");
            }
            finally
            {
                // 恢复按钮状态
                query_alarm_btn.Text = "查询报警";
                query_alarm_btn.Enabled = true;
            }
        }


        private async void delte_2mius_Click(object sender, EventArgs e)
        {
            // 显示确认弹窗
            DialogResult result = MessageBox.Show(
                " 重要注意事项：\n\n" +
                "1. 此操作将删除重要数据，会备份数据\n" +
                "2. 无法自动判断是否是标校所产生的数据\n" +
                "3. 删除操作必须确认 开始结束时间的选择\n" +
                "4. 此操作前一定要确认查询出来的数据是否为非标校数据\n\n" +
                "您是否知晓以上注意事项并确认要继续执行？",
                "操作确认 - 请仔细阅读",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2); // 默认选择"否"

            // 判断用户选择
            if (result == DialogResult.Yes)
            {
                // 用户点击"是"，执行下一流程
                //ExecuteDeleteOperation();
            }
            else
            {
                // 用户点击"否"，取消操作
                MessageBox.Show("操作已取消", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return; // 直接返回，不进行后续操作
            }


            string condition = cedianhao.Text; // 从文本框获取删除条件
            DateTime startDate = startDateTimePicker.Value;
            DateTime endDate = endDateTimePicker.Value;
            int dayOfYear = endDateTimePicker.Value.Day;
            double maxAlarmValue;

            maxAlarmValue = double.Parse(comboBox1.Text);


            if (string.IsNullOrEmpty(condition))
            {
                MessageBox.Show("请输入删除条件！");
                return;
            }
            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("数据库连接未配置!");
                return;
            }
            try
            {
                delte_2mius.Enabled = false;

                delete_redord.Text = "更新中...";
                var DataProcessor = new DataProcessor();
                await DataProcessor.ProcessAndUpdateDataMinusAsync(valueEncryptMapping, connectionString, (cedianhao.SelectedItem as ComboBoxItem).Value, startDateTimePicker.Value.ToString("yyyyMMdd"), startDateTimePicker.Value, endDateTimePicker.Value, "aq_main", maxAlarmValue);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "查询操作失败");
                MessageBox.Show($"操作失败: {ex.Message}");
            }
            finally
            {
                // 恢复按钮状态
                delte_2mius.Enabled = true;

                delete_redord.Text = "更新统计数据";
            }


        }

        private async void Restore_minus_Click(object sender, EventArgs e)
        {
            try
            { // 禁用按钮防止重复点击
                Restore_minus.Enabled = false;

                Restore_minus.Text = "恢复中...";
                var selectedTable = (cedianhao.SelectedItem as ComboBoxItem)?.Value;
                // 组合表名格式：测点号_denserecord_年月
                string tableName = $"s_analogstaminute{startDateTimePicker.Value:yyyyMMdd}";
                var restoreService = new DataRestoreService(connectionString);

                var restoreSuccess = await restoreService.RestoreDataAsync("aq_main", tableName);

                if (restoreSuccess.Item1)
                {

                    MessageBox.Show($"数据恢复成功！\n受影响记录已从备份表  恢复",
                                   "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // 可选：记录恢复日志
                    Log.Information(selectedTable, "成功");
                }
                else
                {

                    MessageBox.Show("数据恢复失败，请检查日志", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    Log.Information(selectedTable, "失败");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show($"恢复过程中发生错误：{ex.Message}", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // 记录异常日志
                Log.Information("Restore_btn_Click", ex);
            }
            finally
            {
                // 恢复按钮状态
                Restore_minus.Enabled = true;

                Restore_minus.Text = "还原统计数据";
            }
        }


        private async Task<bool> CheckLicenseAsync()
        {
            // 设置超时时间
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
            {
                try
                {
                    // 从数据库获取过期时间
                    DateTime? dbExpireTime = await _licenseService.GetExpireTimeFromDatabaseAsync(connectionString);

                    if (dbExpireTime == null)
                    {
                        MessageBox.Show("无法连接数据库，程序将退出。", "许可错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }



                    // 检查是否过期（与硬编码时间比较）
                    if (dbExpireTime.Value > _hardcodedExpireTime)
                    {
                        // 数据库时间超过了硬编码时间
                        ShowExpireMessage(dbExpireTime.Value, _hardcodedExpireTime);
                        return false;
                    }

                    // 额外检查：如果数据库时间已经过期（相对于当前时间）

                    // 许可有效
                    return true;
                }
                catch (OperationCanceledException)
                {
                    MessageBox.Show("检查许可超时，请检查网络连接。", "超时",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                catch (MySqlException)
                {
                    MessageBox.Show("无法连接数据库，请检查数据库服务。", "数据库错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        private void ShowExpireMessage(DateTime dbTime, DateTime hardcodedTime)
        {
            string message = $@"程序使用时间已到期！
当前时间: {dbTime:yyyy-MM-dd HH:mm:ss}
系统允许时间: {hardcodedTime:yyyy-MM-dd HH:mm:ss}

请联系管理员续费。";

            MessageBox.Show(message, "使用时间到期",
                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private async Task<DataTable> ExecuteQueryWithDatabaseCedianHoursAsync(string tablePrefix, string tableEnd, DateTime min, DateTime max, string databaseName = "aq_main")
        {
            //SELECT* from s_analogstahour2025 WHERE  UniqueId LIKE '007a02%'
            // SELECT* from s_analogstahour2025 WHERE  UniqueId LIKE '007a02%' and `Status` not like  '3221225473' and StaTime BETWEEN '2025-9-2 00:00:00' and '2025-9-2 23:59:59'
            DataTable resultTable = new DataTable();

            // 定义查询参数
            string uniqueIdPattern = $"{tablePrefix}%";
            //string statusToExclude = "3221225473";
            DateTime startTime = min;
            DateTime endTime = max;
            string tableName = $"s_analogstahour{tableEnd}"; // 表名可以根据年份动态生成

            // 构建参数化查询
            string query = $@"
             SELECT * 
             FROM  `aq_main`.`{tableName}`
             WHERE UniqueId LIKE @UniqueIdPattern 
         
            AND StaTime BETWEEN @StartTime AND @EndTime
";
            try
            {
                using MySqlConnection connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableName", tableName);
                    command.Parameters.AddWithValue("@UniqueIdPattern", uniqueIdPattern);
                    // command.Parameters.AddWithValue("@StatusToExclude", statusToExclude);
                    command.Parameters.AddWithValue("@StartTime", startTime);
                    command.Parameters.AddWithValue("@EndTime", endTime);


                    // 调试：输出最终SQL（实际执行的）
                    //string finalSql = command.CommandText;
                    //foreach (MySqlParameter param in command.Parameters)
                    //{
                    //    string value = param.Value?.ToString() ?? "NULL";
                    //    if (param.DbType == DbType.String || param.DbType == DbType.DateTime)
                    //        value = $"'{value.Replace("'", "''")}'";

                    //    finalSql = finalSql.Replace(param.ParameterName, value);
                    //}
                    //Log.Information($"最终执行的SQL: {finalSql}");
                    // 执行查询
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        // 使用Task.Run将同步的Fill方法包装为异步
                        await Task.Run(() => adapter.Fill(resultTable));
                    }

                }
                Log.Information($"查询成功，返回 {resultTable.Rows.Count} 条记录");
                return resultTable;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "执行UNION查询时发生错误");
                MessageBox.Show($"查询失败: {ex.Message}");
                return null;
            }
        }
        private async void query_hour_Click(object sender, EventArgs e)
        {
            // SELECT* from s_analogstaminute20250901 WHERE `Status`  != '3221225473' and UniqueId like '006a01%'  
            //这里的数据库分不清是标校报警还是正常报警  必须严格输入起始时间
            string condition = cedianhao.Text; // 从文本框获取删除条件
            if (string.IsNullOrEmpty(condition))
            {
                MessageBox.Show("请输入删除条件！");
                return;
            }
            query_hour.Text = "查询中...";
            query_hour.Enabled = false;
            try
            {
                // 显示加载状态
                query_hour.Text = "查询中...";
                query_hour.Enabled = false;


                Log.Information("开始执行条件查询...");

                // 执行异步查询
                DataTable result = await ExecuteQueryWithDatabaseCedianHoursAsync((cedianhao.SelectedItem as ComboBoxItem).Value, startDateTimePicker.Value.ToString("yyyy"), startDateTimePicker.Value, endDateTimePicker.Value, "aq_main");


                if (result != null && result.Rows.Count > 0)
                {

                    // 显示结果
                    Log.Information("查询结果:");
                    Log.Information("══════════════════════════════════");
                    Log.Information("Value\t\tValueEncrypt");
                    Log.Information("──────────────────────────────────");


                    Log.Information($"总共 {result.Rows.Count} 条记录");

                    // 如果有DataGridView，绑定数据
                    if (dataGridView1 != null)
                    {
                        dataGridView1.DataSource = result;
                    }
                }
                else
                {
                    Log.Information("未找到匹配的记录");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "查询操作失败");
                MessageBox.Show($"操作失败: {ex.Message}");
            }
            finally
            {
                // 恢复按钮状态
                query_hour.Text = "日报表按天小时统计";
                query_hour.Enabled = true;
            }
        }

        private async void delete_hours_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
               " 重要注意事项：\n\n" +
               "1. 此操作将删除重要数据，会备份数据\n" +
               "2. 无法自动判断是否是标校所产生的数据\n" +
               "3. 删除操作必须确认 开始结束时间的选择\n" +
               "4. 此操作前一定要确认查询出来的数据是否为非标校数据\n\n" +
               "您是否知晓以上注意事项并确认要继续执行？",
               "操作确认 - 请仔细阅读",
               MessageBoxButtons.YesNo,
               MessageBoxIcon.Warning,
               MessageBoxDefaultButton.Button2); // 默认选择"否"

            // 判断用户选择
            if (result == DialogResult.Yes)
            {
                // 用户点击"是"，执行下一流程
                //ExecuteDeleteOperation();
            }
            else
            {
                // 用户点击"否"，取消操作
                MessageBox.Show("操作已取消", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return; // 直接返回，不进行后续操作
            }


            string condition = cedianhao.Text; // 从文本框获取删除条件
            DateTime startDate = startDateTimePicker.Value;
            DateTime endDate = endDateTimePicker.Value;
            int dayOfYear = endDateTimePicker.Value.Day;
            double maxAlarmValue;

            maxAlarmValue = double.Parse(comboBox1.Text);


            if (string.IsNullOrEmpty(condition))
            {
                MessageBox.Show("请输入删除条件！");
                return;
            }
            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("数据库连接未配置!");
                return;
            }
            try
            {
                delete_hours.Enabled = false;

                delete_hours.Text = "更新中...";
                var DataProcessor = new DataProcessor();
                await DataProcessor.ProcessAndUpdateDataHoursAsync(valueEncryptMapping, connectionString, (cedianhao.SelectedItem as ComboBoxItem).Value, startDateTimePicker.Value.ToString("yyyy"), startDateTimePicker.Value, endDateTimePicker.Value, "aq_main", maxAlarmValue);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "查询操作失败");
                MessageBox.Show($"操作失败: {ex.Message}");
            }
            finally
            {
                // 恢复按钮状态
                delete_hours.Enabled = true;

                delete_hours.Text = "更新小时统计数据";
            }
        }

        private async void Restore_hour_Click(object sender, EventArgs e)
        {
            try
            { // 禁用按钮防止重复点击
                Restore_hour.Enabled = false;

                Restore_hour.Text = "恢复中...";
                var selectedTable = (cedianhao.SelectedItem as ComboBoxItem)?.Value;
                // 组合表名格式：测点号_denserecord_年月
                string tableName = $"s_analogstahour{startDateTimePicker.Value:yyyy}";
                var restoreService = new DataRestoreService(connectionString);

                var restoreSuccess = await restoreService.RestoreDataAsync("aq_main", tableName);

                if (restoreSuccess.Item1)
                {

                    MessageBox.Show($"数据恢复成功！\n受影响记录已从备份表  恢复",
                                   "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // 可选：记录恢复日志
                    Log.Information(selectedTable, "成功");
                }
                else
                {

                    MessageBox.Show("数据恢复失败，请检查日志", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    Log.Information(selectedTable, "失败");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show($"恢复过程中发生错误：{ex.Message}", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // 记录异常日志
                Log.Information("Restore_btn_Click", ex);
            }
            finally
            {
                // 恢复按钮状态
                Restore_hour.Enabled = true;

                Restore_hour.Text = "还原小时统计记录";
            }
        }

       async private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                await InitializeComboBoxAsync(102001);
            }
            else
            {
                await InitializeComboBoxAsync(101003);
            }
        
        }
    }
}



