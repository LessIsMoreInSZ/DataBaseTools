namespace DataBaseTools
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
            button1 = new Button();
            label1 = new Label();
            btnDelete = new Button();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(25, 37);
            button1.Name = "button1";
            button1.Size = new Size(222, 181);
            button1.TabIndex = 0;
            button1.Text = "点我哦~只优化速度版";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(1, 277);
            label1.Name = "label1";
            label1.Size = new Size(573, 24);
            label1.TabIndex = 1;
            label1.Text = "一台机使用一次皆可。使用时关闭ajyapp那个程序，出现执行成功即可";
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(327, 37);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(222, 181);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "删剩最新5000条数据";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(580, 372);
            Controls.Add(btnDelete);
            Controls.Add(label1);
            Controls.Add(button1);
            Name = "Form1";
            Text = "数据库优化工具";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Label label1;
        private Button btnDelete;
    }
}
