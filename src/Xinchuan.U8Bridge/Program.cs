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
            string configPath = args.Length > 0 ? args[0] : "appsettings.json";
            BridgeOptions options = BridgeOptionsLoader.Load(configPath);
            ServiceRegistry.Initialize(options);

            using (WebApp.Start<Startup>(options.BaseUrl))
            {
                Console.WriteLine("Xinchuan U8 Bridge started at " + options.BaseUrl);
                Console.WriteLine("Press ENTER to stop.");
                Console.ReadLine();
            }
        }
    }
}
