using System.Web.Http;
using Xinchuan.U8Bridge.Models;
using Xinchuan.U8Bridge.U8;

namespace Xinchuan.U8Bridge.Controllers
{
    public sealed partial class U8Controller
    {
        [HttpPost]
        [Route("official/{*apiPath}")]
        [System.Obsolete(
            "Use strong typed /api/u8/<business-resource>/<action> endpoints for integrations. "
            + "This generic official route is an internal compatibility endpoint.")]
        public IHttpActionResult InvokeOfficialApi(string apiPath, OfficialApiInvokeRequest request)
        {
            return InvokeOfficialApiAddress(apiPath, request);
        }

        [HttpPost]
        [Route("{*bridgePath}")]
        public IHttpActionResult InvokeOfficialApiAlias(string bridgePath, OfficialApiInvokeRequest request)
        {
            if (!OfficialU8ApiAliasMap.TryGetOfficialApi(bridgePath, out var apiPath))
            {
                string requestId = request == null ? RequestIdHeader() : request.RequestId;
                return Bridge(BridgeResponse.Fail(requestId, BridgeErrorCodes.RequestInvalid, "U8 Bridge 迁移路径不合法"));
            }

            return InvokeOfficialApiAddress(apiPath, request);
        }

        private IHttpActionResult InvokeOfficialApiAddress(string apiPath, OfficialApiInvokeRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.BusinessNo))
            {
                return Bridge(BridgeResponse.Fail(RequestIdHeader(), BridgeErrorCodes.RequestInvalid, "请求字段不合法"));
            }

            if (!IsOfficialApiPath(apiPath))
            {
                return Bridge(BridgeResponse.Fail(request.RequestId, BridgeErrorCodes.RequestInvalid, "U8 API 地址不合法"));
            }

            string documentType = string.IsNullOrWhiteSpace(request.DocumentType)
                ? "official-" + apiPath.Replace("/", "-").ToLowerInvariant()
                : request.DocumentType;
            var call = U8ApiCall.Create(apiPath, documentType, request.BusinessNo, request);
            return Bridge(service.Invoke(RequestIdHeader(), request, call, request.Idempotent));
        }

        private static bool IsOfficialApiPath(string apiPath)
        {
            return !string.IsNullOrWhiteSpace(apiPath)
                && (apiPath.StartsWith("U8API/") || apiPath.StartsWith("U8ERP_"));
        }
    }
}
