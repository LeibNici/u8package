using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Xinchuan.U8Bridge.Manager.Services
{
    public sealed class UpdateInstallService
    {
        private const string UpdatesDirectoryName = "updates";
        private const string PackageFileName = "package.zip";
        private const string UpgradeScriptName = "install-update.cmd";

        public async Task<string> PrepareAsync(UpdateCheckResult result, string baseDirectory)
        {
            string normalizedBaseDirectory = NormalizePath(baseDirectory, "Bridge 程序目录");
            Validate(result, normalizedBaseDirectory);
            string updateDirectory = CreateUpdateDirectory(normalizedBaseDirectory, result.LatestVersion);
            string packagePath = Path.Combine(updateDirectory, PackageFileName);
            await DownloadAsync(result.DownloadUrl, packagePath).ConfigureAwait(false);
            string scriptPath = Path.Combine(updateDirectory, UpgradeScriptName);
            File.WriteAllText(
                scriptPath,
                BuildScript(normalizedBaseDirectory, updateDirectory, packagePath),
                Encoding.Default);
            return scriptPath;
        }

        public void Launch(string scriptPath)
        {
            if (string.IsNullOrWhiteSpace(scriptPath) || !File.Exists(scriptPath))
            {
                throw new FileNotFoundException("更新脚本不存在", scriptPath);
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = scriptPath,
                WorkingDirectory = Path.GetDirectoryName(scriptPath),
                UseShellExecute = true
            });
        }

        private static void Validate(UpdateCheckResult result, string baseDirectory)
        {
            if (result == null || !result.HasUpdate)
            {
                throw new InvalidOperationException("没有可安装的新版本");
            }

            if (!result.ConfigCompatible)
            {
                throw new InvalidOperationException(result.ConfigMessage);
            }

            if (string.IsNullOrWhiteSpace(result.DownloadUrl))
            {
                throw new InvalidOperationException("新版本缺少下载地址");
            }

            if (string.IsNullOrWhiteSpace(baseDirectory) || !Directory.Exists(baseDirectory))
            {
                throw new DirectoryNotFoundException("Bridge 程序目录不存在: " + baseDirectory);
            }
        }

        private static string CreateUpdateDirectory(string baseDirectory, string latestVersion)
        {
            string updatesRoot = Path.Combine(baseDirectory, UpdatesDirectoryName);
            string name = SafeName(latestVersion) + "-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            string updateDirectory = Path.Combine(updatesRoot, name);
            Directory.CreateDirectory(updateDirectory);
            return updateDirectory;
        }

        private static async Task DownloadAsync(string url, string packagePath)
        {
            using (var client = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Xinchuan-U8Bridge-Manager");
                byte[] bytes = await client.GetByteArrayAsync(url).ConfigureAwait(false);
                File.WriteAllBytes(packagePath, bytes);
            }
        }

        private static string BuildScript(string baseDirectory, string updateDirectory, string packagePath)
        {
            string extractDirectory = NormalizePath(Path.Combine(updateDirectory, "extract"), "更新解压目录");
            string normalizedPackagePath = NormalizePath(packagePath, "更新包路径");
            string backupDirectory = Path.Combine(
                baseDirectory,
                "backup-before-update-" + DateTime.Now.ToString("yyyyMMddHHmmss"));
            var script = new StringBuilder();
            script.AppendLine("@echo off");
            script.AppendLine("setlocal");
            script.AppendLine("set \"BASE=" + baseDirectory + "\"");
            script.AppendLine("set \"ZIP=" + normalizedPackagePath + "\"");
            script.AppendLine("set \"EXTRACT=" + extractDirectory + "\"");
            script.AppendLine("set \"BACKUP=" + backupDirectory + "\"");
            script.AppendLine("echo Waiting for U8 Bridge Manager to exit...");
            script.AppendLine("timeout /t 2 /nobreak >nul");
            script.AppendLine("taskkill /F /IM Xinchuan.U8Bridge.exe >nul 2>nul");
            script.AppendLine("taskkill /F /IM Xinchuan.U8Bridge.Manager.exe >nul 2>nul");
            script.AppendLine("mkdir \"%BACKUP%\" >nul 2>nul");
            script.AppendLine("robocopy \"%BASE%\" \"%BACKUP%\" /E /XD updates logs backup-before-update-* /R:1 /W:1 >nul");
            script.AppendLine("powershell -NoProfile -ExecutionPolicy Bypass -Command \"Expand-Archive -LiteralPath '%ZIP%' -DestinationPath '%EXTRACT%' -Force\"");
            script.AppendLine("if errorlevel 1 goto failed");
            script.AppendLine("robocopy \"%EXTRACT%\" \"%BASE%\" /E /XD updates logs /XF appsettings.json /R:2 /W:1");
            script.AppendLine("if errorlevel 8 goto failed");
            script.AppendLine("start \"\" \"%BASE%\\Xinchuan.U8Bridge.Manager.exe\"");
            script.AppendLine("exit /b 0");
            script.AppendLine(":failed");
            script.AppendLine("echo Update failed. Backup directory: %BACKUP%");
            script.AppendLine("pause");
            script.AppendLine("exit /b 1");
            return script.ToString();
        }

        private static string NormalizePath(string value, string label)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DirectoryNotFoundException(label + "不能为空");
            }

            string normalized = Path.GetFullPath(value.Trim().Trim('"'));
            if (normalized.IndexOf('"') >= 0)
            {
                throw new InvalidOperationException(label + "不能包含引号: " + normalized);
            }

            return normalized;
        }

        private static string SafeName(string value)
        {
            string safe = Regex.Replace(value ?? "unknown", @"[^A-Za-z0-9_.-]+", "-");
            return string.IsNullOrWhiteSpace(safe) ? "unknown" : safe;
        }
    }
}
