namespace deletedata
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            labelResult = new Label();
            btndelete_micai = new Button();
            label2 = new Label();
            startDateTimePicker = new DateTimePicker();
            endDateTimePicker = new DateTimePicker();
            label1 = new Label();
            label3 = new Label();
            richTextBoxLogControl1 = new Serilog.Sinks.WinForms.Core.RichTextBoxLogControl();
            btnExecuteQuery = new Button();
            dataGridView1 = new DataGridView();
            query_value_with_cedian = new Button();
            minValue = new TextBox();
            label5 = new Label();
            delete_alarm = new Button();
            label6 = new Label();
            maxValue = new TextBox();
            cedianhao = new ComboBox();
            Restore_btn = new Button();
            delete_redord = new Button();
            delte_2mius = new Button();
            Restore_record = new Button();
            Restore_alarm = new Button();
            query_record_btn = new Button();
            query_alarm_btn = new Button();
            query_5mins_btn = new Button();
            label4 = new Label();
            comboBox1 = new ComboBox();
            label7 = new Label();
            label8 = new Label();
            label9 = new Label();
            label10 = new Label();
            toolTip1 = new ToolTip(components);
            Restore_minus = new Button();
            Restore_hour = new Button();
            label11 = new Label();
            query_hour = new Button();
            delete_hours = new Button();
            checkBox1 = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // labelResult
            // 
            labelResult.AutoSize = true;
            labelResult.Location = new Point(325, 46);
            labelResult.Margin = new Padding(2, 0, 2, 0);
            labelResult.Name = "labelResult";
            labelResult.Size = new Size(0, 17);
            labelResult.TabIndex = 0;
            // 
            // btndelete_micai
            // 
            btndelete_micai.Location = new Point(616, 80);
            btndelete_micai.Margin = new Padding(2, 3, 2, 3);
            btndelete_micai.Name = "btndelete_micai";
            btndelete_micai.Size = new Size(123, 25);
            btndelete_micai.TabIndex = 1;
            btndelete_micai.Text = "更新密采数据";
            btndelete_micai.UseVisualStyleBackColor = true;
            btndelete_micai.Click += btndelete_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(16, 30);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(44, 17);
            label2.TabIndex = 3;
            label2.Text = "测点号";
            // 
            // startDateTimePicker
            // 
            startDateTimePicker.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            startDateTimePicker.Format = DateTimePickerFormat.Custom;
            startDateTimePicker.Location = new Point(91, 84);
            startDateTimePicker.Margin = new Padding(2, 3, 2, 3);
            startDateTimePicker.Name = "startDateTimePicker";
            startDateTimePicker.Size = new Size(195, 23);
            startDateTimePicker.TabIndex = 4;
            // 
            // endDateTimePicker
            // 
            endDateTimePicker.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            endDateTimePicker.Format = DateTimePickerFormat.Custom;
            endDateTimePicker.Location = new Point(386, 84);
            endDateTimePicker.Margin = new Padding(2, 3, 2, 3);
            endDateTimePicker.Name = "endDateTimePicker";
            endDateTimePicker.Size = new Size(195, 23);
            endDateTimePicker.TabIndex = 5;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(11, 89);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(56, 17);
            label1.TabIndex = 6;
            label1.Text = "开始时间";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(309, 88);
            label3.Margin = new Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new Size(56, 17);
            label3.TabIndex = 7;
            label3.Text = "结束时间";
            // 
            // richTextBoxLogControl1
            // 
            richTextBoxLogControl1.AutoPurge = false;
            richTextBoxLogControl1.AutoPurgeTime = 60D;
            richTextBoxLogControl1.ForContext = "";
            richTextBoxLogControl1.Location = new Point(-3, 443);
            richTextBoxLogControl1.Name = "richTextBoxLogControl1";
            richTextBoxLogControl1.Size = new Size(1331, 240);
            richTextBoxLogControl1.TabIndex = 8;
            richTextBoxLogControl1.Text = "";
            // 
            // btnExecuteQuery
            // 
            btnExecuteQuery.Location = new Point(337, 133);
            btnExecuteQuery.Name = "btnExecuteQuery";
            btnExecuteQuery.Size = new Size(81, 27);
            btnExecuteQuery.TabIndex = 9;
            btnExecuteQuery.Text = "存储密码映射";
            btnExecuteQuery.UseVisualStyleBackColor = true;
            btnExecuteQuery.Click += btnExecuteQuery_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(11, 166);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(1317, 271);
            dataGridView1.TabIndex = 10;
            // 
            // query_value_with_cedian
            // 
            query_value_with_cedian.Location = new Point(616, 46);
            query_value_with_cedian.Name = "query_value_with_cedian";
            query_value_with_cedian.Size = new Size(123, 23);
            query_value_with_cedian.TabIndex = 13;
            query_value_with_cedian.Text = "查询密采的数据";
            query_value_with_cedian.UseVisualStyleBackColor = true;
            query_value_with_cedian.Click += query_value_with_cedian_Click;
            // 
            // minValue
            // 
            minValue.Location = new Point(411, 29);
            minValue.Name = "minValue";
            minValue.Size = new Size(73, 23);
            minValue.TabIndex = 14;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(349, 32);
            label5.Name = "label5";
            label5.Size = new Size(56, 17);
            label5.TabIndex = 15;
            label5.Text = "大于等于";
            // 
            // delete_alarm
            // 
            delete_alarm.Location = new Point(916, 80);
            delete_alarm.Name = "delete_alarm";
            delete_alarm.Size = new Size(115, 23);
            delete_alarm.TabIndex = 16;
            delete_alarm.Text = "删除对应报警记录";
            delete_alarm.UseVisualStyleBackColor = true;
            delete_alarm.Click += delete_alarm_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(490, 32);
            label6.Name = "label6";
            label6.Size = new Size(32, 17);
            label6.TabIndex = 17;
            label6.Text = "小于";
            // 
            // maxValue
            // 
            maxValue.Location = new Point(528, 29);
            maxValue.Name = "maxValue";
            maxValue.Size = new Size(63, 23);
            maxValue.TabIndex = 18;
            // 
            // cedianhao
            // 
            cedianhao.FormattingEnabled = true;
            cedianhao.Location = new Point(65, 29);
            cedianhao.Name = "cedianhao";
            cedianhao.Size = new Size(179, 25);
            cedianhao.TabIndex = 19;
            // 
            // Restore_btn
            // 
            Restore_btn.Location = new Point(630, 123);
            Restore_btn.Name = "Restore_btn";
            Restore_btn.Size = new Size(84, 23);
            Restore_btn.TabIndex = 20;
            Restore_btn.Text = "还原密采数据";
            Restore_btn.UseVisualStyleBackColor = true;
            Restore_btn.Click += Restore_btn_Click;
            // 
            // delete_redord
            // 
            delete_redord.Location = new Point(782, 80);
            delete_redord.Name = "delete_redord";
            delete_redord.Size = new Size(91, 23);
            delete_redord.TabIndex = 21;
            delete_redord.Text = "更新曲线数据";
            delete_redord.UseVisualStyleBackColor = true;
            delete_redord.Click += delete_redord_Click;
            // 
            // delte_2mius
            // 
            delte_2mius.ForeColor = Color.Red;
            delte_2mius.Location = new Point(1060, 84);
            delte_2mius.Name = "delte_2mius";
            delte_2mius.Size = new Size(128, 23);
            delte_2mius.TabIndex = 22;
            delte_2mius.Text = "更新五分钟统计记录";
            delte_2mius.UseVisualStyleBackColor = true;
            delte_2mius.Click += delte_2mius_Click;
            // 
            // Restore_record
            // 
            Restore_record.Location = new Point(788, 121);
            Restore_record.Name = "Restore_record";
            Restore_record.Size = new Size(84, 23);
            Restore_record.TabIndex = 23;
            Restore_record.Text = "还原曲线数据";
            Restore_record.UseVisualStyleBackColor = true;
            Restore_record.Click += Restore_record_Click;
            // 
            // Restore_alarm
            // 
            Restore_alarm.CausesValidation = false;
            Restore_alarm.Location = new Point(930, 121);
            Restore_alarm.Name = "Restore_alarm";
            Restore_alarm.Size = new Size(84, 23);
            Restore_alarm.TabIndex = 24;
            Restore_alarm.Text = "还原报警数据";
            Restore_alarm.UseVisualStyleBackColor = true;
            Restore_alarm.Click += Restore_alarm_Click;
            // 
            // query_record_btn
            // 
            query_record_btn.Location = new Point(788, 47);
            query_record_btn.Name = "query_record_btn";
            query_record_btn.Size = new Size(75, 23);
            query_record_btn.TabIndex = 26;
            query_record_btn.Text = "查询曲线数据";
            query_record_btn.UseVisualStyleBackColor = true;
            query_record_btn.Click += query_record_btn_Click;
            // 
            // query_alarm_btn
            // 
            query_alarm_btn.Location = new Point(939, 46);
            query_alarm_btn.Name = "query_alarm_btn";
            query_alarm_btn.Size = new Size(75, 23);
            query_alarm_btn.TabIndex = 27;
            query_alarm_btn.Text = "查询报警";
            query_alarm_btn.UseVisualStyleBackColor = true;
            query_alarm_btn.Click += query_alarm_btn_Click;
            // 
            // query_5mins_btn
            // 
            query_5mins_btn.Location = new Point(1067, 47);
            query_5mins_btn.Name = "query_5mins_btn";
            query_5mins_btn.Size = new Size(121, 23);
            query_5mins_btn.TabIndex = 28;
            query_5mins_btn.Text = "查询5分钟统计记录";
            query_5mins_btn.UseVisualStyleBackColor = true;
            query_5mins_btn.Click += query_5mins_btn_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(11, 138);
            label4.Name = "label4";
            label4.Size = new Size(176, 17);
            label4.TabIndex = 30;
            label4.Text = "修改最大值建议根据报警值设定";
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "0.1", "0.2", "0.3", "0.4", "0.5", "0.55", "0.6" });
            comboBox1.Location = new Point(204, 135);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(98, 25);
            comboBox1.TabIndex = 31;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(616, 17);
            label7.Name = "label7";
            label7.Size = new Size(137, 17);
            label7.TabIndex = 32;
            label7.Text = "密采数据按测点+月份表";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(768, 18);
            label8.Name = "label8";
            label8.Size = new Size(104, 17);
            label8.TabIndex = 33;
            label8.Text = "曲线数据按天分表";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(930, 15);
            label9.Name = "label9";
            label9.Size = new Size(92, 17);
            label9.TabIndex = 34;
            label9.Text = "报警是按年存储";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(1085, 15);
            label10.Name = "label10";
            label10.Size = new Size(92, 17);
            label10.TabIndex = 35;
            label10.Text = "统计表按天存储";
            // 
            // Restore_minus
            // 
            Restore_minus.Location = new Point(1067, 123);
            Restore_minus.Name = "Restore_minus";
            Restore_minus.Size = new Size(110, 23);
            Restore_minus.TabIndex = 36;
            Restore_minus.Text = "还原统计数据";
            Restore_minus.UseVisualStyleBackColor = true;
            Restore_minus.Click += Restore_minus_Click;
            // 
            // Restore_hour
            // 
            Restore_hour.Location = new Point(1218, 123);
            Restore_hour.Name = "Restore_hour";
            Restore_hour.Size = new Size(110, 23);
            Restore_hour.TabIndex = 41;
            Restore_hour.Text = "还原小时统计记录";
            Restore_hour.UseVisualStyleBackColor = true;
            Restore_hour.Click += Restore_hour_Click;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(1236, 15);
            label11.Name = "label11";
            label11.Size = new Size(80, 17);
            label11.TabIndex = 40;
            label11.Text = "模拟量日报表";
            // 
            // query_hour
            // 
            query_hour.Location = new Point(1218, 47);
            query_hour.Name = "query_hour";
            query_hour.Size = new Size(121, 23);
            query_hour.TabIndex = 39;
            query_hour.Text = "日报表按天小时统计";
            query_hour.UseVisualStyleBackColor = true;
            query_hour.Click += query_hour_Click;
            // 
            // delete_hours
            // 
            delete_hours.ForeColor = Color.Red;
            delete_hours.Location = new Point(1211, 84);
            delete_hours.Name = "delete_hours";
            delete_hours.Size = new Size(128, 23);
            delete_hours.TabIndex = 38;
            delete_hours.Text = "更新小时统计数据";
            delete_hours.UseVisualStyleBackColor = true;
            delete_hours.Click += delete_hours_Click;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Checked = true;
            checkBox1.CheckState = CheckState.Checked;
            checkBox1.Location = new Point(254, 33);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(75, 21);
            checkBox1.TabIndex = 42;
            checkBox1.Text = "一氧化碳";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1340, 680);
            Controls.Add(checkBox1);
            Controls.Add(Restore_hour);
            Controls.Add(label11);
            Controls.Add(query_hour);
            Controls.Add(delete_hours);
            Controls.Add(Restore_minus);
            Controls.Add(label10);
            Controls.Add(label9);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(comboBox1);
            Controls.Add(label4);
            Controls.Add(query_5mins_btn);
            Controls.Add(query_alarm_btn);
            Controls.Add(query_record_btn);
            Controls.Add(Restore_alarm);
            Controls.Add(Restore_record);
            Controls.Add(delte_2mius);
            Controls.Add(delete_redord);
            Controls.Add(Restore_btn);
            Controls.Add(cedianhao);
            Controls.Add(maxValue);
            Controls.Add(label6);
            Controls.Add(delete_alarm);
            Controls.Add(label5);
            Controls.Add(minValue);
            Controls.Add(query_value_with_cedian);
            Controls.Add(dataGridView1);
            Controls.Add(btnExecuteQuery);
            Controls.Add(richTextBoxLogControl1);
            Controls.Add(label3);
            Controls.Add(label1);
            Controls.Add(endDateTimePicker);
            Controls.Add(startDateTimePicker);
            Controls.Add(label2);
            Controls.Add(btndelete_micai);
            Controls.Add(labelResult);
            Margin = new Padding(2, 3, 2, 3);
            Name = "Form1";
            Text = "人员定位脏数据清除v1.1";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelResult;
        private Button btndelete_micai;
        private Label label2;
        private DateTimePicker startDateTimePicker;
        private DateTimePicker endDateTimePicker;
        private Label label1;
        private Label label3;
        private Serilog.Sinks.WinForms.Core.RichTextBoxLogControl richTextBoxLogControl1;
        private Button btnExecuteQuery;
        private DataGridView dataGridView1;
        private Button query_value_with_cedian;
        private TextBox minValue;
        private Label label5;
        private Button delete_alarm;
        private Label label6;
        private TextBox maxValue;
        private ComboBox cedianhao;
        private Button Restore_btn;
        private Button delete_redord;
        private Button delte_2mius;
        private Button Restore_record;
        private Button Restore_alarm;

        private Button query_record_btn;
        private Button query_alarm_btn;
        private Button query_5mins_btn;
        private Label label4;
        private ComboBox comboBox1;
        private Label label7;
        private Label label8;
        private Label label9;
        private Label label10;
        private ToolTip toolTip1;
        private Button Restore_minus;
        private Button Restore_hour;
        private Label label11;
        private Button query_hour;
        private Button delete_hours;
        private CheckBox checkBox1;
    }
}
