using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Migrations;

namespace DataBaseTools.net48
{
    public class ToolsDataContext:DbContext
    {
        public ToolsDataContext() : base("ToolsDataContext")
        {

        }
    }
}
