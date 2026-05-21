namespace Xinchuan.U8Bridge.Manager.Services
{
    public sealed class UpdateCheckResult
    {
        public string CurrentVersion { get; set; }

        public string LatestVersion { get; set; }

        public bool HasUpdate { get; set; }

        public string ReleaseUrl { get; set; }

        public string DownloadUrl { get; set; }

        public string Notes { get; set; }

        public string Source { get; set; }
    }
}
