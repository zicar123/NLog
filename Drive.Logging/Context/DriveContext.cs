using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Drive.Logging.Models;

namespace Drive.Logging.Context
{
    public class DriveContext : DbContext
    {
        public DriveContext() : base("DriveDB")
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<DriveContext>());
        }
        public DbSet<LogEntry> LogEntries { get; set; }
    }
}
