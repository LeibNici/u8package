using Xinchuan.U8Bridge.Configuration;
using Xinchuan.U8Bridge.U8;

namespace Xinchuan.U8Bridge.Services
{
    public static class ServiceRegistry
    {
        public static BridgeOptions Options { get; private set; }

        public static BridgeOperationService OperationService { get; private set; }

        public static void Initialize(BridgeOptions options)
        {
            Options = options;
            IU8ApiClient apiClient = options.IsDryRun
                ? (IU8ApiClient)new DryRunU8ApiClient()
                : new OfficialU8ApiClient(options);
            BridgeLogger.Info(
                "Service registry initialized. U8Mode="
                + options.U8Mode
                + ", Profiles="
                + options.Profiles.Count);
            OperationService = new BridgeOperationService(
                options,
                apiClient,
                new IdempotencyStore(),
                new U8DatabaseQueryService(options.Database));
        }
    }
}
