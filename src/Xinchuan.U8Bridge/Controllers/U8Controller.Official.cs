using System.Web.Http;
using Xinchuan.U8Bridge.Models;

namespace Xinchuan.U8Bridge.Controllers
{
    public sealed partial class U8Controller
    {
        [HttpPost]
        [Route("official/{*apiPath}")]
        public IHttpActionResult InvokeOfficialApi(string apiPath, OfficialApiInvokeRequest request)
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
