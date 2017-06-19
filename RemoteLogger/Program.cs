using System;
using System.IO;
using System.Runtime.Remoting;
using System.Threading;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;

namespace RemoteLogger
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger("RemoteLogger");
        private static readonly ILog ErrorLog = LogManager.GetLogger("ErrorLog");
        private static readonly ManualResetEvent Shutdown = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;
            
            Environment.SetEnvironmentVariable("ServerLogFolder", args.Length > 0 ? args[0] : "./logs");
            GlobalContext.Properties["ServerName"] = "RLogger";

            XmlConfigurator.Configure(new FileInfo("log4net_rl.config"));
            
            RemotingConfiguration.Configure(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, false);
            RemotingConfiguration.RegisterWellKnownServiceType(
                new WellKnownServiceTypeEntry(typeof(RemoteSink), 
                "LoggingSink", 
                WellKnownObjectMode.SingleCall));

            Console.CancelKeyPress += delegate
            {
                Stop();
            };

            Log.Info("Remote logger started...");
            Shutdown.WaitOne();
        }

        public static void Stop()
        {
            Shutdown.Set();
        }

        private static void LogUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            ErrorLog.Fatal((Exception)args.ExceptionObject);
            Stop();
        }
    }

    public class RemoteSink : MarshalByRefObject, RemotingAppender.IRemoteLoggingSink
    {
        private static readonly ILog RemoteLog = LogManager.GetLogger("RemoteLogger");
        private static readonly ILog WServerLog = LogManager.GetLogger("WServerLog");
        private static readonly ILog AppEngineLog = LogManager.GetLogger("AppEngineLog");
        private static readonly ILog ErrorLog = LogManager.GetLogger("ErrorLog");
        private static readonly ILog PassLog = LogManager.GetLogger("PassLog");
        private static readonly ILog CheatLog = LogManager.GetLogger("CheatLog");
        private static readonly ILog RankManagerLog = LogManager.GetLogger("RankManagerLog");
        private static readonly ILog NameChangeLog = LogManager.GetLogger("NameChangeLog");

        public void LogEvents(LoggingEvent[] events)
        {
            foreach (var logEvent in events)
            {
                if (logEvent.Level >= Level.Error)
                {
                    ErrorLog.Logger.Log(logEvent);
                }
                
                if (logEvent.Properties.Contains("ServerType"))
                {
                    var svrType = (string) logEvent.Properties["ServerType"];
                    switch (svrType)
                    {
                        case "Account":
                            AppEngineLog.Logger.Log(logEvent);
                            break;
                        case "World":
                            WServerLog.Logger.Log(logEvent);
                            break;
                    }
                }

                switch (logEvent.LoggerName)
                {
                    case "PassLog":
                        PassLog.Logger.Log(logEvent);
                        break;
                    case "CheatLog":
                        CheatLog.Logger.Log(logEvent);
                        break;
                    case "RankManagerLog":
                        RankManagerLog.Logger.Log(logEvent);
                        break;
                    case "NameChangeLog":
                        NameChangeLog.Logger.Log(logEvent);
                        break;
                }

                RemoteLog.Logger.Log(logEvent);
            }
        }
    }
}
