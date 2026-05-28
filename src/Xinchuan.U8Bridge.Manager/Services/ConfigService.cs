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
            if (config.ConfigSchemaVersion <= 0)
            {
                config.ConfigSchemaVersion = BridgeConfig.CurrentConfigSchemaVersion;
            }

            if (config.ConfigSchemaVersion > BridgeConfig.CurrentConfigSchemaVersion)
            {
                throw new InvalidOperationException("配置版本高于当前管理器支持版本，请使用匹配的新版本管理器。");
            }

            if (string.IsNullOrWhiteSpace(config.BaseUrl))
            {
                config.BaseUrl = "http://+:8081/";
            }

            if (string.IsNullOrWhiteSpace(config.DefaultProfileName))
            {
                config.DefaultProfileName = "prod-100";
            }

            config.Database = config.Database ?? new U8DatabaseConfig();
            config.Update = config.Update ?? new UpdateCheckConfig();
            NormalizeUpdate(config.Update);
            config.Profiles = config.Profiles ?? new Dictionary<string, U8ProfileConfig>();
            if (!config.Profiles.Any())
            {
                config.Profiles[config.DefaultProfileName] = new U8ProfileConfig();
            }
        }

        private static void NormalizeUpdate(UpdateCheckConfig update)
        {
            update.AutoCheckIntervalMinutes = Clamp(
                update.AutoCheckIntervalMinutes,
                UpdateCheckConfig.MinAutoCheckIntervalMinutes,
                UpdateCheckConfig.MaxAutoCheckIntervalMinutes);
            update.AutoCheckStartupDelaySeconds = Clamp(
                update.AutoCheckStartupDelaySeconds,
                UpdateCheckConfig.MinAutoCheckStartupDelaySeconds,
                UpdateCheckConfig.MaxAutoCheckStartupDelaySeconds);
        }

        private static int Clamp(int value, int minimum, int maximum)
        {
            if (value < minimum)
            {
                return minimum;
            }

            return value > maximum ? maximum : value;
        }
    }
}
