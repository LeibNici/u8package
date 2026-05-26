using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Xinchuan.U8Bridge.Manager.Services
{
    public sealed class UpdateCheckService
    {
        public async Task<UpdateCheckResult> CheckAsync(
            UpdateCheckConfig config,
            string baseDirectory,
            int currentConfigSchemaVersion)
        {
            if (config == null || !config.Enabled)
            {
                throw new InvalidOperationException("更新检测未启用");
            }

            string json = await DownloadJsonAsync(config.CheckUrl).ConfigureAwait(false);
            JToken token = JToken.Parse(json);
            UpdateCheckResult result = ParseResult(config, token);
            result.CurrentVersion = ResolveCurrentVersion(baseDirectory);
            result.HasUpdate = CompareVersion(result.LatestVersion, result.CurrentVersion) > 0;
            EvaluateConfigCompatibility(result, currentConfigSchemaVersion);
            return result;
        }

        private static async Task<string> DownloadJsonAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new InvalidOperationException("更新检测地址不能为空");
            }

            using (var client = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Xinchuan-U8Bridge-Manager");
                return await client.GetStringAsync(url).ConfigureAwait(false);
            }
        }

        private static UpdateCheckResult ParseResult(UpdateCheckConfig config, JToken token)
        {
            if (string.Equals(config.SourceType, "manifest", StringComparison.OrdinalIgnoreCase))
            {
                return ParseManifest((JObject)token);
            }

            JToken release = token.Type == JTokenType.Array
                ? token.Children<JObject>().FirstOrDefault(r => IncludeRelease(r, config))
                : token;
            return ParseGithubRelease((JObject)release);
        }

        private static bool IncludeRelease(JObject release, UpdateCheckConfig config)
        {
            bool draft = release.Value<bool?>("draft") == true;
            bool prerelease = release.Value<bool?>("prerelease") == true;
            return !draft && (config.IncludePrerelease || !prerelease);
        }

        private static UpdateCheckResult ParseGithubRelease(JObject release)
        {
            if (release == null)
            {
                throw new InvalidOperationException("未找到可用 GitHub Release");
            }

            JToken asset = release["assets"]?.FirstOrDefault();
            return new UpdateCheckResult
            {
                Source = "githubRelease",
                LatestVersion = Value(release, "tag_name"),
                ReleaseUrl = Value(release, "html_url"),
                DownloadUrl = Value(asset as JObject, "browser_download_url"),
                Notes = Value(release, "body"),
                LatestConfigSchemaVersion = BodyInt(release, "ConfigSchemaVersion", BridgeConfig.CurrentConfigSchemaVersion),
                MinConfigSchemaVersion = BodyInt(release, "MinConfigSchemaVersion", BridgeConfig.CurrentConfigSchemaVersion)
            };
        }

        private static UpdateCheckResult ParseManifest(JObject manifest)
        {
            return new UpdateCheckResult
            {
                Source = "manifest",
                LatestVersion = Value(manifest, "version"),
                ReleaseUrl = Value(manifest, "releaseUrl"),
                DownloadUrl = Value(manifest, "downloadUrl"),
                Notes = Value(manifest, "notes"),
                LatestConfigSchemaVersion = IntValue(manifest, "configSchemaVersion", BridgeConfig.CurrentConfigSchemaVersion),
                MinConfigSchemaVersion = IntValue(manifest, "minConfigSchemaVersion", BridgeConfig.CurrentConfigSchemaVersion)
            };
        }

        private static void EvaluateConfigCompatibility(UpdateCheckResult result, int currentConfigSchemaVersion)
        {
            result.CurrentConfigSchemaVersion = currentConfigSchemaVersion <= 0
                ? BridgeConfig.CurrentConfigSchemaVersion
                : currentConfigSchemaVersion;
            if (result.MinConfigSchemaVersion <= 0)
            {
                result.MinConfigSchemaVersion = BridgeConfig.CurrentConfigSchemaVersion;
            }

            if (result.LatestConfigSchemaVersion <= 0)
            {
                result.LatestConfigSchemaVersion = result.MinConfigSchemaVersion;
            }

            result.ConfigCompatible = result.MinConfigSchemaVersion <= result.CurrentConfigSchemaVersion;
            result.RequiresConfigMigration = result.LatestConfigSchemaVersion > result.CurrentConfigSchemaVersion;
            result.ConfigMessage = BuildConfigMessage(result);
        }

        private static string BuildConfigMessage(UpdateCheckResult result)
        {
            if (!result.ConfigCompatible)
            {
                return "新版本要求配置 schema >= " + result.MinConfigSchemaVersion + "，当前配置不能直接升级";
            }

            if (result.RequiresConfigMigration)
            {
                return "新版本会迁移配置到 schema " + result.LatestConfigSchemaVersion + "，升级前请备份 appsettings.json";
            }

            return "配置兼容，可直接升级";
        }

        public static string ResolveCurrentVersion(string baseDirectory)
        {
            string manifest = Path.Combine(baseDirectory, "package-version.json");
            if (File.Exists(manifest))
            {
                string version = Value(JObject.Parse(File.ReadAllText(manifest)), "version");
                if (!string.IsNullOrWhiteSpace(version))
                {
                    return version.Trim();
                }
            }

            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private static int CompareVersion(string latest, string current)
        {
            Version left = ParseVersion(latest);
            Version right = ParseVersion(current);
            return left.CompareTo(right);
        }

        private static Version ParseVersion(string value)
        {
            MatchCollection matches = Regex.Matches(value ?? "0", @"\d+(?:\.\d+)*");
            string normalized = matches.Count == 0 ? "0" : matches[matches.Count - 1].Value;
            return Version.TryParse(string.IsNullOrWhiteSpace(normalized) ? "0" : normalized, out var version)
                ? version
                : new Version(0, 0);
        }

        private static string Value(JObject obj, string property)
        {
            return obj == null ? null : obj.Value<string>(property);
        }

        private static int IntValue(JObject obj, string property, int defaultValue)
        {
            int? value = obj == null ? null : obj.Value<int?>(property);
            return value ?? defaultValue;
        }

        private static int BodyInt(JObject release, string name, int defaultValue)
        {
            string body = Value(release, "body");
            if (string.IsNullOrWhiteSpace(body))
            {
                return defaultValue;
            }

            Match match = Regex.Match(body, @"(?im)^" + Regex.Escape(name) + @":\s*(\d+)\s*$");
            return match.Success ? int.Parse(match.Groups[1].Value) : defaultValue;
        }
    }
}
