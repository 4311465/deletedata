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
            labelResult = new Label();
            btndelete = new Button();
            texboxID = new TextBox();
            label2 = new Label();
            startDateTimePicker = new DateTimePicker();
            endDateTimePicker = new DateTimePicker();
            label1 = new Label();
            label3 = new Label();
            SuspendLayout();
            // 
            // labelResult
            // 
            labelResult.AutoSize = true;
            labelResult.Location = new Point(511, 54);
            labelResult.Name = "labelResult";
            labelResult.Size = new Size(0, 20);
            labelResult.TabIndex = 0;
            // 
            // btndelete
            // 
            btndelete.Location = new Point(301, 200);
            btndelete.Name = "btndelete";
            btndelete.Size = new Size(94, 29);
            btndelete.TabIndex = 1;
            btndelete.Text = "删除";
            btndelete.UseVisualStyleBackColor = true;
            btndelete.Click += btndelete_Click;
            // 
            // texboxID
            // 
            texboxID.Location = new Point(128, 202);
            texboxID.Name = "texboxID";
            texboxID.Size = new Size(125, 27);
            texboxID.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(39, 209);
            label2.Name = "label2";
            label2.Size = new Size(39, 20);
            label2.TabIndex = 3;
            label2.Text = "卡号";
            // 
            // startDateTimePicker
            // 
            startDateTimePicker.Location = new Point(85, 282);
            startDateTimePicker.Name = "startDateTimePicker";
            startDateTimePicker.Size = new Size(250, 27);
            startDateTimePicker.TabIndex = 4;
            // 
            // endDateTimePicker
            // 
            endDateTimePicker.Location = new Point(85, 341);
            endDateTimePicker.Name = "endDateTimePicker";
            endDateTimePicker.Size = new Size(250, 27);
            endDateTimePicker.TabIndex = 5;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(20, 288);
            label1.Name = "label1";
            label1.Size = new Size(69, 20);
            label1.TabIndex = 6;
            label1.Text = "开始时间";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(20, 347);
            label3.Name = "label3";
            label3.Size = new Size(69, 20);
            label3.TabIndex = 7;
            label3.Text = "结束时间";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(882, 454);
            Controls.Add(label3);
            Controls.Add(label1);
            Controls.Add(endDateTimePicker);
            Controls.Add(startDateTimePicker);
            Controls.Add(label2);
            Controls.Add(texboxID);
            Controls.Add(btndelete);
            Controls.Add(labelResult);
            Name = "Form1";
            Text = "人员定位脏数据清除";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelResult;
        private Button btndelete;
        private TextBox texboxID;
        private Label label2;
        private DateTimePicker startDateTimePicker;
        private DateTimePicker endDateTimePicker;
        private Label label1;
        private Label label3;
    }
}
