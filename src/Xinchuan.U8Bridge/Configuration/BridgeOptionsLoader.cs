using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Xinchuan.U8Bridge.Services;

namespace Xinchuan.U8Bridge.Configuration
{
    public static class BridgeOptionsLoader
    {
        public static BridgeOptions Load(string path)
        {
            string resolvedPath = ResolvePath(path);
            BridgeLogger.Info("Loading bridge config: " + resolvedPath);

            if (!File.Exists(resolvedPath))
            {
                throw new FileNotFoundException(
                    "Bridge config file was not found. Copy appsettings.sample.json to appsettings.json and update apiKey and U8 profile.",
                    resolvedPath);
            }

            try
            {
                var json = File.ReadAllText(resolvedPath);
                var options = JsonConvert.DeserializeObject<BridgeOptions>(json) ?? new BridgeOptions();
                Normalize(options);
                BridgeLogger.Info(
                    "Bridge config loaded. Mode="
                    + options.U8Mode
                    + ", BaseUrl="
                    + options.BaseUrl
                    + ", DefaultProfile="
                    + options.DefaultProfileName
                    + ", Profiles="
                    + options.Profiles.Count);
                return options;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Bridge config JSON is invalid: " + resolvedPath, ex);
            }
        }

        private static void Normalize(BridgeOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                options.BaseUrl = "http://+:8081/";
            }

            if (options.AllowedSourceIps == null)
            {
                options.AllowedSourceIps = new List<string>();
            }

            if (options.Profiles == null)
            {
                options.Profiles = new Dictionary<string, U8ProfileOptions>();
            }

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

        private static string ResolvePath(string path)
        {
            string candidate = string.IsNullOrWhiteSpace(path) ? "appsettings.json" : path;
            if (Path.IsPathRooted(candidate))
            {
                return candidate;
            }

            string currentDirectoryPath = Path.GetFullPath(candidate);
            if (File.Exists(currentDirectoryPath))
            {
                return currentDirectoryPath;
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, candidate);
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
