using System.IO;
using System.Text.Json;

namespace DataBaseTools.Configuration
{
    public class SqlServerToolSettings
    {
        public string Server { get; set; } = ".";

        public string DatabaseName { get; set; } = "VacuumSystem2";

        public string UserId { get; set; } = "sa";

        public string Password { get; set; } = string.Empty;

        public bool TrustServerCertificate { get; set; } = true;

        public string BackupDirectory { get; set; } = @"D:\";

        public string DefaultBackupFileName { get; set; } = "VacuumSystem2_Backup";

        public string DefaultCleanupTable { get; set; } = "chart";

        public int DefaultKeepRows { get; set; } = 1000;

        public string DefaultIdColumn { get; set; } = "ID";

        public string DefaultIndexTable { get; set; } = "chart";

        public string DefaultIndexColumn { get; set; } = "creat_time";

        public static SqlServerToolSettings Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new SqlServerToolSettings();
            }

            try
            {
                string json = File.ReadAllText(filePath);
                SqlServerToolSettings? settings = JsonSerializer.Deserialize<SqlServerToolSettings>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return settings ?? new SqlServerToolSettings();
            }
            catch (JsonException)
            {
                return new SqlServerToolSettings();
            }
        }
    }
}
