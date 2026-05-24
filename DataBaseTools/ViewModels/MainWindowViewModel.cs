using DataBaseTools.Configuration;
using DataBaseTools.Infrastructure;
using Microsoft.Data.SqlClient;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DataBaseTools.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IDialogService _dialogService;
        private readonly SqlServerToolSettings _settings;
        private readonly string _settingsPath;
        private readonly DelegateCommand _loadTablesCommand;
        private readonly DelegateCommand _refreshSummaryCommand;
        private readonly DelegateCommand _backupDatabaseCommand;
        private readonly DelegateCommand _cleanupTableCommand;
        private readonly DelegateCommand _createIndexCommand;

        public MainWindowViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            _settingsPath = Path.Combine(AppContext.BaseDirectory, "toolsettings.json");
            _settings = SqlServerToolSettings.Load(_settingsPath);

            databaseName = _settings.DatabaseName;
            backUpFileName = _settings.DefaultBackupFileName;
            backupDirectory = _settings.BackupDirectory;
            keepRowsText = _settings.DefaultKeepRows.ToString();
            currentSelectTable = _settings.DefaultCleanupTable;
            statusMessage = $"已加载配置文件：{_settingsPath}";
            databaseSummary = "点击“读取数据库概览”查看当前数据库大小、日志大小和数据表数量。";

            _loadTablesCommand = new DelegateCommand(async () => await LoadTablesAsync(), () => !IsBusy);
            _refreshSummaryCommand = new DelegateCommand(async () => await RefreshDatabaseSummaryAsync(), () => !IsBusy);
            _backupDatabaseCommand = new DelegateCommand(async () => await BackupDatabaseAsync(), () => !IsBusy);
            _cleanupTableCommand = new DelegateCommand(async () => await CleanupTableAsync(), () => !IsBusy);
            _createIndexCommand = new DelegateCommand(async () => await CreateIndexAsync(), () => !IsBusy);
        }

        private string databaseName = string.Empty;
        public string DatabaseName
        {
            get => databaseName;
            set => SetProperty(ref databaseName, value);
        }

        private string backUpFileName = string.Empty;
        public string BackUpFileName
        {
            get => backUpFileName;
            set => SetProperty(ref backUpFileName, value);
        }

        private string backupDirectory = @"D:\";
        public string BackupDirectory
        {
            get => backupDirectory;
            set => SetProperty(ref backupDirectory, value);
        }

        private string keepRowsText = "1000";
        public string KeepRowsText
        {
            get => keepRowsText;
            set => SetProperty(ref keepRowsText, value);
        }

        private ObservableCollection<string> tables = new ObservableCollection<string>();
        public ObservableCollection<string> Tables
        {
            get => tables;
            set => SetProperty(ref tables, value);
        }

        private string currentSelectTable = string.Empty;
        public string CurrentSelectTable
        {
            get => currentSelectTable;
            set => SetProperty(ref currentSelectTable, value);
        }

        private string statusMessage = string.Empty;
        public string StatusMessage
        {
            get => statusMessage;
            set => SetProperty(ref statusMessage, value);
        }

        private string databaseSummary = string.Empty;
        public string DatabaseSummary
        {
            get => databaseSummary;
            set => SetProperty(ref databaseSummary, value);
        }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            private set
            {
                if (SetProperty(ref isBusy, value))
                {
                    RaiseCommandStates();
                }
            }
        }

        public ICommand WholeLibraryBackupCommand => _backupDatabaseCommand;

        public ICommand PitchOnCommand => _loadTablesCommand;

        public ICommand RefreshSummaryCommand => _refreshSummaryCommand;

        public ICommand DeleteLeft1000Command => _cleanupTableCommand;

        public ICommand CreateIndexCommand => _createIndexCommand;

        private async Task LoadTablesAsync()
        {
            await RunBusyOperationAsync(
                "正在读取数据表列表...",
                async () =>
                {
                    using SqlConnection connection = SqlServerConnectionFactory.Create(_settings, DatabaseName);
                    using SqlCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT name FROM sys.tables ORDER BY name;";

                    await connection.OpenAsync();
                    using SqlDataReader reader = await command.ExecuteReaderAsync();

                    ObservableCollection<string> loadedTables = new ObservableCollection<string>();
                    while (await reader.ReadAsync())
                    {
                        loadedTables.Add(reader.GetString(0));
                    }

                    Tables = loadedTables;
                    if (string.IsNullOrWhiteSpace(CurrentSelectTable) || !Tables.Any(t => t.Equals(CurrentSelectTable, StringComparison.OrdinalIgnoreCase)))
                    {
                        CurrentSelectTable = Tables.FirstOrDefault() ?? string.Empty;
                    }

                    StatusMessage = $"已读取 {Tables.Count} 张数据表。";
                });
        }

        private async Task RefreshDatabaseSummaryAsync()
        {
            await RunBusyOperationAsync(
                "正在读取数据库概览...",
                async () =>
                {
                    using SqlConnection connection = SqlServerConnectionFactory.Create(_settings, DatabaseName);
                    await connection.OpenAsync();

                    decimal dataSizeMb;
                    decimal logSizeMb;
                    int tableCount;

                    using (SqlCommand sizeCommand = connection.CreateCommand())
                    {
                        sizeCommand.CommandText = @"
SELECT
    CAST(SUM(CASE WHEN type_desc = 'ROWS' THEN size ELSE 0 END) * 8.0 / 1024 AS DECIMAL(18, 2)) AS DataSizeMb,
    CAST(SUM(CASE WHEN type_desc = 'LOG' THEN size ELSE 0 END) * 8.0 / 1024 AS DECIMAL(18, 2)) AS LogSizeMb
FROM sys.database_files;";

                        using SqlDataReader reader = await sizeCommand.ExecuteReaderAsync();
                        await reader.ReadAsync();
                        dataSizeMb = reader.IsDBNull(0) ? 0m : reader.GetDecimal(0);
                        logSizeMb = reader.IsDBNull(1) ? 0m : reader.GetDecimal(1);
                    }

                    using (SqlCommand tableCommand = connection.CreateCommand())
                    {
                        tableCommand.CommandText = "SELECT COUNT(*) FROM sys.tables;";
                        object? countResult = await tableCommand.ExecuteScalarAsync();
                        tableCount = Convert.ToInt32(countResult);
                    }

                    DatabaseSummary =
                        $"数据库：{DatabaseName}{Environment.NewLine}" +
                        $"数据文件大小：{dataSizeMb:N2} MB{Environment.NewLine}" +
                        $"日志文件大小：{logSizeMb:N2} MB{Environment.NewLine}" +
                        $"数据表数量：{tableCount}{Environment.NewLine}" +
                        $"配置文件：{_settingsPath}";

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

                    using SqlConnection connection = SqlServerConnectionFactory.Create(_settings, DatabaseName);
                    using SqlCommand command = connection.CreateCommand();
                    command.CommandTimeout = 300;
                    command.CommandText = $@"
BACKUP DATABASE {escapedDatabase}
TO DISK = N'{escapedPath}'
WITH FORMAT, MEDIANAME = N'{escapedName}', NAME = N'{escapedName}';";

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    StatusMessage = $"备份完成：{backupPath}";
                    ShowMessage($"备份成功，文件已生成到：{backupPath}");
                });
        }

        private async Task CleanupTableAsync()
        {
            if (!TryGetValidCleanupOptions(out int keepRows, out string verifiedTableName, out string backupPath, out string errorMessage))
            {
                ShowMessage(errorMessage);
                return;
            }

            await RunBusyOperationAsync(
                $"正在清理数据表 {verifiedTableName}，保留最近 {keepRows} 条记录...",
                async () =>
                {
                    using SqlConnection connection = SqlServerConnectionFactory.Create(_settings, DatabaseName);
                    await connection.OpenAsync();

                    bool hasIdColumn = await HasColumnAsync(connection, verifiedTableName, _settings.DefaultIdColumn);
                    if (!hasIdColumn)
                    {
                        throw new InvalidOperationException($"数据表 {verifiedTableName} 不包含 {_settings.DefaultIdColumn} 列，无法执行按 ID 保留数据。");
                    }

                    using SqlCommand command = connection.CreateCommand();
                    command.CommandTimeout = 300;
                    command.CommandText = $@"
DELETE FROM {EscapeIdentifier(verifiedTableName)}
WHERE {EscapeIdentifier(_settings.DefaultIdColumn)} < (
    SELECT MIN({EscapeIdentifier(_settings.DefaultIdColumn)})
    FROM (
        SELECT TOP (@KeepRows) {EscapeIdentifier(_settings.DefaultIdColumn)}
        FROM {EscapeIdentifier(verifiedTableName)}
        ORDER BY {EscapeIdentifier(_settings.DefaultIdColumn)} DESC
    ) AS RecentRows
);";
                    command.Parameters.AddWithValue("@KeepRows", keepRows);

                    int affectedRows = await command.ExecuteNonQueryAsync();
                    StatusMessage = $"已清理 {verifiedTableName}，影响 {affectedRows} 条数据。";
                    ShowMessage($"操作成功。已基于备份文件 {backupPath} 清理旧数据，保留最近 {keepRows} 条。");
                });
        }

        private async Task CreateIndexAsync()
        {
            string indexTable = _settings.DefaultIndexTable;
            string indexColumn = _settings.DefaultIndexColumn;
            string indexName = $"IX_{indexTable}_{indexColumn}";

            await RunBusyOperationAsync(
                $"正在为 {indexTable}.{indexColumn} 创建索引...",
                async () =>
                {
                    using SqlConnection connection = SqlServerConnectionFactory.Create(_settings, DatabaseName);
                    await connection.OpenAsync();

                    if (Tables.Any() && !Tables.Any(t => t.Equals(indexTable, StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new InvalidOperationException($"当前数据库中未找到 {indexTable} 数据表。");
                    }

                    bool hasColumn = await HasColumnAsync(connection, indexTable, indexColumn);
                    if (!hasColumn)
                    {
                        throw new InvalidOperationException($"数据表 {indexTable} 不包含 {indexColumn} 列，无法创建索引。");
                    }

                    using SqlCommand command = connection.CreateCommand();
                    command.CommandText = $@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'{EscapeSqlStringLiteral(indexName)}'
      AND object_id = OBJECT_ID(N'{EscapeSqlStringLiteral(indexTable)}')
)
BEGIN
    CREATE INDEX {EscapeIdentifier(indexName)}
    ON {EscapeIdentifier(indexTable)} ({EscapeIdentifier(indexColumn)});
END;";

                    await command.ExecuteNonQueryAsync();
                    StatusMessage = $"索引检查完成：{indexName}";
                    ShowMessage($"索引处理完成：{indexName}");
                });
        }

        private async Task<bool> HasColumnAsync(SqlConnection connection, string tableName, string columnName)
        {
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT COUNT(*)
FROM sys.columns
WHERE object_id = OBJECT_ID(@TableName)
  AND name = @ColumnName;";
            command.Parameters.AddWithValue("@TableName", tableName);
            command.Parameters.AddWithValue("@ColumnName", columnName);

            object? result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
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
                errorMessage = $"未找到备份文件：{backupPath}{Environment.NewLine}请先完成整库备份，再执行删数。";
                return false;
            }

            verifiedTableName = Tables.First(t => t.Equals(CurrentSelectTable, StringComparison.OrdinalIgnoreCase));
            return true;
        }

        private string GetBackupFilePath()
        {
            string safeFileName = string.Join(
                "_",
                BackUpFileName
                    .Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries))
                .Trim();

            if (string.IsNullOrWhiteSpace(safeFileName))
            {
                safeFileName = "DatabaseBackup";
            }

            return Path.Combine(BackupDirectory, $"{safeFileName}.bak");
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
                StatusMessage = $"操作失败：{ex.Message}";
                ShowMessage($"操作失败：{ex.Message}");
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

            _dialogService.ShowDialog("MessageView", dialogParameters, null);
        }

        private void RaiseCommandStates()
        {
            _loadTablesCommand.RaiseCanExecuteChanged();
            _refreshSummaryCommand.RaiseCanExecuteChanged();
            _backupDatabaseCommand.RaiseCanExecuteChanged();
            _cleanupTableCommand.RaiseCanExecuteChanged();
            _createIndexCommand.RaiseCanExecuteChanged();
        }

        private static string EscapeIdentifier(string identifier)
        {
            return $"[{identifier.Replace("]", "]]")}]";
        }

        private static string EscapeSqlStringLiteral(string value)
        {
            return value.Replace("'", "''");
        }
    }
}
