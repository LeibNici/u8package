using System;
using System.Diagnostics;

namespace Xinchuan.U8Bridge.Manager.Services
{
    public sealed class UrlAclService
    {
        public string BuildCommand(string baseUrl)
        {
            string url = NormalizeUrl(baseUrl);
            return "/c netsh http delete urlacl url=\"" + url + "\""
                + " & netsh http add urlacl url=\"" + url + "\" sddl=\"D:(A;;GX;;;WD)\"";
        }

        public void Configure(string baseUrl)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = BuildCommand(baseUrl),
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Normal
            };
            Process.Start(startInfo);
        }

        private static string NormalizeUrl(string baseUrl)
        {
            string value = string.IsNullOrWhiteSpace(baseUrl) ? "http://+:8081/" : baseUrl.Trim();
            if (!value.EndsWith("/", StringComparison.Ordinal))
            {
                value += "/";
            }

            return value;
        }
    }
}
