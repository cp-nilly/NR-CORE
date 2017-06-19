using System;
using System.Threading;
using common;
using common.resources;
using wServer.networking.server;
using wServer.realm;
using wServer.networking;
using System.Globalization;
using log4net;
using log4net.Config;
using System.IO;
using System.Threading.Tasks;

namespace wServer
{
    static class Program
    {
        internal static ServerConfig Config;
        internal static Resources Resources;

        static readonly ILog Log = LogManager.GetLogger("wServer");

        private static readonly ManualResetEvent Shutdown = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.Name = "Entry";

            Config = args.Length > 0 ? 
                ServerConfig.ReadFile(args[0]) : 
                ServerConfig.ReadFile("wServer.json");

            Environment.SetEnvironmentVariable("ServerLogFolder", Config.serverSettings.logFolder);
            GlobalContext.Properties["ServerName"] = Config.serverInfo.name;
            GlobalContext.Properties["ServerType"] = Config.serverInfo.type.ToString();

            XmlConfigurator.ConfigureAndWatch(new FileInfo(Config.serverSettings.log4netConfig));
            
            using (Resources = new Resources(Config.serverSettings.resourceFolder, true))
            using (var db = new Database(
                Config.dbInfo.host,
                Config.dbInfo.port,
                Config.dbInfo.auth,
                Config.dbInfo.index,
                Resources))
            {
                var manager = new RealmManager(Resources, db, Config);
                manager.Run();

                var policy = new PolicyServer();
                policy.Start();

                var server = new Server(manager, 
                    Config.serverInfo.port,
                    Config.serverSettings.maxConnections,
                    StringUtils.StringToByteArray(Config.serverSettings.key));
                server.Start();
                
                Console.CancelKeyPress += delegate
                {
                    Shutdown.Set();
                };

                Shutdown.WaitOne();

                Log.Info("Terminating...");
                manager.Stop();
                server.Stop();
                policy.Stop();
                Log.Info("Server terminated.");
            }
        }

        public static void Stop(Task task = null)
        {
            if (task != null)
                Log.Fatal(task.Exception);

            Shutdown.Set();
        }

        private static void LogUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Log.Fatal((Exception)args.ExceptionObject);
        }
    }
}
