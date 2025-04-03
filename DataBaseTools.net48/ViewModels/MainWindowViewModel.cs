using NLog;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace DataBaseTools.net48.ViewModels
{
    public class TablesEntity
    {
        public string TableName { get; set; }
    }

    public class MainWindowViewModel : BindableBase
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        //string connectionString = @"Data Source=JYJ;Initial Catalog=VacuumSystem2;User ID=sa;Password=123456;";
        //string connectionString = @"Data Source=BK_IPC;Initial Catalog=VacuumSystem;User ID=sa;Password=edgyvac";
        string connectionString = GetConnectionString("sqlString");

        private readonly IDialogService _dialogService;

        public MainWindowViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        private string databaseName = "VacuumSystem";
        public string DatabaseName
        {
            get { return databaseName; }
            set { databaseName = value; RaisePropertyChanged(); }
        }

        private bool isOperate;
        public bool IsOperate
        {
            get { return isOperate; }
            set { isOperate = value; RaisePropertyChanged(); }
        }
        

        private string backUpFileName = "Test";
        public string BackUpFileName
        {
            get { return backUpFileName; }
            set { backUpFileName = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<string> tables = new ObservableCollection<string>();
        public ObservableCollection<string> Tables
        {
            get { return tables; }
            set { tables = value; RaisePropertyChanged(); }
        }

        private string currentSelectTable;
        public string CurrentSelectTable
        {
            get { return currentSelectTable; }
            set { currentSelectTable = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// Database file size
        /// </summary>
        string str1 = @"SELECT 
                        name AS FileName,
                        size/128.0 AS CurrentSizeMB,
                        size/128.0 - CAST(FILEPROPERTY(name, 'SpaceUsed') AS int)/128.0 AS FreeSpaceMB
                    FROM 
                        sys.database_files;";

        /// <summary>
        /// Whole database size 
        /// </summary>
        string str2 = @"SELECT
                        DB_NAME(database_id) AS DatabaseName,
                        SUM(size / 128.0) AS TotalSizeMB,
                        SUM(CASE WHEN type_desc = 'LOG' THEN size / 128.0 ELSE 0 END) AS LogSizeMB,
                        SUM(CASE WHEN type_desc = 'ROWS' THEN size / 128.0 ELSE 0 END) AS DataSizeMB
                    FROM
                        sys.master_files
                    WHERE
                        database_id = DB_ID('YourDatabaseName')-- your database name 
                    GROUP BY
                        database_id;";


        /// <summary>
        /// Whole FileGroups size
        /// </summary>
        string str3 = @"SELECT
                        f.name AS FileGroupName,
                        SUM(a.total_pages) * 8 AS TotalSpaceKB,
                        SUM(a.used_pages) * 8 AS UsedSpaceKB,
                        (SUM(a.total_pages) - SUM(a.used_pages)) * 8 AS UnusedSpaceKB
                    FROM
                        sys.allocation_units a
                    INNER JOIN
                        sys.filegroups f ON a.data_space_id = f.data_space_id
                    GROUP BY
                        f.name;";


        // SqlCommand.CommandTimeout
        // 获取或设置在终止执行命令的尝试并生成错误之前的等待时间。
        // 等待命令执行的时间（以秒为单位）。默认为 30 秒。
        // SqlConnection.ConnectionTimeout
        // 获取在尝试建立连接时终止尝试并生成错误之前所等待的时间。
        // 等待连接打开的时间（以秒为单位）。默认值为 15 秒。

        public static string GetConnectionString(string connectionStringName)
        {
            // 获取连接字符串集合
            ConnectionStringsSection section = (ConnectionStringsSection)ConfigurationManager.GetSection("connectionStrings");

            if (section != null)
            {
                ConnectionStringSettings settings = section.ConnectionStrings[connectionStringName];
                if (settings != null)
                {
                    return settings.ConnectionString;
                }
                else
                {
                    throw new ConfigurationErrorsException($"连接字符串 '{connectionStringName}' 未在 web.config 中找到。");
                }
            }
            else
            {
                throw new ConfigurationErrorsException("无法加载 connectionStrings 配置节。");
            }
        }

        /// <summary>
        /// Whole DataBase backup
        /// </summary>
        public ICommand WholeLibraryBackupCommand
        {
            get => new DelegateCommand(() =>
            {
                if (string.IsNullOrEmpty(BackUpFileName))
                    return;
                Task.Run(() =>
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        IsOperate = true;
                        //connection.ConnectionTimeout = 20;
                        try
                        {
                            connection.Open();
                            string sql = $@"
                            BACKUP DATABASE [{DatabaseName}] 
                            TO DISK = 'D:\{BackUpFileName}.bak'
                            WITH FORMAT, MEDIANAME = '{BackUpFileName}', NAME = '{BackUpFileName}'";
                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                command.CommandTimeout = 120;
                                command.ExecuteNonQuery();
                            }
                        }
                        catch (Exception ex)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                IsOperate = false;
                                logger.Error(ex.ToString());
                                DialogParameters keyValuePairs = new DialogParameters();
                                keyValuePairs.Add("Content", "操作失败");
                                _dialogService.ShowDialog("MessageView", keyValuePairs, null);
                                return;
                            });
                        }
                    }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        IsOperate = false;
                        DialogParameters keyValuePairs1 = new DialogParameters();
                        keyValuePairs1.Add("Content", "操作成功");
                        _dialogService.ShowDialog("MessageView", keyValuePairs1, null);
                    });
                });



            });
        }

        /// <summary>
        /// Pitch On Which DataBase 
        /// </summary>
        public ICommand PitchOnCommand
        {
            get => new DelegateCommand(() =>
            {
                try
                {

                    Tables = new ObservableCollection<string>();
                    //string str = "SELECT name as TableName FROM sys.tables;";
                    //context.Database.SqlQuery<TablesEntity>(str);
                    //List<TablesEntity> lst= context.Database.SqlQuery<TablesEntity>(str).ToList();
                    //lst.ForEach(t => Tables.Add(t.TableName));


                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        string sql = "SELECT name as TableName FROM sys.tables;";
                        connection.Open();
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                List<string> tableNames = new List<string>();
                                while (reader.Read())
                                {
                                    tableNames.Add(reader.GetString(0));
                                }
                                tableNames.ForEach(t => Tables.Add(t));
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    DialogParameters keyValuePairs = new DialogParameters();
                    keyValuePairs.Add("Content", "连接数据库失败，请联系厂商！");
                    _dialogService.ShowDialog("MessageView", keyValuePairs, null);
                }
            });
        }

        /// <summary>
        /// Delete Data Left 1000
        /// </summary>
        public ICommand DeleteLeft1000Command
        {
            get => new DelegateCommand(() =>
            {
                bool isSuscess = true;
                IsOperate = true;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        if (!File.Exists($@"D:\{BackUpFileName}.bak"))
                        {
                            DialogParameters keyValuePairs = new DialogParameters();
                            keyValuePairs.Add("Content", "请先备份数据库文件");
                            _dialogService.ShowDialog("MessageView", keyValuePairs, null);
                            // Todo A pop-up indicates that the operation succeeded
                            return;
                        }

                        connection.Open();
                        //string sql = $@" DELETE FROM {CurrentSelectTable} WHERE ID < (
                        //             SELECT MIN(ID) FROM
                        //        (SELECT TOP 1000 ID FROM {CurrentSelectTable} ORDER BY ID DESC))";

                        string sql = $@"DELETE FROM {CurrentSelectTable}
                                        WHERE ID < (
                                        SELECT MIN(ID)
                                FROM(
                                    SELECT TOP 5000 ID
                                    FROM {CurrentSelectTable}
                                    ORDER BY ID DESC
                                ) AS tmp);";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.ExecuteReader();
                        }
                    }
                    catch (Exception ex)
                    {
                        isSuscess = false;
                        IsOperate = false;
                        DialogParameters keyValuePairs = new DialogParameters();
                        keyValuePairs.Add("Content", "操作失败");
                        _dialogService.ShowDialog("MessageView", keyValuePairs, null);
                        return;
                    }
                }

                if(isSuscess)
                {
                    IsOperate = false;
                    DialogParameters keyValuePairs1 = new DialogParameters();
                    keyValuePairs1.Add("Content", "操作成功");
                    _dialogService.ShowDialog("MessageView", keyValuePairs1, null);
                }
            });
        }


        /// <summary>
        /// Delete Data Left 5000
        /// </summary>
        public ICommand DeleteLeft5000Command
        {
            get => new DelegateCommand(() =>
            {

                bool IsSuccess = true;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        IsOperate = true;
                        connection.Open();
                        //string sql = $@" DELETE FROM {CurrentSelectTable} WHERE ID < (
                        //             SELECT MIN(ID) FROM
                        //        (SELECT TOP 5000 ID FROM {CurrentSelectTable} ORDER BY ID DESC))";

                        string sql = $@"DELETE FROM {CurrentSelectTable}
                                        WHERE ID < (
                                        SELECT MIN(ID)
                                FROM(
                                    SELECT TOP 5000 ID
                                    FROM {CurrentSelectTable}
                                    ORDER BY ID DESC
                                ) AS tmp);";
                        logger.Debug(sql);
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.ExecuteReader();
                        }
                    }
                    catch (Exception ex)
                    {
                        IsOperate = false;
                        IsSuccess = false;
                        DialogParameters keyValuePairs = new DialogParameters();
                        keyValuePairs.Add("Content", "操作失败");
                        _dialogService.ShowDialog("MessageView", keyValuePairs, null);

                        logger.Error(ex.ToString());
                        return;
                    }
                }

                IsOperate = false;
                if(IsSuccess)
                {
                    DialogParameters keyValuePairs1 = new DialogParameters();
                    keyValuePairs1.Add("Content", "操作成功");
                    _dialogService.ShowDialog("MessageView", keyValuePairs1, null);
                }
            });
        }


        /// <summary>
        /// Delete Data Left 1/10
        /// </summary>
        public ICommand DeleteLeft110Command
        {
            get => new DelegateCommand(() =>
            {
                // Todo
            });
        }
    }

    public class ConnectionStringHelper
    {
        /// <summary>
        /// 获取指定名称的连接字符串（简化版）
        /// </summary>
        /// <param name="connectionStringName">连接字符串的名称</param>
        /// <returns>连接字符串的值</returns>
        public static string GetConnectionString(string connectionStringName)
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (settings != null)
            {
                return settings.ConnectionString;
            }
            else
            {
                throw new ConfigurationErrorsException($"连接字符串 '{connectionStringName}' 未在 web.config 中找到。");
            }
        }

        /// <summary>
        /// 示例用法
        /// </summary>
        public static void ExampleUsage()
        {
            try
            {
                string sqlConnectionString = GetConnectionString("sqlString");
                Console.WriteLine("sqlString 连接字符串内容:");
                Console.WriteLine(sqlConnectionString);
            }
            catch (ConfigurationErrorsException ex)
            {
                Console.WriteLine("配置错误: " + ex.Message);
            }
        }
    }
}
