using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Xinchuan.U8Bridge.Manager.Services
{
    public sealed class ConfigService
    {
        public string BaseDirectory { get; } = AppDomain.CurrentDomain.BaseDirectory;

        public string ConfigPath => Path.Combine(BaseDirectory, "appsettings.json");

        public string LogDirectory => Path.Combine(BaseDirectory, "logs");

        public BridgeConfig LoadOrDefault()
        {
            if (!File.Exists(ConfigPath))
            {
                return CreateDefault();
            }

            string json = File.ReadAllText(ConfigPath);
            var config = JsonConvert.DeserializeObject<BridgeConfig>(json) ?? CreateDefault();
            Normalize(config);
            return config;
        }

        public void Save(BridgeConfig config)
        {
            Normalize(config);
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(ConfigPath, json);
        }

        public string GetTodayLogPath()
        {
            string fileName = "u8-bridge-" + DateTime.Now.ToString("yyyyMMdd") + ".log";
            return Path.Combine(LogDirectory, fileName);
        }

        private static BridgeConfig CreateDefault()
        {
            var config = new BridgeConfig();
            config.Profiles[config.DefaultProfileName] = new U8ProfileConfig();
            return config;
        }

        private static void Normalize(BridgeConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.BaseUrl))
            {
                config.BaseUrl = "http://+:8081/";
            }

            if (string.IsNullOrWhiteSpace(config.DefaultProfileName))
            {
                config.DefaultProfileName = "prod-100";
            }

            config.Database = config.Database ?? new U8DatabaseConfig();
            config.Profiles = config.Profiles ?? new Dictionary<string, U8ProfileConfig>();
            if (!config.Profiles.Any())
            {
                config.Profiles[config.DefaultProfileName] = new U8ProfileConfig();
            }
        }
    }
}
