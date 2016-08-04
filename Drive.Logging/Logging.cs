using System.Text;
using System.Data.SqlClient;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Drive.Logging
{
    public class Logging
    {
        public Logger Log { get; }

        public Logging(Logger log)
        {
            Configure(@"Server = (localdb)\MSSQLLocalDB; Database = DriveDB; Integrated Security = true; Trusted_Connection = True");
            //LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(@"C:\FILES\GitHub\NLog\NLog\Drive.Logging\NLog.config", true);
            Log = log;
        }

        private void Configure(string strConnectionString)
        {
            var sb = new StringBuilder();
            var installationContext = new InstallationContext();

            var builder = new SqlConnectionStringBuilder {ConnectionString = strConnectionString};
            var strDatabase = builder.InitialCatalog;

            var targetDb = new DatabaseTarget();
            var targetFile = new FileTarget();


            //Target properties
            targetDb.Name = "DriveDB";
            targetDb.ConnectionString = strConnectionString;

            targetFile.FileName = "${specialfolder:folder=LocalApplicationData}/Logs/log ${shortdate}.log"; //  C:\Users\Zicar\AppData\Local\Logs
            targetFile.Layout = "${longdate} | ${uppercase:${level}} | ${logger} | ${message} | ${exception:format=tostring}";
            targetFile.KeepFileOpen = false;
            targetFile.Encoding = Encoding.UTF8;


            
            targetDb.Parameters.Add(new DatabaseParameterInfo { Name = "@Date", Layout = string.Format("${{longdate}}") });
            targetDb.Parameters.Add(new DatabaseParameterInfo { Name = "@Level", Layout = string.Format("${{level}}") });
            targetDb.Parameters.Add(new DatabaseParameterInfo { Name = "@Logger", Layout = string.Format("${{logger}}") });
            targetDb.Parameters.Add(new DatabaseParameterInfo { Name = "@Message", Layout = string.Format("${{message}}") });

            targetDb.CommandText = "INSERT INTO LogEntries(Date, Level, Logger, Message) VALUES (@Date, @Level, @Logger, @Message);";

            //Config object
            var config = LogManager.Configuration ?? new LoggingConfiguration();
            config.AddTarget(targetDb.Name, targetDb);

            //Rules
            var dbRule = new LoggingRule("*", LogLevel.Debug, targetDb);
            config.LoggingRules.Add(dbRule);
            var fileRule = new LoggingRule("*", LogLevel.Debug, targetFile);
            config.LoggingRules.Add(fileRule);

            LogManager.Configuration = config;

            var builder2 = new SqlConnectionStringBuilder
            {
                ConnectionString = strConnectionString,
                InitialCatalog = "master"
            };

            targetDb.InstallConnectionString = builder2.ConnectionString;

            sb.AppendLine(string.Format("IF NOT EXISTS (SELECT name FROM master.sys.databases WHERE name = N'{0}')", strDatabase));
            sb.AppendLine(string.Format("CREATE DATABASE {0}", strDatabase));

            var createDbCommand = new DatabaseCommandInfo
            {
                Text = sb.ToString(),
                CommandType = System.Data.CommandType.Text
            };
            targetDb.InstallDdlCommands.Add(createDbCommand);

            // create the database if it does not exist
            targetDb.Install(installationContext);

            targetDb.InstallDdlCommands.Clear();
            sb.Clear();
            sb.AppendLine("IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'LogEntries')");
            sb.AppendLine("RETURN");
            sb.AppendLine("");
            sb.AppendLine("CREATE TABLE [dbo].[LogEntries](");
            sb.AppendLine("[LogId] [int] IDENTITY(1,1) NOT NULL,");
            sb.AppendLine("[Date] [nvarchar](max) NULL,");
            sb.AppendLine("[Level] [nvarchar](max) NULL,");
            sb.AppendLine("[Logger] [nvarchar](max) NULL,");
            sb.AppendLine("[Message] [nvarchar](max) NULL,");
            sb.AppendLine(" CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED ");
            sb.AppendLine("(");
            sb.AppendLine("[LogId] ASC");
            sb.AppendLine(")WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]");
            sb.AppendLine(") ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]");

            var createTableCommand = new DatabaseCommandInfo
            {
                Text = sb.ToString(),
                CommandType = System.Data.CommandType.Text
            };
            targetDb.InstallDdlCommands.Add(createTableCommand);

            targetDb.InstallConnectionString = strConnectionString;

            // create the table if it does not exist
            targetDb.Install(installationContext);
        }
    }
}

//Example of use:
//  Logging.Logging log = new Logging.Logging(LogManager.GetCurrentClassLogger());
//  log.Log.Trace("message");