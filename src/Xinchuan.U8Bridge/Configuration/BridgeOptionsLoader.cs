using System;
using System.IO;
using Newtonsoft.Json;

namespace Xinchuan.U8Bridge.Configuration
{
    public static class BridgeOptionsLoader
    {
        public static BridgeOptions Load(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Bridge config file was not found.", path);
            }

            var json = File.ReadAllText(path);
            var options = JsonConvert.DeserializeObject<BridgeOptions>(json) ?? new BridgeOptions();
            Normalize(options);
            return options;
        }

        private static void Normalize(BridgeOptions options)
        {
            options.ApiKey = ResolveEnv(options.ApiKey);
            foreach (var profile in options.Profiles.Values)
            {
                profile.Password = ResolveEnv(profile.Password);
            }

            if (string.IsNullOrWhiteSpace(options.ApiKey) || options.ApiKey.Contains("CHANGE_ME"))
            {
                throw new InvalidOperationException("apiKey is required and must not use a default value.");
            }

            if (options.Profiles == null || options.Profiles.Count == 0)
            {
                throw new InvalidOperationException("At least one U8 login profile is required.");
            }
        }

        private static string ResolveEnv(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.StartsWith("${") || !value.EndsWith("}"))
            {
                return value;
            }

            string name = value.Substring(2, value.Length - 3);
            return Environment.GetEnvironmentVariable(name);
        }
    }
}
