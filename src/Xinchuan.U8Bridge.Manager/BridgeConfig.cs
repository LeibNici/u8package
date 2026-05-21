using System.Collections.Generic;
using Newtonsoft.Json;

namespace Xinchuan.U8Bridge.Manager
{
    public sealed class BridgeConfig
    {
        public string BaseUrl { get; set; } = "http://+:8081/";

        public string ApiKey { get; set; } = "local-dev-u8-bridge-key";

        public string DefaultProfileName { get; set; } = "prod-100";

        public string U8Mode { get; set; } = "dryRun";

        public U8DatabaseConfig Database { get; set; } = new U8DatabaseConfig();

        public IDictionary<string, U8ProfileConfig> Profiles { get; set; } =
            new Dictionary<string, U8ProfileConfig>();
    }

    public sealed class U8ProfileConfig
    {
        [JsonIgnore]
        public string SubId { get; set; } = "AS";

        [JsonIgnore]
        public string AccountId { get; set; }

        [JsonIgnore]
        public string Year { get; set; }

        public string UserId { get; set; } = "168";

        public string Password { get; set; }

        public string LoginDate { get; set; } = "2026-05-21";

        public string Server { get; set; } = "100";

        [JsonIgnore]
        public string Serial { get; set; } = string.Empty;
    }

    public sealed class U8DatabaseConfig
    {
        public bool Enabled { get; set; }

        public string Server { get; set; } = "192.168.2.16";

        public string Database { get; set; } = "ufdata_100_2018";

        public string User { get; set; } = "link100";

        public string Password { get; set; }
    }
}
