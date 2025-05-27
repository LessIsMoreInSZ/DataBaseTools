//using CommunityToolkit.Mvvm.ComponentModel;
//using Prism.Common;
//using System;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace DataBaseTools.net48.ViewModels
//{
//    public class SqliteViewModel : ObservableObject
//    {
//        private readonly string _connectionString;

//        public SqliteViewModel(string dbFilePath)
//        {
//            _connectionString = $"Data Source={dbFilePath};Version=3;";
//        }

//        /// <summary>
//        /// 执行VACUUM命令压缩数据库
//        /// </summary>
//        /// <returns>压缩后的数据库大小(MB)</returns>
//        public double CompactDatabase()
//        {
//            long originalSize = GetDatabaseSize();
//            Console.WriteLine($"压缩前数据库大小: {originalSize / 1024.0 / 1024.0:F2} MB");

//            try
//            {
//                using (var connection = new SQLiteConnection(_connectionString))
//                {
//                    connection.Open();

//                    // 执行VACUUM命令
//                    using (var command = new SQLiteCommand("VACUUM;", connection))
//                    {
//                        command.ExecuteNonQuery();
//                    }

//                    connection.Close();
//                }

//                long newSize = GetDatabaseSize();
//                double savedSpace = (originalSize - newSize) / 1024.0 / 1024.0;

//                Console.WriteLine($"压缩后数据库大小: {newSize / 1024.0 / 1024.0:F2} MB");
//                Console.WriteLine($"节省空间: {savedSpace:F2} MB");

//                return newSize / 1024.0 / 1024.0;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"压缩数据库时出错: {ex.Message}");
//                throw;
//            }
//        }

//        /// <summary>
//        /// 获取数据库文件大小(字节)
//        /// </summary>
//        private long GetDatabaseSize()
//        {
//            string dbPath = _connectionString.Split('=')[1].Split(';')[0];
//            return new System.IO.FileInfo(dbPath).Length;
//        }

//        /// <summary>
//        /// 设置自动VACUUM模式
//        /// </summary>
//        public void SetAutoVacuumMode(bool enableFullAutoVacuum)
//        {
//            try
//            {
//                using (var connection = new SQLiteConnection(_connectionString))
//                {
//                    connection.Open();

//                    string mode = enableFullAutoVacuum ? "FULL" : "NONE";
//                    using (var command = new SQLiteCommand($"PRAGMA auto_vacuum = {mode};", connection))
//                    {
//                        command.ExecuteNonQuery();
//                    }

//                    connection.Close();
//                }
//                Console.WriteLine($"已设置auto_vacuum模式为: {(enableFullAutoVacuum ? "FULL" : "NONE")}");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"设置auto_vacuum模式时出错: {ex.Message}");
//                throw;
//            }
//        }
//    }
//}
