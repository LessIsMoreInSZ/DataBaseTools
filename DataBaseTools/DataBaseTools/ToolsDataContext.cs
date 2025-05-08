using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DataBaseTools
{
    public class ToolsDataContext: DbContext
    {
        public ToolsDataContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            switch (GlobalDbSettings.CurrentDatabaseType)
            {
                case DatabaseType.SqlServer:
                    optionsBuilder.UseSqlServer(GlobalDbSettings.ConnectionString);
                    break;
                case DatabaseType.Mysql:
                    optionsBuilder.UseMySql(GlobalDbSettings.ConnectionString, ServerVersion.AutoDetect(GlobalDbSettings.ConnectionString));
                    break;
                case DatabaseType.Sqlite:
                    optionsBuilder.UseSqlite(GlobalDbSettings.ConnectionString);
                    break;

            }
        }
    }
   
}
