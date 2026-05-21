using System;
using System.Linq;
using System.Threading;
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
            bool serviceMode = args.Any(IsServiceArgument);
            string configPath = args.FirstOrDefault(arg => !IsServiceArgument(arg)) ?? "appsettings.json";
            BridgeLogger.Info("Bridge starting. Config argument: " + configPath);

            BridgeOptions options = BridgeOptionsLoader.Load(configPath);
            ServiceRegistry.Initialize(options);
            using (WebApp.Start<Startup>(options.BaseUrl))
            {
                BridgeLogger.Info("Bridge started at " + options.BaseUrl);
                WaitForStop(serviceMode);
            }

            BridgeLogger.Info("Bridge stopped.");
        }

        private static bool IsServiceArgument(string arg)
        {
            return string.Equals(arg, "--service", StringComparison.OrdinalIgnoreCase);
        }

        private static void WaitForStop(bool serviceMode)
        {
            if (serviceMode)
            {
                BridgeLogger.Info("Bridge is running in service mode.");
                new ManualResetEvent(false).WaitOne();
                return;
            }

            Console.WriteLine("Press ENTER to stop.");
            Console.ReadLine();
        }
    }
}
