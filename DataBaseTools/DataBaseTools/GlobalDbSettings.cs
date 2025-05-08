using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseTools
{
    public static class GlobalDbSettings
    {
        public static DatabaseType CurrentDatabaseType { get; set; }=DatabaseType.SqlServer;
        public static string  ConnectionString { get; set; }= @"Data Source=JYJ;Initial Catalog=VacuumSystem2;User ID=sa;Password=123456;MultipleActiveResultSets=true";
        public static void UseSqlServer(string connectString)
        {
            CurrentDatabaseType = DatabaseType.SqlServer;
            ConnectionString = connectString;
        }
        public static void UseMySql(String  connectString)
        {
            CurrentDatabaseType = DatabaseType.Mysql;
            ConnectionString = connectString;
        }
        public static void UseSqlite(String connectString)
        {
            CurrentDatabaseType = DatabaseType.SqlServer;
            ConnectionString = connectString;
        }
    }
}
