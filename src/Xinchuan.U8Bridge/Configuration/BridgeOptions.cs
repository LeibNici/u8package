using System.Collections.Generic;

namespace Xinchuan.U8Bridge.Configuration
{
    public sealed class BridgeOptions
    {
        public string BaseUrl { get; set; } = "http://+:8081/";

        public string ApiKey { get; set; }

        public string DefaultProfileName { get; set; } = "prod-100";

        public string U8Mode { get; set; } = "official";

        public U8DatabaseOptions Database { get; set; } = new U8DatabaseOptions();

        public IDictionary<string, U8ProfileOptions> Profiles { get; set; } =
            new Dictionary<string, U8ProfileOptions>();

        public bool IsDryRun => string.Equals(U8Mode, "dryRun", System.StringComparison.OrdinalIgnoreCase);
    }

    public sealed class U8ProfileOptions
    {
        public string SubId { get; set; } = "AS";

        public string AccountId { get; set; }

        public string Year { get; set; }

        public string UserId { get; set; }

        public string Password { get; set; }

        public string LoginDate { get; set; }

        public string Server { get; set; }

        public string Serial { get; set; } = string.Empty;
    }

    public sealed class U8DatabaseOptions
    {
        public bool Enabled { get; set; }

        public string Server { get; set; }

        public string Database { get; set; }

        public string User { get; set; }

        public string Password { get; set; }
    }
}
