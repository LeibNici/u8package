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
            return Bridge(service.QueryCustomers(RequestIdHeader(), request));
        }

        [HttpPost]
        [Route("materials/query")]
        public IHttpActionResult QueryMaterials(MasterQueryRequest request)
        {
            return Bridge(service.QueryMaterials(RequestIdHeader(), request));
        }

        [HttpPost]
        [Route("suppliers/query")]
        public IHttpActionResult QuerySuppliers(MasterQueryRequest request)
        {
            return Bridge(service.QuerySuppliers(RequestIdHeader(), request));
        }

        [HttpPost]
        [Route("inventory/query")]
        public IHttpActionResult QueryInventory(InventoryQueryRequest request)
        {
            return Bridge(service.QueryInventory(RequestIdHeader(), request));
        }

        [HttpPost]
        [Route("in-transit/query")]
        public IHttpActionResult QueryInTransit(InTransitQueryRequest request)
        {
            return Bridge(service.QueryInTransit(RequestIdHeader(), request));
        }

        [HttpPost]
        [Route("material-price/query")]
        public IHttpActionResult QueryMaterialPrice(MaterialPriceQueryRequest request)
        {
            return Bridge(service.QueryMaterialPrice(RequestIdHeader(), request));
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
            return Invoke(request, "U8API/MaterialOut/Audit", "material-out-audit", request?.U8Id, false);
        }

        [HttpPost]
        [Route("material-out/cancel-audit")]
        public IHttpActionResult CancelAuditMaterialOut(StockAuditRequest request)
        {
            return Invoke(request, "U8API/MaterialOut/CancelAudit", "material-out-cancel-audit", request?.U8Id, false);
        }

        [HttpPost]
        [Route("material-out/delete")]
        public IHttpActionResult DeleteMaterialOut(StockAuditRequest request)
        {
            return Invoke(request, "U8API/MaterialOut/Delete", "material-out-delete", request?.U8Id, false);
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
            return Invoke(request, "U8API/MOrder/MOrderAdd", "morder", request?.OrderNo, true);
        }

        [HttpPost]
        [Route("morder/update")]
        public IHttpActionResult UpdateManufactureOrder(ManufactureOrderAddRequest request)
        {
            return Invoke(request, "U8API/MOrder/MOrderUpdate", "morder-update", request?.OrderNo, true);
        }

        [HttpPost]
        [Route("morder/audit")]
        public IHttpActionResult AuditManufactureOrder(ManufactureOrderAuditRequest request)
        {
            return Invoke(request, "U8API/MOrder/MOrderAuditing", "morder-audit", request?.OrderNo, false);
        }

        [HttpPost]
        [Route("morder/unaudit")]
        public IHttpActionResult UnauditManufactureOrder(ManufactureOrderSimpleRequest request)
        {
            return Invoke(request, "U8API/MOrder/MOrderUnauditing", "morder-unaudit", request?.OrderNo, false);
        }

        [HttpPost]
        [Route("morder/delete")]
        public IHttpActionResult DeleteManufactureOrder(ManufactureOrderSimpleRequest request)
        {
            return Invoke(request, "U8API/MOrder/MOrderDelete", "morder-delete", request?.OrderNo, false);
        }

        [HttpPost]
        [Route("morder/load")]
        public IHttpActionResult LoadManufactureOrder(ManufactureOrderSimpleRequest request)
        {
            return Invoke(request, "U8API/MOrder/MOrderLoad", "morder-load", request?.OrderNo, false);
        }

        [HttpPost]
        [Route("purchase-order/confirm")]
        public IHttpActionResult ConfirmPurchaseOrder(PurchaseOrderConfirmRequest request)
        {
            return Invoke(request, "U8API/PurchaseOrder/ConfirmPO", "purchase-order-confirm", request?.PurchaseOrderNo, false);
        }

        [HttpPost]
        [Route("purchase-order/cancel-confirm")]
        public IHttpActionResult CancelConfirmPurchaseOrder(PurchaseOrderConfirmRequest request)
        {
            return Invoke(request, "U8API/PurchaseOrder/CancelconfirmPo", "purchase-order-cancel-confirm", request?.PurchaseOrderNo, false);
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
