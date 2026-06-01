using System;
using System.Collections.Generic;
using Xinchuan.U8Bridge.Models;
using static Xinchuan.U8Bridge.U8.U8BrokerMapperBuilder;

namespace Xinchuan.U8Bridge.U8
{
    public static class U8BrokerDocumentMapper
    {
        public static U8BrokerCall Map(U8ApiCall call)
        {
            var officialRequest = call.Payload as OfficialApiInvokeRequest;
            if (officialRequest != null)
            {
                return OfficialU8BrokerMapper.Map(officialRequest);
            }

            switch (call.ApiAddress)
            {
                case "U8API/SaleOrder/Save":
                    return MapSalesOrder((SalesOrderSaveRequest)call.Payload);
                case "U8API/SaleOrder/Audit":
                    return MapHeadAudit((AuditRequest)call.Payload, 12);
                case "U8API/Consignment/Save":
                    return MapConsignment((ConsignmentSaveRequest)call.Payload);
                case "U8API/Consignment/Audit":
                    return MapHeadAudit((AuditRequest)call.Payload, 9);
                case "U8API/saleout/Add":
                    return MapSaleOut((SaleOutAddRequest)call.Payload);
                case "U8API/saleout/Audit":
                    return MapStockAudit(
                        (StockAuditRequest)call.Payload,
                        call.DocumentType == "material-out-audit" ? "11" : "32");
                case "U8API/MaterialOut/Add":
                    return MapMaterialOut((MaterialOutAddRequest)call.Payload);
                case "U8API/materialapp/Add":
                    return U8AdvancedDocumentMapper.MapMaterialApp((MaterialAppAddRequest)call.Payload);
                case "U8API/materialapp/Audit":
                    return U8AdvancedDocumentMapper.MapMaterialAppAudit((StockAuditRequest)call.Payload);
                case "U8API/MOrder/MOrderAdd":
                case "U8API/MOrder/MOrderUpdate":
                    return U8AdvancedDocumentMapper.MapManufactureOrderAdd((ManufactureOrderAddRequest)call.Payload);
                case "U8API/MOrder/MOrderAuditing":
                    return U8AdvancedDocumentMapper.MapManufactureOrderAudit((ManufactureOrderAuditRequest)call.Payload);
                case "U8API/MOrder/MOrderUnauditing":
                case "U8API/MOrder/MOrderDelete":
                case "U8API/MOrder/MOrderLoad":
                    return U8AdvancedDocumentMapper.MapManufactureOrderSimple((ManufactureOrderSimpleRequest)call.Payload);
                case "U8API/PurchaseOrder/ConfirmPO":
                case "U8API/PurchaseOrder/CancelconfirmPo":
                    return U8AdvancedDocumentMapper.MapPurchaseOrderConfirm((PurchaseOrderConfirmRequest)call.Payload);
                case "U8API/MaterialOut/Audit":
                    return U8AdvancedDocumentMapper.MapMaterialOutAction((StockAuditRequest)call.Payload, true);
                case "U8API/MaterialOut/CancelAudit":
                case "U8API/MaterialOut/Delete":
                    return U8AdvancedDocumentMapper.MapMaterialOutAction((StockAuditRequest)call.Payload, false);
                case "U8API/BOM/BomAdd":
                    return U8BomDocumentMapper.MapAdd((BomAddRequest)call.Payload);
                case "U8API/BOM/BomLoad":
                    return U8BomDocumentMapper.MapAction((BomActionRequest)call.Payload, true);
                case "U8API/BOM/BomAuditing":
                case "U8API/BOM/BomUnauditing":
                case "U8API/BOM/BomDelete":
                    return U8BomDocumentMapper.MapAction((BomActionRequest)call.Payload, false);
                default:
                    throw new InvalidOperationException("Unsupported U8 API address: " + call.ApiAddress);
            }
        }

        private static U8BrokerCall MapSalesOrder(SalesOrderSaveRequest request)
        {
            var call = CreateSaveCall(12, "vNewID");
            var head = OneRow("domHead");
            IDictionary<string, object> headRow = head.Rows[0];
            PutAll(headRow, "id", string.Empty, "csocode", request.OrderNo, "ddate", request.OrderDate);
            PutAll(headRow, "cbustype", "普通销售", "cstname", Any(request.SalesTypeName, request.SalesTypeCode));
            PutAll(headRow, "ccusabbname", Any(request.CustomerName, request.CustomerCode));
            PutAll(headRow, "cdepname", Any(request.DepartmentName, request.DepartmentCode));
            PutAll(headRow, "itaxrate", request.TaxRate, "cexch_name", Any(request.Currency, "人民币"));
            PutAll(headRow, "iexchrate", 1, "ivtid", 131507);
            PutAll(headRow, "cmaker", request.Maker, "breturnflag", "0", "ufts", string.Empty);
            PutAll(headRow, "cstcode", request.SalesTypeCode, "cdepcode", request.DepartmentCode);
            PutAll(headRow, "ccuscode", request.CustomerCode, "ccushand", string.Empty);
            PutAll(headRow, "cpsnophone", string.Empty, "cpsnmobilephone", string.Empty);
            PutAll(headRow, "cattachment", string.Empty, "ccusname", Any(request.CustomerName, request.CustomerCode));
            PutAll(headRow, "csscode", string.Empty, "cssname", string.Empty);
            PutAll(headRow, "cinvoicecompany", string.Empty, "cinvoicecompanyabbname", string.Empty);
            PutAll(headRow, "ccuspersoncode", string.Empty, "dclosedate", string.Empty);
            PutAll(headRow, "dclosesystime", string.Empty, "bmustbook", string.Empty);
            PutAll(headRow, "fbookratio", string.Empty, "cgathingcode", string.Empty);
            PutAll(headRow, "fbooksum", string.Empty, "fbooknatsum", string.Empty);
            PutAll(headRow, "fgbooknatsum", string.Empty, "fgbooksum", string.Empty);
            PutAll(headRow, "ccrmpersonname", string.Empty, "csysbarcode", string.Empty);
            PutAll(headRow, "ioppid", string.Empty, "contract_status", string.Empty);
            PutAll(headRow, "csvouchtype", string.Empty, "bcashsale", string.Empty);
            PutAll(headRow, "iflowid", string.Empty, "cflowname", string.Empty);
            PutAll(headRow, "cchangeverifier", string.Empty, "dchangeverifydate", string.Empty);
            PutAll(headRow, "dchangeverifytime", string.Empty);
            Put(head, "cmemo", request.Memo);
            MapSalesItems(call, request.Items, "domBody");
            call.BusinessObjects.Add(head);
            return call;
        }

        private static U8BrokerCall MapConsignment(ConsignmentSaveRequest request)
        {
            var call = CreateSaveCall(9, "vNewID");
            var head = OneRow("domHead");
            PutAll(head.Rows[0], "dlid", string.Empty, "cdlcode", request.DeliveryNo);
            PutAll(head.Rows[0], "ddate", DateValue(request.DeliveryDate), "cbustype", "普通销售");
            PutAll(head.Rows[0], "cstcode", Any(request.SalesTypeCode, "01"));
            PutAll(head.Rows[0], "cstname", Any(request.SalesTypeName, request.SalesTypeCode, "普通销售"));
            PutAll(head.Rows[0], "ccuscode", request.CustomerCode, "ccusname", request.CustomerName);
            PutAll(head.Rows[0], "ccusabbname", Any(request.CustomerName, request.CustomerCode));
            PutAll(head.Rows[0], "cdepcode", request.DepartmentCode);
            PutAll(head.Rows[0], "cdepname", Any(request.DepartmentName, request.DepartmentCode));
            PutAll(head.Rows[0], "cexch_name", Any(request.Currency, "人民币"));
            PutAll(head.Rows[0], "iexchrate", request.ExchangeRate <= 0 ? 1 : request.ExchangeRate);
            PutAll(head.Rows[0], "itaxrate", request.TaxRate);
            PutAll(head.Rows[0], "cmaker", request.Maker, "csocode", request.OrderNo);
            PutAll(head.Rows[0], "breturnflag", "0", "cmemo", request.Memo);
            MapOutboundItems(call, request.Items, "domBody");
            call.BusinessObjects.Add(head);
            return call;
        }

        private static U8BrokerCall MapSaleOut(SaleOutAddRequest request)
        {
            var call = CreateStockAddCall("32");
            var head = OneRow("DomHead");
            PutAll(head.Rows[0], "id", string.Empty, "ccode", request.OutboundNo);
            PutAll(head.Rows[0], "ddate", DateValue(request.OutboundDate));
            PutAll(head.Rows[0], "cwhcode", request.WarehouseCode, "cwhname", request.WarehouseName);
            PutAll(head.Rows[0], "cbustype", "普通销售", "ccuscode", request.CustomerCode);
            PutAll(head.Rows[0], "ccusname", request.CustomerName);
            PutAll(head.Rows[0], "ccusabbname", Any(request.CustomerName, request.CustomerCode));
            PutAll(head.Rows[0], "cdepcode", request.DepartmentCode, "crdcode", request.RdCode);
            PutAll(head.Rows[0], "cmaker", request.Maker, "cvouchtype", "32");
            PutAll(head.Rows[0], "brdflag", 0, "csource", 1, "cmemo", request.Memo);
            MapOutboundItems(call, request.Items, "domBody");
            call.BusinessObjects.Add(head);
            return call;
        }

        private static U8BrokerCall MapMaterialOut(MaterialOutAddRequest request)
        {
            var call = CreateStockAddCall("11");
            var head = OneRow("DomHead");
            PutAll(head.Rows[0], "id", string.Empty, "ccode", request.MaterialOutNo);
            PutAll(head.Rows[0], "ddate", DateValue(request.OutDate));
            PutAll(head.Rows[0], "cwhcode", request.WarehouseCode, "cwhname", request.WarehouseName);
            PutAll(head.Rows[0], "crdcode", request.RdCode, "crdname", request.RdName);
            PutAll(head.Rows[0], "cdepcode", request.DepartmentCode, "cdepname", request.DepartmentName);
            PutAll(head.Rows[0], "cmaker", request.Maker, "cvouchtype", "11");
            PutAll(head.Rows[0], "brdflag", "0", "cmemo", request.Memo);
            MapMaterialItems(call, request.Items);
            call.BusinessObjects.Add(head);
            return call;
        }

        private static U8BrokerCall MapHeadAudit(AuditRequest request, int voucherType)
        {
            var call = new U8BrokerCall { VoucherType = voucherType };
            var head = OneRow("domHead");
            Put(head, "csocode", request.OrderNo, "cdlcode", request.DeliveryNo, "cverifier", request.Verifier);
            Put(head, "id", request.U8Id, "bverify", request.Verify ? 1 : 0);
            call.BusinessObjects.Add(head);
            call.NormalValues["bVerify"] = request.Verify;
            return call;
        }

        private static U8BrokerCall MapStockAudit(StockAuditRequest request, string defaultType)
        {
            var call = new U8BrokerCall { BooleanReturn = true };
            call.NormalValues["sVouchType"] = Any(request.VouchType, defaultType);
            call.NormalValues["VouchId"] = request.U8Id;
            call.NormalValues["TimeStamp"] = string.Empty;
            call.NormalValues["domMsg"] = U8BrokerSpecialValue.DomDocument();
            call.NormalValues["bCheck"] = request.CheckStock;
            call.NormalValues["bBeforCheckStock"] = request.BeforeCheckStock;
            call.NormalValues["bList"] = false;
            call.NormalValues["MakeWheres"] = U8BrokerSpecialValue.Com("VBA.Collection");
            call.NormalValues["sWebXml"] = string.Empty;
            call.NormalValues["oGenVouchIds"] = U8BrokerSpecialValue.Com("Scripting.Dictionary");
            call.ReturnIdName = "VouchId";
            return call;
        }

        private static void MapSalesItems(U8BrokerCall call, IList<SalesOrderItem> items, string name)
        {
            var body = new U8BoObject(name);
            foreach (SalesOrderItem item in items)
            {
                var row = NewSalesOrderRow(item);
                body.Rows.Add(row);
            }

            call.BusinessObjects.Add(body);
        }

        private static IDictionary<string, object> NewSalesOrderRow(SalesOrderItem item)
        {
            var row = new Dictionary<string, object>();
            PutAll(row, "isosid", string.Empty, "autoid", string.Empty, "id", string.Empty);
            PutAll(row, "irowno", item.LineNo);
            PutAll(row, "cinvcode", item.MaterialCode, "cinvname", item.MaterialName);
            PutAll(row, "iquantity", item.Quantity, "dpredate", item.DeliveryDate);
            PutAll(row, "dpremodate", item.DeliveryDate, "borderbom", 0, "borderbomover", 0);
            PutAll(row, "iinvexchrate", 1, "cunitid", Any(item.UnitCode, "28"));
            PutAll(row, "cinva_unit", item.Unit ?? string.Empty, "cinvm_unit", item.Unit ?? string.Empty);
            PutAll(row, "igrouptype", 0, "cgroupcode", "1", "dreleasedate", string.Empty);
            PutAll(row, "editprop", "A", "itaxunitprice", item.TaxUnitPrice, "itax", item.TaxAmount);
            Put(row, "isum", item.TaxAmount, "cinvstd", item.Specification);
            PutAll(row, "fstockquano", string.Empty, "fcanusequano", string.Empty);
            PutAll(row, "iimid", string.Empty, "btracksalebill", string.Empty);
            PutAll(row, "ccorvouchtype", string.Empty, "ccorvouchtypename", string.Empty);
            PutAll(row, "icorrowno", string.Empty, "fcanusequan", string.Empty);
            PutAll(row, "fstockquan", string.Empty, "bsaleprice", true);
            PutAll(row, "bgift", false, "forecastdid", string.Empty);
            PutAll(row, "cdetailsdemandcode", string.Empty, "cdetailsdemandmemo", string.Empty);
            PutAll(row, "cbsysbarcode", string.Empty, "busecusbom", string.Empty);
            PutAll(row, "bptomodel", string.Empty, "cparentcode", string.Empty);
            PutAll(row, "cchildcode", string.Empty, "icalctype", string.Empty);
            PutAll(row, "fchildqty", string.Empty, "fchildrate", string.Empty);
            PutAll(row, "iunitprice", 0, "imoney", 0);
            PutAll(row, "inatunitprice", 0, "inatmoney", 0);
            PutAll(row, "inattax", 0, "inatsum", 0, "kl", 100, "kl2", 100);
            return row;
        }

        private static void MapOutboundItems(U8BrokerCall call, IList<OutboundItem> items, string name)
        {
            var body = new U8BoObject(name);
            foreach (OutboundItem item in items)
            {
                var row = NewBodyRow(item.LineNo, item.MaterialCode, item.MaterialName, item.Quantity, item.Unit);
                Put(row, "cbatch", item.BatchNo, "csocode", item.SourceOrderNo, "isosid", item.SourceLineNo);
                body.Rows.Add(row);
            }

            call.BusinessObjects.Add(body);
        }

        private static void MapMaterialItems(U8BrokerCall call, IList<MaterialOutItem> items)
        {
            var body = new U8BoObject("domBody");
            foreach (MaterialOutItem item in items)
            {
                var row = NewBodyRow(item.LineNo, item.MaterialCode, item.MaterialName, item.Quantity, item.Unit);
                Put(row, "cbatch", item.BatchNo, "cmocode", item.WorkOrderNo, "imoseq", item.SourceDetailId);
                body.Rows.Add(row);
            }

            call.BusinessObjects.Add(body);
        }

    }
}
