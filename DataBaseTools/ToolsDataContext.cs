using DataBaseTools.Configuration;
using DataBaseTools.Infrastructure;
using Microsoft.Data.SqlClient;

namespace DataBaseTools
{
    public static class ToolsDataContext
    {
        public static SqlConnection CreateConnection(SqlServerToolSettings settings, string databaseName)
        {
            return SqlServerConnectionFactory.Create(settings, databaseName);
        }
    }
}
