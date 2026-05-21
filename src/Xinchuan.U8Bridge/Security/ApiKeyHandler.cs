using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Xinchuan.U8Bridge.Configuration;
using Xinchuan.U8Bridge.Models;

namespace Xinchuan.U8Bridge.Security
{
    public sealed class ApiKeyHandler : DelegatingHandler
    {
        private const string ApiKeyHeader = "X-API-KEY";
        private readonly BridgeOptions options;

        public ApiKeyHandler(BridgeOptions options)
        {
            this.options = options;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (IsAnonymous(request))
            {
                return base.SendAsync(request, cancellationToken);
            }

            BridgeResponse rejection = CheckApiKey(request);
            return rejection == null
                ? base.SendAsync(request, cancellationToken)
                : Task.FromResult(request.CreateResponse(rejection.ToHttpStatus(), rejection));
        }

        private BridgeResponse CheckApiKey(HttpRequestMessage request)
        {
            if (!request.Headers.TryGetValues(ApiKeyHeader, out var values))
            {
                return BridgeResponse.Fail(GetRequestId(request), BridgeErrorCodes.AuthMissing, "缺少 X-API-KEY");
            }

            if (!string.Equals(values.FirstOrDefault(), options.ApiKey, System.StringComparison.Ordinal))
            {
                return BridgeResponse.Fail(GetRequestId(request), BridgeErrorCodes.AuthInvalid, "X-API-KEY 不正确");
            }

            return null;
        }

        private static bool IsAnonymous(HttpRequestMessage request)
        {
            string path = request.RequestUri.AbsolutePath.TrimEnd('/').ToLowerInvariant();
            return path == "/health" || path == "/openapi.yaml" || path == "/swagger";
        }

        private static string GetRequestId(HttpRequestMessage request)
        {
            return request.Headers.TryGetValues("X-Request-ID", out var values) ? values.FirstOrDefault() : null;
        }

    }
}
