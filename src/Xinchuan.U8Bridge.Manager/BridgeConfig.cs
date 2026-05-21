using System.Collections.Generic;

namespace Xinchuan.U8Bridge.Manager
{
    public sealed class BridgeConfig
    {
        public string BaseUrl { get; set; } = "http://+:8081/";

        public string ApiKey { get; set; } = "local-dev-u8-bridge-key";

        public string DefaultProfileName { get; set; } = "prod-100";

        public IList<string> AllowedSourceIps { get; set; } = new List<string>();

        public string U8Mode { get; set; } = "dryRun";

        public IDictionary<string, U8ProfileConfig> Profiles { get; set; } =
            new Dictionary<string, U8ProfileConfig>();
    }

    public sealed class U8ProfileConfig
    {
        public string SubId { get; set; } = "AS";

        public string AccountId { get; set; } = "(default)@100";

        public string Year { get; set; } = "2018";

        public string UserId { get; set; } = "168";

        public string Password { get; set; }

        public string LoginDate { get; set; } = "2026-05-21";

        public string Server { get; set; } = "100";

        public string Serial { get; set; } = string.Empty;
    }
}
