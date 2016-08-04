using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Reflection;
using Drive.Logging.Context;
using Drive.Logging.Models;
using NLog;

namespace Drive.Logging
{
    public class Logging
    {
        public Logger Log { get; }

        public Logging(Logger log)
        {
            LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(@"C:\FILES\GitHub\NLog\BinaryDrive-NLog\Drive.Logging\NLog.config", true);

            Log = log;

            using (DriveContext context = new DriveContext())
            {
                context.LogEntries.Add(new LogEntry());
                context.SaveChanges();
            }
        }
    }
}
