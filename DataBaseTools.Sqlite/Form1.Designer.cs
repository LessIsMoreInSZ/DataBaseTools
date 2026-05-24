namespace DataBaseTools
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            labelTitle = new Label();
            labelDbPath = new Label();
            txtDbPath = new TextBox();
            btnBrowseDb = new Button();
            labelTableName = new Label();
            txtTableName = new TextBox();
            labelIdColumn = new Label();
            txtIdColumn = new TextBox();
            labelIndexColumn = new Label();
            txtIndexColumn = new TextBox();
            labelKeepRows = new Label();
            numKeepRows = new NumericUpDown();
            btnCreateIndex = new Button();
            btnDeleteOldRows = new Button();
            lblTips = new Label();
            ((System.ComponentModel.ISupportInitialize)numKeepRows).BeginInit();
            SuspendLayout();
            // 
            // labelTitle
            // 
            labelTitle.AutoSize = true;
            labelTitle.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            labelTitle.Location = new Point(28, 22);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(344, 31);
            labelTitle.TabIndex = 0;
            labelTitle.Text = "SQLite 数据维护工具 (.NET 6)";
            // 
            // labelDbPath
            // 
            labelDbPath.AutoSize = true;
            labelDbPath.Location = new Point(32, 80);
            labelDbPath.Name = "labelDbPath";
            labelDbPath.Size = new Size(100, 24);
            labelDbPath.TabIndex = 1;
            labelDbPath.Text = "数据库文件";
            // 
            // txtDbPath
            // 
            txtDbPath.Location = new Point(150, 77);
            txtDbPath.Name = "txtDbPath";
            txtDbPath.Size = new Size(520, 30);
            txtDbPath.TabIndex = 2;
            // 
            // btnBrowseDb
            // 
            btnBrowseDb.Location = new Point(688, 75);
            btnBrowseDb.Name = "btnBrowseDb";
            btnBrowseDb.Size = new Size(96, 34);
            btnBrowseDb.TabIndex = 3;
            btnBrowseDb.Text = "浏览...";
            btnBrowseDb.UseVisualStyleBackColor = true;
            btnBrowseDb.Click += btnBrowseDb_Click;
            // 
            // labelTableName
            // 
            labelTableName.AutoSize = true;
            labelTableName.Location = new Point(32, 132);
            labelTableName.Name = "labelTableName";
            labelTableName.Size = new Size(64, 24);
            labelTableName.TabIndex = 4;
            labelTableName.Text = "表名";
            // 
            // txtTableName
            // 
            txtTableName.Location = new Point(150, 129);
            txtTableName.Name = "txtTableName";
            txtTableName.Size = new Size(180, 30);
            txtTableName.TabIndex = 5;
            // 
            // labelIdColumn
            // 
            labelIdColumn.AutoSize = true;
            labelIdColumn.Location = new Point(360, 132);
            labelIdColumn.Name = "labelIdColumn";
            labelIdColumn.Size = new Size(82, 24);
            labelIdColumn.TabIndex = 6;
            labelIdColumn.Text = "排序列名";
            // 
            // txtIdColumn
            // 
            txtIdColumn.Location = new Point(458, 129);
            txtIdColumn.Name = "txtIdColumn";
            txtIdColumn.Size = new Size(160, 30);
            txtIdColumn.TabIndex = 7;
            // 
            // labelIndexColumn
            // 
            labelIndexColumn.AutoSize = true;
            labelIndexColumn.Location = new Point(32, 184);
            labelIndexColumn.Name = "labelIndexColumn";
            labelIndexColumn.Size = new Size(100, 24);
            labelIndexColumn.TabIndex = 8;
            labelIndexColumn.Text = "索引目标列";
            // 
            // txtIndexColumn
            // 
            txtIndexColumn.Location = new Point(150, 181);
            txtIndexColumn.Name = "txtIndexColumn";
            txtIndexColumn.Size = new Size(180, 30);
            txtIndexColumn.TabIndex = 9;
            // 
            // labelKeepRows
            // 
            labelKeepRows.AutoSize = true;
            labelKeepRows.Location = new Point(360, 184);
            labelKeepRows.Name = "labelKeepRows";
            labelKeepRows.Size = new Size(82, 24);
            labelKeepRows.TabIndex = 10;
            labelKeepRows.Text = "保留行数";
            // 
            // numKeepRows
            // 
            numKeepRows.Location = new Point(458, 181);
            numKeepRows.Maximum = new decimal(new int[] { 100000000, 0, 0, 0 });
            numKeepRows.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numKeepRows.Name = "numKeepRows";
            numKeepRows.Size = new Size(160, 30);
            numKeepRows.TabIndex = 11;
            numKeepRows.Value = new decimal(new int[] { 5000, 0, 0, 0 });
            // 
            // btnCreateIndex
            // 
            btnCreateIndex.Location = new Point(150, 242);
            btnCreateIndex.Name = "btnCreateIndex";
            btnCreateIndex.Size = new Size(220, 56);
            btnCreateIndex.TabIndex = 12;
            btnCreateIndex.Text = "检查并创建索引";
            btnCreateIndex.UseVisualStyleBackColor = true;
            btnCreateIndex.Click += btnCreateIndex_Click;
            // 
            // btnDeleteOldRows
            // 
            btnDeleteOldRows.Location = new Point(398, 242);
            btnDeleteOldRows.Name = "btnDeleteOldRows";
            btnDeleteOldRows.Size = new Size(220, 56);
            btnDeleteOldRows.TabIndex = 13;
            btnDeleteOldRows.Text = "删除旧数据并整理";
            btnDeleteOldRows.UseVisualStyleBackColor = true;
            btnDeleteOldRows.Click += btnDeleteOldRows_Click;
            // 
            // lblTips
            // 
            lblTips.Location = new Point(32, 316);
            lblTips.Name = "lblTips";
            lblTips.Size = new Size(752, 49);
            lblTips.TabIndex = 14;
            lblTips.Text = "提示信息";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(820, 384);
            Controls.Add(lblTips);
            Controls.Add(btnDeleteOldRows);
            Controls.Add(btnCreateIndex);
            Controls.Add(numKeepRows);
            Controls.Add(labelKeepRows);
            Controls.Add(txtIndexColumn);
            Controls.Add(labelIndexColumn);
            Controls.Add(txtIdColumn);
            Controls.Add(labelIdColumn);
            Controls.Add(txtTableName);
            Controls.Add(labelTableName);
            Controls.Add(btnBrowseDb);
            Controls.Add(txtDbPath);
            Controls.Add(labelDbPath);
            Controls.Add(labelTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SQLite 数据维护工具";
            ((System.ComponentModel.ISupportInitialize)numKeepRows).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelTitle;
        private Label labelDbPath;
        private TextBox txtDbPath;
        private Button btnBrowseDb;
        private Label labelTableName;
        private TextBox txtTableName;
        private Label labelIdColumn;
        private TextBox txtIdColumn;
        private Label labelIndexColumn;
        private TextBox txtIndexColumn;
        private Label labelKeepRows;
        private NumericUpDown numKeepRows;
        private Button btnCreateIndex;
        private Button btnDeleteOldRows;
        private Label lblTips;
    }
}
