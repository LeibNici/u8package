namespace Xinchuan.U8Bridge.Manager.Services
{
    public sealed class UpdateCheckResult
    {
        public string CurrentVersion { get; set; }

        public string LatestVersion { get; set; }

        public bool HasUpdate { get; set; }

        public int CurrentConfigSchemaVersion { get; set; }

        public int LatestConfigSchemaVersion { get; set; }

        public int MinConfigSchemaVersion { get; set; }

        public bool ConfigCompatible { get; set; }

        public bool RequiresConfigMigration { get; set; }

        public string ConfigMessage { get; set; }

        public string ReleaseUrl { get; set; }

        public string DownloadUrl { get; set; }

        public string Notes { get; set; }

        public string Source { get; set; }
    }
}
