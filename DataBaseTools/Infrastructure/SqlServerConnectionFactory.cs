using DataBaseTools.Configuration;
using Microsoft.Data.SqlClient;

namespace DataBaseTools.Infrastructure
{
    public static class SqlServerConnectionFactory
    {
        public static SqlConnection Create(SqlServerToolSettings settings, string databaseName)
        {
            return new SqlConnection(BuildConnectionString(settings, databaseName));
        }

        public static string BuildConnectionString(SqlServerToolSettings settings, string databaseName)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = settings.Server,
                InitialCatalog = databaseName,
                UserID = settings.UserId,
                Password = settings.Password,
                TrustServerCertificate = settings.TrustServerCertificate,
                MultipleActiveResultSets = true
            };

            return builder.ConnectionString;
        }
    }
}
