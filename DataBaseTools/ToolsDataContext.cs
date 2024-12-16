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
        private string connectionString = @"Data Source=JYJ;Initial Catalog=VacuumSystem2;User ID=sa;Password=123456;MultipleActiveResultSets=true";
        public ToolsDataContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}
