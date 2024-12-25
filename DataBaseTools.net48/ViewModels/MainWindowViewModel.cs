using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
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
        string connectionString = @"Data Source=JYJ;Initial Catalog=VacuumSystem2;User ID=sa;Password=123456;";
        private readonly IDialogService _dialogService;

        public MainWindowViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        private string databaseName = "VacuumSystem2";
        public string DatabaseName
        {
            get { return databaseName; }
            set { databaseName = value; RaisePropertyChanged(); }
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


        /// <summary>
        /// Whole DataBase backup
        /// </summary>
        public ICommand WholeLibraryBackupCommand
        {
            get => new DelegateCommand(() =>
            {
                if (string.IsNullOrEmpty(BackUpFileName))
                    return;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        string sql = $@"
                            BACKUP DATABASE [{DatabaseName}] 
                            TO DISK = 'D:\{BackUpFileName}.bak'
                            WITH FORMAT, MEDIANAME = '{BackUpFileName}', NAME = '{BackUpFileName}'";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.ExecuteReader();
                        }
                    }
                    catch (Exception ex)
                    {
                        DialogParameters keyValuePairs = new DialogParameters();
                        keyValuePairs.Add("Content", "操作失败");
                        _dialogService.ShowDialog("MessageView", keyValuePairs, null);
                        return;
                    }
                }

                DialogParameters keyValuePairs1 = new DialogParameters();
                keyValuePairs1.Add("Content", "操作成功");
                _dialogService.ShowDialog("MessageView", keyValuePairs1, null);


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
                        string sql = $@" DELETE FROM {CurrentSelectTable} WHERE ID < (
                                     SELECT MIN(ID) FROM
                                (SELECT TOP 1000 ID FROM {CurrentSelectTable} ORDER BY ID DESC)";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.ExecuteReader();
                        }
                    }
                    catch (Exception ex)
                    {
                        DialogParameters keyValuePairs = new DialogParameters();
                        keyValuePairs.Add("Content", "操作失败");
                        _dialogService.ShowDialog("MessageView", keyValuePairs, null);
                        return;
                    }
                }

                DialogParameters keyValuePairs1 = new DialogParameters();
                keyValuePairs1.Add("Content", "操作成功");
                _dialogService.ShowDialog("MessageView", keyValuePairs1, null);


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
}
