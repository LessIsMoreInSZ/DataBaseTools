using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DataBaseTools
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            txtDbPath.Text = @"C:\Data\Application.db";
            txtTableName.Text = "Products";
            txtIdColumn.Text = "Id";
            txtIndexColumn.Text = "CreateTime";
            numKeepRows.Value = 5000;
            lblTips.Text = "使用前请先关闭 AJY App 或其他占用 SQLite 文件的程序，再执行删除或 VACUUM。";
        }

        private void btnBrowseDb_Click(object sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "SQLite Database|*.db;*.sqlite;*.sqlite3|All Files|*.*";
            dialog.Title = "选择 SQLite 数据库文件";

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                txtDbPath.Text = dialog.FileName;
                lblTips.Text = "已选择数据库文件：" + dialog.FileName;
            }
        }

        private void btnCreateIndex_Click(object sender, EventArgs e)
        {
            if (!TryGetValidatedInputs(out string dbPath, out string tableName, out string idColumn, out string indexColumn, out int keepRows))
            {
                return;
            }

            try
            {
                using SqliteConnection connection = CreateConnection(dbPath);
                connection.Open();

                string indexName = $"IX_{tableName}_{indexColumn}";
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = $"CREATE INDEX IF NOT EXISTS {QuoteIdentifier(indexName)} ON {QuoteIdentifier(tableName)} ({QuoteIdentifier(indexColumn)});";
                command.ExecuteNonQuery();

                lblTips.Text = $"索引创建完成：{indexName}。当前保留策略仍是最新 {keepRows} 条，排序列为 {idColumn}。";
                MessageBox.Show(this, $"执行成功，已检查或创建索引：{indexName}", "操作成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "索引处理失败：" + ex.Message, "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteOldRows_Click(object sender, EventArgs e)
        {
            if (!TryGetValidatedInputs(out string dbPath, out string tableName, out string idColumn, out _, out int keepRows))
            {
                return;
            }

            DialogResult result = MessageBox.Show(
                this,
                $"确定要删除 {tableName} 的旧记录，仅保留最新 {keepRows} 条吗？",
                "删除确认",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                using SqliteConnection connection = CreateConnection(dbPath);
                connection.Open();

                int affectedRows;
                using (SqliteCommand deleteCommand = connection.CreateCommand())
                {
                    deleteCommand.CommandText = $@"
DELETE FROM {QuoteIdentifier(tableName)}
WHERE {QuoteIdentifier(idColumn)} NOT IN (
    SELECT {QuoteIdentifier(idColumn)}
    FROM {QuoteIdentifier(tableName)}
    ORDER BY {QuoteIdentifier(idColumn)} DESC
    LIMIT $keepRows
);";
                    deleteCommand.Parameters.AddWithValue("$keepRows", keepRows);
                    affectedRows = deleteCommand.ExecuteNonQuery();
                }

                using (SqliteCommand vacuumCommand = connection.CreateCommand())
                {
                    vacuumCommand.CommandText = "VACUUM;";
                    vacuumCommand.ExecuteNonQuery();
                }

                lblTips.Text = $"删除完成：{tableName} 保留最新 {keepRows} 条，清理了 {affectedRows} 条旧记录，并执行了 VACUUM。";
                MessageBox.Show(this, "删除成功，并已完成数据库整理。", "操作成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "删除失败：" + ex.Message, "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool TryGetValidatedInputs(
            out string dbPath,
            out string tableName,
            out string idColumn,
            out string indexColumn,
            out int keepRows)
        {
            dbPath = txtDbPath.Text.Trim();
            tableName = txtTableName.Text.Trim();
            idColumn = txtIdColumn.Text.Trim();
            indexColumn = txtIndexColumn.Text.Trim();
            keepRows = decimal.ToInt32(numKeepRows.Value);

            if (string.IsNullOrWhiteSpace(dbPath))
            {
                MessageBox.Show(this, "请先选择数据库文件。", "参数缺失", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!File.Exists(dbPath))
            {
                MessageBox.Show(this, "数据库文件不存在，请重新选择。", "路径错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!IsSafeIdentifier(tableName) || !IsSafeIdentifier(idColumn) || !IsSafeIdentifier(indexColumn))
            {
                MessageBox.Show(this, "表名和列名只允许字母、数字和下划线。", "参数错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (keepRows <= 0)
            {
                MessageBox.Show(this, "保留行数必须大于 0。", "参数错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private static bool IsSafeIdentifier(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.All(ch => char.IsLetterOrDigit(ch) || ch == '_');
        }

        private static string QuoteIdentifier(string identifier)
        {
            return $"\"{identifier.Replace("\"", "\"\"")}\"";
        }

        private static SqliteConnection CreateConnection(string dbPath)
        {
            SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath
            };

            return new SqliteConnection(builder.ToString());
        }
    }
}
