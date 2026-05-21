using System.Linq;
using System.Web.Http;
using Xinchuan.U8Bridge.Models;
using Xinchuan.U8Bridge.Services;

namespace Xinchuan.U8Bridge.Controllers
{
    [RoutePrefix("api/u8")]
    public sealed class U8Controller : ApiController
    {
        private readonly BridgeOperationService service = ServiceRegistry.OperationService;

        [HttpPost]
        [Route("login-test")]
        public IHttpActionResult LoginTest(LoginTestRequest request)
        {
            return Bridge(service.LoginTest(RequestIdHeader(), request));
        }

        [HttpPost]
        [Route("sales-order/save")]
        public IHttpActionResult SaveSalesOrder(SalesOrderSaveRequest request)
        {
            return Invoke(request, "U8API/SaleOrder/Save", "sales-order", request?.OrderNo, true);
        }

        [HttpPost]
        [Route("sales-order/audit")]
        public IHttpActionResult AuditSalesOrder(AuditRequest request)
        {
            return Invoke(request, "U8API/SaleOrder/Audit", "sales-order-audit", request?.OrderNo, false);
        }

        [HttpPost]
        [Route("consignment/save")]
        public IHttpActionResult SaveConsignment(ConsignmentSaveRequest request)
        {
            return Invoke(request, "U8API/Consignment/Save", "consignment", request?.DeliveryNo, true);
        }

        [HttpPost]
        [Route("consignment/audit")]
        public IHttpActionResult AuditConsignment(AuditRequest request)
        {
            return Invoke(request, "U8API/Consignment/Audit", "consignment-audit", request?.DeliveryNo, false);
        }

        [HttpPost]
        [Route("saleout/add")]
        public IHttpActionResult AddSaleOut(SaleOutAddRequest request)
        {
            return Invoke(request, "U8API/saleout/Add", "saleout", request?.OutboundNo, true);
        }

        [HttpPost]
        [Route("saleout/audit")]
        public IHttpActionResult AuditSaleOut(StockAuditRequest request)
        {
            return Invoke(request, "U8API/saleout/Audit", "saleout-audit", request?.U8Id, false);
        }

        [HttpPost]
        [Route("material-out/add")]
        public IHttpActionResult AddMaterialOut(MaterialOutAddRequest request)
        {
            return Invoke(request, "U8API/MaterialOut/Add", "material-out", request?.MaterialOutNo, true);
        }

        [HttpPost]
        [Route("material-out/audit")]
        public IHttpActionResult AuditMaterialOut(StockAuditRequest request)
        {
            return Invoke(request, "U8API/saleout/Audit", "material-out-audit", request?.U8Id, false);
        }

        private IHttpActionResult Invoke(
            BaseBusinessRequest request,
            string apiAddress,
            string documentType,
            string businessNo,
            bool idempotent)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(businessNo))
            {
                var invalid = BridgeResponse.Fail(RequestIdHeader(), BridgeErrorCodes.RequestInvalid, "请求字段不合法");
                return Bridge(invalid);
            }

            var call = U8ApiCall.Create(apiAddress, documentType, businessNo, request);
            return Bridge(service.Invoke(RequestIdHeader(), request, call, idempotent));
        }

        private IHttpActionResult Bridge(BridgeResponse response)
        {
            return Content(response.ToHttpStatus(), response);
        }

        private string RequestIdHeader()
        {
            return Request.Headers.TryGetValues("X-Request-ID", out var values)
                ? values.FirstOrDefault()
                : null;
        }
    }
}
