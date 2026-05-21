using System;
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
        [Route("customers/query")]
        public IHttpActionResult QueryCustomers(MasterQueryRequest request)
        {
            return UnsupportedQuery(request, "U8 客户主数据查询官方 API 示例未提供，系统不再直连 U8 数据库");
        }

        [HttpPost]
        [Route("materials/query")]
        public IHttpActionResult QueryMaterials(MasterQueryRequest request)
        {
            return UnsupportedQuery(request, "U8 物料主数据查询官方 API 示例未提供，系统不再直连 U8 数据库");
        }

        [HttpPost]
        [Route("suppliers/query")]
        public IHttpActionResult QuerySuppliers(MasterQueryRequest request)
        {
            return UnsupportedQuery(request, "U8 供应商主数据查询官方 API 示例未提供，系统不再直连 U8 数据库");
        }

        [HttpPost]
        [Route("inventory/query")]
        public IHttpActionResult QueryInventory(InventoryQueryRequest request)
        {
            return UnsupportedQuery(request, "U8 现存量查询官方 API 示例未提供，系统不再直连 U8 数据库");
        }

        [HttpPost]
        [Route("in-transit/query")]
        public IHttpActionResult QueryInTransit(InTransitQueryRequest request)
        {
            return UnsupportedQuery(request, "U8 采购在途查询官方 API 示例未提供，系统不再直连 U8 数据库");
        }

        [HttpPost]
        [Route("material-price/query")]
        public IHttpActionResult QueryMaterialPrice(MaterialPriceQueryRequest request)
        {
            return UnsupportedQuery(request, "U8 物料价格查询官方 API 示例未提供，系统不再直连 U8 数据库");
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

        [HttpPost]
        [Route("material-app/add")]
        public IHttpActionResult AddMaterialApp(MaterialAppAddRequest request)
        {
            return Invoke(request, "U8API/materialapp/Add", "material-app", request?.ApplicationNo, true);
        }

        [HttpPost]
        [Route("material-app/audit")]
        public IHttpActionResult AuditMaterialApp(StockAuditRequest request)
        {
            return Invoke(request, "U8API/materialapp/Audit", "material-app-audit", request?.U8Id, false);
        }

        [HttpPost]
        [Route("morder/add")]
        public IHttpActionResult AddManufactureOrder(ManufactureOrderAddRequest request)
        {
            return Unsupported(request, request?.OrderNo, "U8 生产订单新增需要 extbo 映射，待按现场模板补齐");
        }

        [HttpPost]
        [Route("morder/audit")]
        public IHttpActionResult AuditManufactureOrder(ManufactureOrderAuditRequest request)
        {
            return Invoke(request, "U8API/MOrder/MOrderAuditing", "morder-audit", request?.OrderNo, false);
        }

        [HttpPost]
        [Route("inbound/add")]
        public IHttpActionResult AddInbound(InboundAddRequest request)
        {
            return Unsupported(request, request?.InboundNo, "U8 入库单官方 API 路径未确认");
        }

        [HttpPost]
        [Route("production-plan/publish")]
        public IHttpActionResult PublishProductionPlan(ProductionPlanPublishRequest request)
        {
            return Unsupported(request, request?.PlanNo, "U8 生产计划发布官方 API 路径未确认");
        }

        [HttpPost]
        [Route("work-report/save")]
        public IHttpActionResult SaveWorkReport(WorkReportSaveRequest request)
        {
            return Unsupported(request, request?.ReportNo, "U8 报工官方 API 路径未确认");
        }

        [HttpPost]
        [Route("material-return/add")]
        public IHttpActionResult AddMaterialReturn(MaterialReturnAddRequest request)
        {
            return Unsupported(request, request?.ReturnNo, "U8 生产退料官方 API 路径未确认");
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

        private IHttpActionResult UnsupportedQuery(BaseBusinessRequest request, string message)
        {
            BridgeResponse invalid = ValidateUnsupportedRequest(request, false, null);
            return invalid == null
                ? Bridge(BridgeResponse.Fail(request.RequestId, BridgeErrorCodes.U8ApiNotSupported, message))
                : Bridge(invalid);
        }

        private IHttpActionResult Unsupported(BaseBusinessRequest request, string businessNo, string message)
        {
            BridgeResponse invalid = ValidateUnsupportedRequest(request, true, businessNo);
            return invalid == null
                ? Bridge(BridgeResponse.Fail(request.RequestId, BridgeErrorCodes.U8ApiNotSupported, message))
                : Bridge(invalid);
        }

        private BridgeResponse ValidateUnsupportedRequest(
            BaseBusinessRequest request,
            bool requireBusinessNo,
            string businessNo)
        {
            if (!ModelState.IsValid || request == null || string.IsNullOrWhiteSpace(request.RequestId))
            {
                return BridgeResponse.Fail(RequestIdHeader(), BridgeErrorCodes.RequestInvalid, "请求字段不合法");
            }

            if (!string.Equals(RequestIdHeader(), request.RequestId, StringComparison.Ordinal))
            {
                return BridgeResponse.Fail(request.RequestId, BridgeErrorCodes.RequestIdMismatch, "X-Request-ID 与 requestId 不一致");
            }

            if (requireBusinessNo && string.IsNullOrWhiteSpace(businessNo))
            {
                return BridgeResponse.Fail(request.RequestId, BridgeErrorCodes.RequestInvalid, "请求字段不合法");
            }

            return null;
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
