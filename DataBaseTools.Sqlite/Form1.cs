using Microsoft.Data.Sqlite;

namespace DataBaseTools
{
    public partial class Form1 : Form
    {
        string dbPath = @"Data Source=c:\Data\‪Application.db";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // 数据库文件路径
                //string dbPath = @"Data Source=c:\Data\‪Application.db";

                // 创建 SqliteConnection 对象
                using (SqliteConnection connection = new SqliteConnection(dbPath))
                {
                    // 打开连接
                    connection.Open();

                    // 创建 SqliteCommand 对象
                    using (SqliteCommand command = connection.CreateCommand())
                    {
                        // SQL 语句：在 my_table 表的 my_column 列上创建索引
                        string sql = "CREATE INDEX IF NOT EXISTS create_time ON Products(CreateTime);";

                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }

                    // 关闭连接（using 块会自动关闭连接）
                }

                MessageBox.Show("执行成功!");
            }
            catch (Exception)
            {
                MessageBox.Show("执行失败");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // 显示确认对话框
            DialogResult result = MessageBox.Show(
                "确定要删除这些记录吗？",
                "删除确认",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2); // 默认选中"No"
            if (result == DialogResult.Yes)
            {
                using (SqliteConnection connection = new SqliteConnection(dbPath))
                {
                    try
                    {

                        string sql = $@"DELETE FROM Products
                                        WHERE ID < (
                                        SELECT MIN(ID)
                                FROM(
                                    SELECT TOP 5000 ID
                                    FROM Products
                                    ORDER BY ID DESC
                                ) AS tmp);
                                VACUUM;";
                        using (SqliteCommand command = new SqliteCommand(sql, connection))
                        {
                            command.ExecuteReader();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"删除失败QAQ: {ex.Message}");
                        return;
                    }

                    MessageBox.Show("删除成功！");
                }
            }
               


        }
    }
}
