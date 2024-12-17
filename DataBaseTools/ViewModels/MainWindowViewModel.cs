using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DataBaseTools.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {

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
                using (var context = new ToolsDataContext())
                {
                    var connection = context.Database.GetDbConnection();
                    try
                    {
                        connection.Open();
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @$"
                            BACKUP DATABASE [{DatabaseName}] 
                            TO DISK = 'D:\{BackUpFileName}.bak'
                            WITH FORMAT, MEDIANAME = '{BackUpFileName}', NAME = '{BackUpFileName}'";
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    if(File.Exists(@$"D:\{BackUpFileName}.bak"))
                    {
                        // Todo A pop-up indicates that the operation succeeded
                        
                    }
                    // Todo Failed

                }
            });
        }

        /// <summary>
        /// Pitch On Which DataBase 
        /// </summary>
        public ICommand PitchOnCommand
        {
            get => new DelegateCommand(() =>
            {
                using (var context = new ToolsDataContext())
                {
                    var connection = context.Database.GetDbConnection();
                    try
                    {
                        Tables = new ObservableCollection<string>();
                        var command = connection.CreateCommand();
                        command.CommandText = "SELECT name FROM sys.tables;";
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Tables.Add(reader.GetString(0));
                            }
                        }
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                    }
                }
            });
        }
    }
}
