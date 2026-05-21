using System;
using Microsoft.Owin.Hosting;
using Xinchuan.U8Bridge.Configuration;
using Xinchuan.U8Bridge.Services;

namespace Xinchuan.U8Bridge
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Run(args);
            }
            catch (Exception ex)
            {
                BridgeLogger.Error("Bridge startup failed.", ex);
                Console.Error.WriteLine("Startup failed. See logs: " + BridgeLogger.LogDirectory);
                Environment.ExitCode = 1;
            }
        }

        private static void Run(string[] args)
        {
            string configPath = args.Length > 0 ? args[0] : "appsettings.json";
            BridgeLogger.Info("Bridge starting. Config argument: " + configPath);

            BridgeOptions options = BridgeOptionsLoader.Load(configPath);
            ServiceRegistry.Initialize(options);
            using (WebApp.Start<Startup>(options.BaseUrl))
            {
                BridgeLogger.Info("Bridge started at " + options.BaseUrl);
                Console.WriteLine("Press ENTER to stop.");
                Console.ReadLine();
            }

            BridgeLogger.Info("Bridge stopped.");
        }
    }
}
