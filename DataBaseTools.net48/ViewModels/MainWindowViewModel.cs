using NLog;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DataBaseTools.net48.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IDialogService dialogService;
        private readonly SqlConnectionStringBuilder connectionStringBuilder;
        private readonly DelegateCommand loadTablesCommand;
        private readonly DelegateCommand refreshSummaryCommand;
        private readonly DelegateCommand backupDatabaseCommand;
        private readonly DelegateCommand cleanupTableCommand;
        private readonly DelegateCommand addIndexCommand;

        public MainWindowViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;
            connectionStringBuilder = new SqlConnectionStringBuilder(GetConnectionString("sqlString"));

            databaseName = string.IsNullOrWhiteSpace(connectionStringBuilder.InitialCatalog)
                ? "VacuumSystem"
                : connectionStringBuilder.InitialCatalog;
            backUpFileName = GetAppSetting("DefaultBackupFileName", databaseName + "_Backup");
            backupDirectory = GetAppSetting("BackupDirectory", @"D:\");
            keepRowsText = GetAppSetting("DefaultKeepRows", "5000");
            currentSelectTable = GetAppSetting("DefaultCleanupTable", "chart");
            statusMessage = "已从 App.config 加载连接配置。";
            databaseSummary = "点击“读取数据库概览”查看当前数据库大小、日志大小和数据表数量。";

            loadTablesCommand = new DelegateCommand(async () => await LoadTablesAsync(), () => !IsBusy);
            refreshSummaryCommand = new DelegateCommand(async () => await RefreshDatabaseSummaryAsync(), () => !IsBusy);
            backupDatabaseCommand = new DelegateCommand(async () => await BackupDatabaseAsync(), () => !IsBusy);
            cleanupTableCommand = new DelegateCommand(async () => await CleanupTableAsync(), () => !IsBusy);
            addIndexCommand = new DelegateCommand(async () => await CreateIndexAsync(), () => !IsBusy);
        }

        private string databaseName = string.Empty;
        public string DatabaseName
        {
            get { return databaseName; }
            set { SetProperty(ref databaseName, value); }
        }

        private string backUpFileName = string.Empty;
        public string BackUpFileName
        {
            get { return backUpFileName; }
            set { SetProperty(ref backUpFileName, value); }
        }

        private string backupDirectory = @"D:\";
        public string BackupDirectory
        {
            get { return backupDirectory; }
            set { SetProperty(ref backupDirectory, value); }
        }

        private string keepRowsText = "5000";
        public string KeepRowsText
        {
            get { return keepRowsText; }
            set { SetProperty(ref keepRowsText, value); }
        }

        private ObservableCollection<string> tables = new ObservableCollection<string>();
        public ObservableCollection<string> Tables
        {
            get { return tables; }
            set { SetProperty(ref tables, value); }
        }

        private string currentSelectTable = string.Empty;
        public string CurrentSelectTable
        {
            get { return currentSelectTable; }
            set { SetProperty(ref currentSelectTable, value); }
        }

        private string statusMessage = string.Empty;
        public string StatusMessage
        {
            get { return statusMessage; }
            set { SetProperty(ref statusMessage, value); }
        }

        private string databaseSummary = string.Empty;
        public string DatabaseSummary
        {
            get { return databaseSummary; }
            set { SetProperty(ref databaseSummary, value); }
        }

        private bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            private set
            {
                if (SetProperty(ref isBusy, value))
                {
                    RaiseCommandStates();
                }
            }
        }

        public ICommand WholeLibraryBackupCommand
        {
            get { return backupDatabaseCommand; }
        }

        public ICommand PitchOnCommand
        {
            get { return loadTablesCommand; }
        }

        public ICommand RefreshSummaryCommand
        {
            get { return refreshSummaryCommand; }
        }

        public ICommand DeleteLeft1000Command
        {
            get { return cleanupTableCommand; }
        }

        public ICommand AddIndexCommand
        {
            get { return addIndexCommand; }
        }

        private async Task LoadTablesAsync()
        {
            await RunBusyOperationAsync(
                "正在读取数据表列表...",
                async () =>
                {
                    using (SqlConnection connection = CreateConnection())
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT name FROM sys.tables ORDER BY name;";

                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            ObservableCollection<string> loadedTables = new ObservableCollection<string>();
                            while (await reader.ReadAsync())
                            {
                                loadedTables.Add(reader.GetString(0));
                            }

                            Tables = loadedTables;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(CurrentSelectTable) || !Tables.Any(t => t.Equals(CurrentSelectTable, StringComparison.OrdinalIgnoreCase)))
                    {
                        CurrentSelectTable = Tables.FirstOrDefault() ?? string.Empty;
                    }

                    StatusMessage = string.Format("已读取 {0} 张数据表。", Tables.Count);
                });
        }

        private async Task RefreshDatabaseSummaryAsync()
        {
            await RunBusyOperationAsync(
                "正在读取数据库概览...",
                async () =>
                {
                    decimal dataSizeMb = 0m;
                    decimal logSizeMb = 0m;
                    int tableCount;

                    using (SqlConnection connection = CreateConnection())
                    {
                        await connection.OpenAsync();

                        using (SqlCommand sizeCommand = connection.CreateCommand())
                        {
                            sizeCommand.CommandText = @"
SELECT
    CAST(SUM(CASE WHEN type_desc = 'ROWS' THEN size ELSE 0 END) * 8.0 / 1024 AS DECIMAL(18, 2)) AS DataSizeMb,
    CAST(SUM(CASE WHEN type_desc = 'LOG' THEN size ELSE 0 END) * 8.0 / 1024 AS DECIMAL(18, 2)) AS LogSizeMb
FROM sys.database_files;";

                            using (SqlDataReader reader = await sizeCommand.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    dataSizeMb = reader.IsDBNull(0) ? 0m : reader.GetDecimal(0);
                                    logSizeMb = reader.IsDBNull(1) ? 0m : reader.GetDecimal(1);
                                }
                            }
                        }

                        using (SqlCommand tableCommand = connection.CreateCommand())
                        {
                            tableCommand.CommandText = "SELECT COUNT(*) FROM sys.tables;";
                            object countResult = await tableCommand.ExecuteScalarAsync();
                            tableCount = Convert.ToInt32(countResult);
                        }
                    }

                    DatabaseSummary =
                        string.Format("数据库：{0}{1}", DatabaseName, Environment.NewLine) +
                        string.Format("数据文件大小：{0:N2} MB{1}", dataSizeMb, Environment.NewLine) +
                        string.Format("日志文件大小：{0:N2} MB{1}", logSizeMb, Environment.NewLine) +
                        string.Format("数据表数量：{0}{1}", tableCount, Environment.NewLine) +
                        "配置来源：App.config";

                    StatusMessage = "数据库概览已更新。";
                });
        }

        private async Task BackupDatabaseAsync()
        {
            if (string.IsNullOrWhiteSpace(DatabaseName))
            {
                ShowMessage("请先填写数据库名称。");
                return;
            }

            if (string.IsNullOrWhiteSpace(BackUpFileName))
            {
                ShowMessage("请先填写备份文件名。");
                return;
            }

            await RunBusyOperationAsync(
                "正在执行整库备份，请稍候...",
                async () =>
                {
                    Directory.CreateDirectory(BackupDirectory);

                    string backupPath = GetBackupFilePath();
                    string escapedDatabase = EscapeIdentifier(DatabaseName);
                    string escapedPath = EscapeSqlStringLiteral(backupPath);
                    string escapedName = EscapeSqlStringLiteral(Path.GetFileNameWithoutExtension(backupPath));

                    using (SqlConnection connection = CreateConnection())
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandTimeout = 300;
                        command.CommandText = string.Format(
                            @"BACKUP DATABASE {0}
TO DISK = N'{1}'
WITH FORMAT, MEDIANAME = N'{2}', NAME = N'{2}';",
                            escapedDatabase,
                            escapedPath,
                            escapedName);

                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }

                    StatusMessage = "备份完成：" + backupPath;
                    ShowMessage("备份成功，文件已生成到：" + backupPath);
                });
        }

        private async Task CleanupTableAsync()
        {
            int keepRows;
            string verifiedTableName;
            string backupPath;
            string errorMessage;

            if (!TryGetValidCleanupOptions(out keepRows, out verifiedTableName, out backupPath, out errorMessage))
            {
                ShowMessage(errorMessage);
                return;
            }

            await RunBusyOperationAsync(
                string.Format("正在清理数据表 {0}，保留最近 {1} 条记录...", verifiedTableName, keepRows),
                async () =>
                {
                    using (SqlConnection connection = CreateConnection())
                    {
                        await connection.OpenAsync();

                        bool hasIdColumn = await HasColumnAsync(connection, verifiedTableName, GetAppSetting("DefaultIdColumn", "ID"));
                        if (!hasIdColumn)
                        {
                            throw new InvalidOperationException(
                                string.Format("数据表 {0} 不包含 {1} 列，无法执行按 ID 保留数据。", verifiedTableName, GetAppSetting("DefaultIdColumn", "ID")));
                        }

                        using (SqlCommand command = connection.CreateCommand())
                        {
                            string idColumn = GetAppSetting("DefaultIdColumn", "ID");
                            command.CommandTimeout = 300;
                            command.CommandText = string.Format(
                                @"DELETE FROM {0}
WHERE {1} < (
    SELECT MIN({1})
    FROM (
        SELECT TOP (@KeepRows) {1}
        FROM {0}
        ORDER BY {1} DESC
    ) AS RecentRows
);",
                                EscapeIdentifier(verifiedTableName),
                                EscapeIdentifier(idColumn));
                            command.Parameters.AddWithValue("@KeepRows", keepRows);

                            int affectedRows = await command.ExecuteNonQueryAsync();
                            StatusMessage = string.Format("已清理 {0}，影响 {1} 条数据。", verifiedTableName, affectedRows);
                        }
                    }

                    ShowMessage(
                        string.Format("操作成功。已基于备份文件 {0} 清理旧数据，保留最近 {1} 条。", backupPath, keepRows));
                });
        }

        private async Task CreateIndexAsync()
        {
            string indexTable = GetAppSetting("DefaultIndexTable", "chart");
            string indexColumn = GetAppSetting("DefaultIndexColumn", "creat_time");
            string indexName = string.Format("IX_{0}_{1}", indexTable, indexColumn);

            await RunBusyOperationAsync(
                string.Format("正在为 {0}.{1} 创建索引...", indexTable, indexColumn),
                async () =>
                {
                    using (SqlConnection connection = CreateConnection())
                    {
                        await connection.OpenAsync();

                        if (Tables.Any() && !Tables.Any(t => t.Equals(indexTable, StringComparison.OrdinalIgnoreCase)))
                        {
                            throw new InvalidOperationException(string.Format("当前数据库中未找到 {0} 数据表。", indexTable));
                        }

                        bool hasColumn = await HasColumnAsync(connection, indexTable, indexColumn);
                        if (!hasColumn)
                        {
                            throw new InvalidOperationException(string.Format("数据表 {0} 不包含 {1} 列，无法创建索引。", indexTable, indexColumn));
                        }

                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = string.Format(
                                @"IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'{0}'
      AND object_id = OBJECT_ID(N'{1}')
)
BEGIN
    CREATE INDEX {2}
    ON {3} ({4});
END;",
                                EscapeSqlStringLiteral(indexName),
                                EscapeSqlStringLiteral(indexTable),
                                EscapeIdentifier(indexName),
                                EscapeIdentifier(indexTable),
                                EscapeIdentifier(indexColumn));

                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    StatusMessage = "索引检查完成：" + indexName;
                    ShowMessage("索引处理完成：" + indexName);
                });
        }

        private async Task<bool> HasColumnAsync(SqlConnection connection, string tableName, string columnName)
        {
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT COUNT(*)
FROM sys.columns
WHERE object_id = OBJECT_ID(@TableName)
  AND name = @ColumnName;";
                command.Parameters.AddWithValue("@TableName", tableName);
                command.Parameters.AddWithValue("@ColumnName", columnName);

                object result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
        }

        private bool TryGetValidCleanupOptions(out int keepRows, out string verifiedTableName, out string backupPath, out string errorMessage)
        {
            keepRows = 0;
            verifiedTableName = string.Empty;
            backupPath = GetBackupFilePath();
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(CurrentSelectTable))
            {
                errorMessage = "请先选择要清理的数据表。";
                return false;
            }

            if (!Tables.Any(t => t.Equals(CurrentSelectTable, StringComparison.OrdinalIgnoreCase)))
            {
                errorMessage = "当前选中的表不在已加载列表中，请重新点击“读取表列表”。";
                return false;
            }

            if (!int.TryParse(KeepRowsText, out keepRows) || keepRows <= 0)
            {
                errorMessage = "保留行数必须是大于 0 的整数。";
                return false;
            }

            if (!File.Exists(backupPath))
            {
                errorMessage = string.Format("未找到备份文件：{0}{1}请先完成整库备份，再执行删数。", backupPath, Environment.NewLine);
                return false;
            }

            verifiedTableName = Tables.First(t => t.Equals(CurrentSelectTable, StringComparison.OrdinalIgnoreCase));
            return true;
        }

        private SqlConnection CreateConnection()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionStringBuilder.ConnectionString)
            {
                InitialCatalog = DatabaseName
            };

            return new SqlConnection(builder.ConnectionString);
        }

        private string GetBackupFilePath()
        {
            string safeFileName = string.Join(
                "_",
                BackUpFileName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim();

            if (string.IsNullOrWhiteSpace(safeFileName))
            {
                safeFileName = "DatabaseBackup";
            }

            return Path.Combine(BackupDirectory, safeFileName + ".bak");
        }

        private async Task RunBusyOperationAsync(string busyMessage, Func<Task> operation)
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = busyMessage;
                await operation();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                StatusMessage = "操作失败：" + ex.Message;
                ShowMessage("操作失败：" + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ShowMessage(string message)
        {
            DialogParameters dialogParameters = new DialogParameters
            {
                { "Content", message }
            };

            dialogService.ShowDialog("MessageView", dialogParameters, null);
        }

        private void RaiseCommandStates()
        {
            loadTablesCommand.RaiseCanExecuteChanged();
            refreshSummaryCommand.RaiseCanExecuteChanged();
            backupDatabaseCommand.RaiseCanExecuteChanged();
            cleanupTableCommand.RaiseCanExecuteChanged();
            addIndexCommand.RaiseCanExecuteChanged();
        }

        private static string GetConnectionString(string connectionStringName)
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                throw new ConfigurationErrorsException(string.Format("连接字符串 {0} 未在 App.config 中找到。", connectionStringName));
            }

            return settings.ConnectionString;
        }

        private static string GetAppSetting(string key, string defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private static string EscapeIdentifier(string identifier)
        {
            return string.Format("[{0}]", identifier.Replace("]", "]]"));
        }

        private static string EscapeSqlStringLiteral(string value)
        {
            return value.Replace("'", "''");
        }
    }
}
