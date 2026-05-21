using System;
using System.Collections.Generic;
using Xinchuan.U8Bridge.Models;

namespace Xinchuan.U8Bridge.U8
{
    public static class U8AdvancedDocumentMapper
    {
        public static U8BrokerCall MapMaterialApp(MaterialAppAddRequest request)
        {
            var call = CreateStockAddCall("64");
            var head = OneRow("DomHead");
            Put(head, "id", 0, "ccode", request.ApplicationNo, "ddate", request.ApplicationDate);
            Put(head, "crdcode", request.RdCode, "cdepcode", request.DepartmentCode);
            Put(head, "cmaker", request.Maker, "cmemo", request.Memo);
            MapMaterialAppItems(call, request.Items);
            call.BusinessObjects.Add(head);
            return call;
        }

        public static U8BrokerCall MapMaterialAppAudit(StockAuditRequest request)
        {
            var call = new U8BrokerCall { BooleanReturn = true, ReturnIdName = "VouchId" };
            call.NormalValues["sVouchType"] = Any(request.VouchType, "64");
            call.NormalValues["VouchId"] = request.U8Id;
            call.NormalValues["cnnFrom"] = U8BrokerSpecialValue.Com("ADODB.Connection");
            call.NormalValues["TimeStamp"] = string.Empty;
            call.NormalValues["domMsg"] = U8BrokerSpecialValue.DomDocument();
            call.NormalValues["bCheck"] = request.CheckStock;
            call.NormalValues["bBeforCheckStock"] = request.BeforeCheckStock;
            call.NormalValues["bList"] = false;
            call.NormalValues["MakeWheres"] = U8BrokerSpecialValue.Com("VBA.Collection");
            call.NormalValues["sWebXml"] = string.Empty;
            call.NormalValues["oGenVouchIds"] = U8BrokerSpecialValue.Com("Scripting.Dictionary");
            return call;
        }

        public static U8BrokerCall MapManufactureOrderAudit(ManufactureOrderAuditRequest request)
        {
            var call = new U8BrokerCall { BooleanReturn = true };
            call.NormalValues["mocode"] = request.OrderNo;
            return call;
        }

        public static U8BrokerCall MapManufactureOrderAdd(ManufactureOrderAddRequest request)
        {
            var call = new U8BrokerCall { BooleanReturn = true };
            var extbo = new U8ExtBoObject("extbo");
            var head = new U8ExtBoRow();
            Put(head.Fields, "MoId", 0, "MoCode", request.OrderNo);
            Put(head.Fields, "CreateUser", request.Maker, "CreateDate", request.CreateDate);
            Put(head.Fields, "CreateTime", request.CreateTime);
            head.Children["Mom_OrderDetail"] = MapManufactureOrderDetails(request.Items);
            extbo.Rows.Add(head);
            call.ExtensionObjects.Add(extbo);
            return call;
        }

        public static U8BrokerCall MapManufactureOrderSimple(ManufactureOrderSimpleRequest request)
        {
            var call = new U8BrokerCall { BooleanReturn = true };
            call.NormalValues["mocode"] = request.OrderNo;
            return call;
        }

        public static U8BrokerCall MapPurchaseOrderConfirm(PurchaseOrderConfirmRequest request)
        {
            var call = new U8BrokerCall { BooleanReturn = true };
            var head = OneRow("DomHead");
            Put(head, "poid", request.U8Id, "cpoid", request.PurchaseOrderNo);
            Put(head, "ufts", request.TimeStamp, "cverifier", request.Verifier);
            call.BusinessObjects.Add(head);
            return call;
        }

        public static U8BrokerCall MapMaterialOutAction(StockAuditRequest request, bool audit)
        {
            var call = new U8BrokerCall { BooleanReturn = true, ReturnIdName = "VouchId" };
            call.NormalValues["sVouchType"] = Any(request.VouchType, "11");
            call.NormalValues["VouchId"] = request.U8Id;
            call.NormalValues["cnnFrom"] = U8BrokerSpecialValue.Com("ADODB.Connection");
            call.NormalValues["TimeStamp"] = U8BrokerSpecialValue.EmptyObject();
            call.NormalValues["domMsg"] = U8BrokerSpecialValue.DomDocument();
            call.NormalValues["bCheck"] = request.CheckStock;
            call.NormalValues["bBeforCheckStock"] = request.BeforeCheckStock;
            call.NormalValues["bList"] = false;
            if (audit)
            {
                call.NormalValues["MakeWheres"] = U8BrokerSpecialValue.Com("VBA.Collection");
                call.NormalValues["sWebXml"] = string.Empty;
                call.NormalValues["oGenVouchIds"] = U8BrokerSpecialValue.Com("Scripting.Dictionary");
            }

            return call;
        }

        private static U8ExtBoObject MapManufactureOrderDetails(IList<ManufactureOrderItem> items)
        {
            var details = new U8ExtBoObject("Mom_OrderDetail");
            foreach (ManufactureOrderItem item in items)
            {
                var row = new U8ExtBoRow();
                Put(row.Fields, "DMoClass", item.OrderClass, "DInvCode", item.MaterialCode);
                Put(row.Fields, "DStartDate", item.StartDate, "DDueDate", item.DueDate);
                Put(row.Fields, "DQty", item.Quantity, "DSortSeq", item.LineNo);
                Put(row.Fields, "DInvName", item.MaterialName, "DWhCode", item.WarehouseCode);
                Put(row.Fields, "DMDeptCode", item.DepartmentCode, "DMoTypeCode", item.OrderTypeCode);
                Put(row.Fields, "DMoLotCode", item.LotNo, "DRemark", item.Remark);
                U8ExtBoObject components = MapManufactureOrderComponents(item.Components);
                if (components.Rows.Count > 0)
                {
                    row.Children["Mom_MoAllocate"] = components;
                }
                details.Rows.Add(row);
            }

            return details;
        }

        private static U8ExtBoObject MapManufactureOrderComponents(IList<ManufactureOrderComponentItem> items)
        {
            var components = new U8ExtBoObject("Mom_MoAllocate");
            if (items == null)
            {
                return components;
            }

            foreach (ManufactureOrderComponentItem item in items)
            {
                var row = new U8ExtBoRow();
                Put(row.Fields, "DSortSeq", item.LineNo, "DOpSeq", item.OperationSeq);
                Put(row.Fields, "DInvCode", item.MaterialCode, "DInvName", item.MaterialName);
                Put(row.Fields, "DBaseQtyN", item.BaseQtyNumerator);
                Put(row.Fields, "DBaseQtyD", item.BaseQtyDenominator);
                Put(row.Fields, "DStartDemDate", item.RequiredDate, "DQty", item.Quantity);
                Put(row.Fields, "DWhCode", item.WarehouseCode, "DLotNo", item.BatchNo);
                Put(row.Fields, "DRemark", item.Remark);
                components.Rows.Add(row);
            }

            return components;
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

        private static void MapMaterialAppItems(U8BrokerCall call, IList<MaterialAppItem> items)
        {
            var body = new U8BoObject("domBody");
            foreach (MaterialAppItem item in items)
            {
                var row = new Dictionary<string, object>();
                Put(row, "autoid", 0, "irowno", item.LineNo, "cinvcode", item.MaterialCode);
                Put(row, "cinvname", item.MaterialName, "cinvm_unit", item.Unit);
                Put(row, "cbatch", item.BatchNo, "iquantity", item.Quantity, "dduedate", item.DueDate);
                body.Rows.Add(row);
            }

            call.BusinessObjects.Add(body);
        }

        private static U8BoObject OneRow(string name)
        {
            var bo = new U8BoObject(name);
            bo.Rows.Add(new Dictionary<string, object>());
            return bo;
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
