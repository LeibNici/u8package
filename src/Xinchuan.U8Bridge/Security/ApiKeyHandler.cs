using System.Linq;
using System.Net;
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

            BridgeResponse rejection = CheckAccess(request);
            return rejection == null
                ? base.SendAsync(request, cancellationToken)
                : Task.FromResult(request.CreateResponse(rejection.ToHttpStatus(), rejection));
        }

        private BridgeResponse CheckAccess(HttpRequestMessage request)
        {
            if (options.AllowedSourceIps != null && options.AllowedSourceIps.Count > 0)
            {
                string remoteIp = GetRemoteIp(request);
                if (!options.AllowedSourceIps.Contains(remoteIp))
                {
                    return BridgeResponse.Fail(
                        GetRequestId(request),
                        BridgeErrorCodes.SourceForbidden,
                        "来源 IP 不允许访问 U8 Bridge");
                }
            }

            return CheckApiKey(request);
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
            return path == "/health" || path == "/openapi.yaml";
        }

        private static string GetRequestId(HttpRequestMessage request)
        {
            return request.Headers.TryGetValues("X-Request-ID", out var values) ? values.FirstOrDefault() : null;
        }

        private static string GetRemoteIp(HttpRequestMessage request)
        {
            const string owinKey = "MS_OwinContext";
            if (!request.Properties.ContainsKey(owinKey))
            {
                return null;
            }

            dynamic context = request.Properties[owinKey];
            return context.Request.RemoteIpAddress;
        }
    }
}
