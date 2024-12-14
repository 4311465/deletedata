

using Microsoft.Data.SqlClient;

namespace deletedata
{


    public partial class Form1 : Form
    {

        private string connectionString = "Server=localhost,1433;Database=UWB_DataTable;User Id=sa;Password=hnma@1972;TrustServerCertificate=True;";

        public Form1()
        {
            InitializeComponent();
            //startDateTimePicker.Format = DateTimePickerFormat.Short;
            //endDateTimePicker.Format = DateTimePickerFormat.Short;
            startDateTimePicker.Format = DateTimePickerFormat.Custom;
            startDateTimePicker.CustomFormat = "yyyy/MM/dd HH:mm"; // 自定义格式

            endDateTimePicker.Format = DateTimePickerFormat.Custom;
            endDateTimePicker.CustomFormat = "yyyy/MM/dd HH:mm"; // 自定义格式

            //startDateTimePicker.Value = DateTime.Now; // 设置为当前时间
            startDateTimePicker.Value = DateTime.Now.AddDays(-1);
            endDateTimePicker.Value = DateTime.Now;
        }
        //private void btnDelete_Click(object sender, EventArgs e)
        //{
        //    string condition = txtCondition.Text;

        //    // 获取用户选择的时间
        //    DateTime startDate = startDateTimePicker.Value;
        //    DateTime endDate = endDateTimePicker.Value;

        //    if (string.IsNullOrEmpty(condition))
        //    {
        //        MessageBox.Show("请输入删除条件！");
        //        return;
        //    }

        //    // 检查时间范围
        //    if (endDate < startDate)
        //    {
        //        MessageBox.Show("结束时间不能早于开始时间！");
        //        return;
        //    }

        //    using (SqlConnection connection = new SqlConnection(connectionString))
        //    {
        //        connection.Open();
        //        using (SqlTransaction transaction = connection.BeginTransaction())
        //        {
        //            try
        //            {
        //                string[] tables = { "Table1", "Table2", "Table3" }; // 替换为你的表名
        //                lblResult.Text = ""; // 清空以前的结果

        //                foreach (string table in tables)
        //                {
        //                    // 修改查询，以包括时间条件
        //                    string query = $"DELETE FROM {table} WHERE YourConditionColumn = @Condition " +
        //                                   $"AND YourDateColumn BETWEEN @StartDate AND @EndDate"; // 替换为你的条件列和日期列
        //                    using (SqlCommand command = new SqlCommand(query, connection, transaction))
        //                    {
        //                        command.Parameters.AddWithValue("@Condition", condition);
        //                        command.Parameters.AddWithValue("@StartDate", startDate);
        //                        command.Parameters.AddWithValue("@EndDate", endDate);
        //                        int rowsAffected = command.ExecuteNonQuery();

        //                        // 使用 Debug.WriteLine 输出调试信息
        //                        System.Diagnostics.Debug.WriteLine($"表 {table} 中已删除 {rowsAffected} 条记录。");

        //                        // 更新 Label 显示删除的信息
        //                        if (rowsAffected > 0)
        //                        {
        //                            lblResult.Text += $"表 {table} 中已删除 {rowsAffected} 条记录。\n";
        //                        }
        //                        else
        //                        {
        //                            lblResult.Text += $"表 {table} 中未找到符合条件的记录。\n";
        //                        }
        //                    }
        //                }

        //                transaction.Commit();
        //                MessageBox.Show("删除成功！");
        //            }
        //            catch (Exception ex)
        //            {
        //                transaction.Rollback();
        //                MessageBox.Show("发生错误: " + ex.Message);
        //            }
        //        }
        //    }
        //}

        private void btndelete_Click(object sender, EventArgs e)
        {
            string condition = texboxID.Text; // 从文本框获取删除条件
            DateTime startDate = startDateTimePicker.Value;
            DateTime endDate = endDateTimePicker.Value;

            if (string.IsNullOrEmpty(condition))
            {
                MessageBox.Show("请输入删除条件！");
                return;
            }

            //using (SqlConnection connection = new SqlConnection(connectionString))
            //{
            //    string query = "DELETE FROM Pub_SysMenu WHERE MenuName = @Condition"; // 替换为你的表名和条件列
            //    using (SqlCommand command = new SqlCommand(query, connection))
            //    {
            //        command.Parameters.AddWithValue("@Condition", condition);

            //        try
            //        {
            //            connection.Open();
            //            int rowsAffected = command.ExecuteNonQuery();

            //            if (rowsAffected > 0)
            //            {
            //                labelResult.Text = "删除成功！";
            //            }
            //            else
            //            {
            //                labelResult.Text = "未找到符合条件的记录。";
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            MessageBox.Show("发生错误: " + ex.Message);
            //        }
            //    }
            //}
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 在此处执行多个 DELETE 命令
                        string[] tables = { "RY_Att_Report_Day", "RY_Att_Report_InOut", "RY_Att_Report_InOut_History",
                            "RY_Att_Report_Month", "RY_ChaoShi", "RY_ChaoShi_History",
                       "RY_ChaoShi_History", "RY_ChaoYuan", "RY_ChaoYuan_History",
                        "RY_Data_History", "RY_IllegalInside", "RY_IllegalInside_History",
                        "RY_InOutSite", "RY_InOutSite_History", "RY_InOutArea_History","RY_InOutArea"}; // 替换为你的表名

                        //string query = $"DELETE FROM {table} WHERE YourConditionColumn = @Condition " +
                                       //    $"AND YourDateColumn BETWEEN @StartDate AND @EndDate"; // 替换为你的条件列和日期列


                        foreach (string table in tables)

                        {
                            string query = $"DELETE FROM {table} WHERE cardId = @Condition "+
                                $"AND CheckTime BETWEEN @StartDate AND @EndDate"; // 替换为你的条件列
                            using (SqlCommand command = new SqlCommand(query, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@Condition", condition);
                                command.Parameters.AddWithValue("@StartDate", startDate);
                                command.Parameters.AddWithValue("@EndDate", endDate);
                                int rowsAffected = command.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    labelResult.Text += $"表 {table} 中已删除 {rowsAffected} 条记录。\n";
                                }
                                else
                                {
                                    labelResult.Text += $"表 {table} 中未找到符合条件的记录。\n";
                                }
                            }
                        }

                        // 提交事务
                        transaction.Commit();
                        MessageBox.Show("删除成功！");
                    }
                    catch (Exception ex)
                    {
                        // 当发生错误时，回滚事务
                        transaction.Rollback();
                        MessageBox.Show("发生错误: " + ex.Message);
                    }
                }
            }
        }
    }
}
