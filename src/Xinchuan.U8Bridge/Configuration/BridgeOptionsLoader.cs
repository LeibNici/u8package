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
            if (options.ConfigSchemaVersion <= 0)
            {
                options.ConfigSchemaVersion = BridgeOptions.CurrentConfigSchemaVersion;
            }

            if (options.ConfigSchemaVersion > BridgeOptions.CurrentConfigSchemaVersion)
            {
                throw new InvalidOperationException("Bridge config schema is newer than this bridge version supports.");
            }

            if (string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                options.BaseUrl = "http://+:8081/";
            }

            if (options.Profiles == null)
            {
                options.Profiles = new Dictionary<string, U8ProfileOptions>();
            }

            options.ApiKey = ResolveEnv(options.ApiKey);
            options.Database = options.Database ?? new U8DatabaseOptions();
            options.Database.Password = ResolveEnv(options.Database.Password);
            foreach (var profile in options.Profiles.Values)
            {
                profile.Password = ResolveEnv(profile.Password);
                NormalizeProfile(profile, options.Database);
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

        private static void NormalizeProfile(U8ProfileOptions profile, U8DatabaseOptions database)
        {
            if (string.IsNullOrWhiteSpace(profile.SubId))
            {
                profile.SubId = "AS";
            }

            if (string.IsNullOrWhiteSpace(profile.AccountId))
            {
                string accountSet = InferAccountSet(database?.Database) ?? profile.Server;
                profile.AccountId = string.IsNullOrWhiteSpace(accountSet) ? null : "(default)@" + accountSet;
            }

            if (string.IsNullOrWhiteSpace(profile.Year))
            {
                profile.Year = InferYear(database?.Database) ?? InferYear(profile.LoginDate);
            }

            if (profile.Serial == null)
            {
                profile.Serial = string.Empty;
            }
        }

        private static string InferAccountSet(string databaseName)
        {
            string[] parts = SplitDatabaseName(databaseName);
            return parts == null ? null : parts[1];
        }

        private static string InferYear(string value)
        {
            string[] parts = SplitDatabaseName(value);
            if (parts != null)
            {
                return parts[2];
            }

            DateTime parsed;
            return DateTime.TryParse(value, out parsed) ? parsed.Year.ToString() : null;
        }

        private static string[] SplitDatabaseName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string[] parts = value.Split('_');
            return parts.Length == 3 && parts[0].Equals("ufdata", StringComparison.OrdinalIgnoreCase)
                ? parts
                : null;
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
