using System;
using System.Collections.Generic;
using Xinchuan.U8Bridge.Models;

namespace Xinchuan.U8Bridge.U8
{
    public static class U8BrokerDocumentMapper
    {
        public static U8BrokerCall Map(U8ApiCall call)
        {
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
                case "U8API/MOrder/MOrderAuditing":
                    return U8AdvancedDocumentMapper.MapManufactureOrderAudit((ManufactureOrderAuditRequest)call.Payload);
                default:
                    throw new InvalidOperationException("Unsupported U8 API address: " + call.ApiAddress);
            }
        }

        private static U8BrokerCall MapSalesOrder(SalesOrderSaveRequest request)
        {
            var call = CreateSaveCall(12, "vNewID");
            var head = OneRow("domHead");
            Put(head, "id", 0, "csocode", request.OrderNo, "ddate", request.OrderDate);
            Put(head, "cbustype", "普通销售", "cstname", Any(request.SalesTypeName, request.SalesTypeCode));
            Put(head, "cstcode", request.SalesTypeCode, "ccuscode", request.CustomerCode);
            Put(head, "ccusname", request.CustomerName, "ccusabbname", Any(request.CustomerName, request.CustomerCode));
            Put(head, "cdepcode", request.DepartmentCode, "cdepname", Any(request.DepartmentName, request.DepartmentCode));
            Put(head, "itaxrate", request.TaxRate, "cexch_name", Any(request.Currency, "人民币"));
            Put(head, "cmaker", request.Maker, "breturnflag", 0, "cmemo", request.Memo);
            MapSalesItems(call, request.Items, "domBody");
            call.BusinessObjects.Add(head);
            return call;
        }

        private static U8BrokerCall MapConsignment(ConsignmentSaveRequest request)
        {
            var call = CreateSaveCall(9, "vNewID");
            var head = OneRow("domHead");
            Put(head, "dlid", 0, "cdlcode", request.DeliveryNo, "ddate", request.DeliveryDate);
            Put(head, "cbustype", "普通销售", "cstcode", Any(request.SalesTypeCode, "01"));
            Put(head, "cstname", request.SalesTypeCode, "ccuscode", request.CustomerCode);
            Put(head, "ccusname", request.CustomerName, "ccusabbname", Any(request.CustomerName, request.CustomerCode));
            Put(head, "cdepcode", request.DepartmentCode, "cdepname", request.DepartmentCode);
            Put(head, "cmaker", request.Maker, "csocode", request.OrderNo, "breturnflag", 0, "cmemo", request.Memo);
            MapOutboundItems(call, request.Items, "domBody");
            call.BusinessObjects.Add(head);
            return call;
        }

        private static U8BrokerCall MapSaleOut(SaleOutAddRequest request)
        {
            var call = CreateStockAddCall("32");
            var head = OneRow("DomHead");
            Put(head, "id", 0, "ccode", request.OutboundNo, "ddate", request.OutboundDate);
            Put(head, "cwhcode", request.WarehouseCode, "cwhname", request.WarehouseName);
            Put(head, "cbustype", "普通销售", "ccuscode", request.CustomerCode);
            Put(head, "ccusname", request.CustomerName, "ccusabbname", Any(request.CustomerName, request.CustomerCode));
            Put(head, "cdepcode", request.DepartmentCode, "crdcode", request.RdCode, "cmaker", request.Maker);
            Put(head, "cvouchtype", "32", "brdflag", 0, "csource", 1, "cmemo", request.Memo);
            MapOutboundItems(call, request.Items, "domBody");
            call.BusinessObjects.Add(head);
            return call;
        }

        private static U8BrokerCall MapMaterialOut(MaterialOutAddRequest request)
        {
            var call = CreateStockAddCall("11");
            var head = OneRow("DomHead");
            Put(head, "id", 0, "ccode", request.MaterialOutNo, "ddate", request.OutDate);
            Put(head, "cwhcode", request.WarehouseCode, "cwhname", request.WarehouseName);
            Put(head, "crdcode", request.RdCode, "crdname", request.RdName);
            Put(head, "cdepcode", request.DepartmentCode, "cdepname", request.DepartmentName);
            Put(head, "cmaker", request.Maker, "cvouchtype", "11", "brdflag", 0, "cmemo", request.Memo);
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

        private static U8BrokerCall CreateSaveCall(int voucherType, string returnIdName)
        {
            var call = new U8BrokerCall { VoucherType = voucherType, ReturnIdName = returnIdName };
            call.NormalValues["VoucherState"] = 0;
            call.NormalValues[returnIdName] = string.Empty;
            call.NormalValues["DomConfig"] = U8BrokerSpecialValue.DomDocument();
            return call;
        }

        private static U8BrokerCall CreateStockAddCall(string type)
        {
            var call = new U8BrokerCall { BooleanReturn = true, ReturnIdName = "VouchId" };
            call.NormalValues["sVouchType"] = type;
            call.NormalValues["domPosition"] = U8BrokerSpecialValue.EmptyObject();
            call.NormalValues["cnnFrom"] = U8BrokerSpecialValue.Com("ADODB.Connection");
            call.NormalValues["VouchId"] = string.Empty;
            call.NormalValues["domMsg"] = U8BrokerSpecialValue.DomDocument();
            call.NormalValues["bCheck"] = true;
            call.NormalValues["bBeforCheckStock"] = true;
            call.NormalValues["bIsRedVouch"] = false;
            call.NormalValues["sAddedState"] = string.Empty;
            call.NormalValues["bReMote"] = false;
            return call;
        }

        private static U8BoObject OneRow(string name)
        {
            var bo = new U8BoObject(name);
            bo.Rows.Add(new Dictionary<string, object>());
            return bo;
        }

        private static void MapSalesItems(U8BrokerCall call, IList<SalesOrderItem> items, string name)
        {
            var body = new U8BoObject(name);
            foreach (SalesOrderItem item in items)
            {
                var row = NewBodyRow(item.LineNo, item.MaterialCode, item.MaterialName, item.Quantity, item.Unit);
                Put(row, "dpredate", item.DeliveryDate, "dpremodate", item.DeliveryDate);
                Put(row, "cunitid", item.UnitCode, "itaxunitprice", item.TaxUnitPrice, "itax", item.TaxAmount);
                body.Rows.Add(row);
            }

            call.BusinessObjects.Add(body);
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

        private static IDictionary<string, object> NewBodyRow(
            int lineNo,
            string code,
            string name,
            decimal quantity,
            string unit)
        {
            var row = new Dictionary<string, object>();
            Put(row, "autoid", 0, "id", 0, "irowno", lineNo, "cinvcode", code);
            Put(row, "cinvname", name, "iquantity", quantity, "cinvm_unit", unit, "editprop", "A");
            return row;
        }

        private static void Put(U8BoObject bo, params object[] items)
        {
            Put(bo.Rows[0], items);
        }

        private static void Put(IDictionary<string, object> row, params object[] items)
        {
            for (int i = 0; i + 1 < items.Length; i += 2)
            {
                string key = Convert.ToString(items[i]);
                object value = items[i + 1];
                if (value != null && Convert.ToString(value).Length > 0)
                {
                    row[key] = value;
                }
            }
        }

        private static string Any(params string[] values)
        {
            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return string.Empty;
        }
    }
}
