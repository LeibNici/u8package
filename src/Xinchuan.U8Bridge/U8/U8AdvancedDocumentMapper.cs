using System;
using System.Collections.Generic;
using System.Globalization;
using Xinchuan.U8Bridge.Models;
using static Xinchuan.U8Bridge.U8.U8BrokerMapperBuilder;

namespace Xinchuan.U8Bridge.U8
{
    public static class U8AdvancedDocumentMapper
    {
        public static U8BrokerCall MapMaterialApp(MaterialAppAddRequest request)
        {
            var call = CreateStockAddCall("64");
            var head = OneRow("DomHead");
            head.Rows[0]["id"] = string.Empty;
            Put(head, "ccode", request.ApplicationNo, "ddate", DateValue(request.ApplicationDate));
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
            var call = new U8BrokerCall { BooleanReturn = true, VoucherType = 1 };
            call.ContextValues["bPositive"] = true;
            call.ContextValues["sBillType"] = string.Empty;
            call.ContextValues["sBusType"] = Any(request.BusinessType, "普通采购");
            var head = OneRow("DomHead");
            PutAll(head.Rows[0], "poid", request.U8Id, "cpoid", request.PurchaseOrderNo);
            PutAll(head.Rows[0], "ufts", request.TimeStamp, "cverifier", request.Verifier);
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

        private static void MapMaterialAppItems(U8BrokerCall call, IList<MaterialAppItem> items)
        {
            var body = new U8BoObject("domBody");
            foreach (MaterialAppItem item in items)
            {
                var row = new Dictionary<string, object>();
                row["autoid"] = string.Empty;
                Put(row, "cinvcode", item.MaterialCode);
                Put(row, "irowno", Convert.ToString(item.LineNo, CultureInfo.InvariantCulture));
                Put(row, "cinvname", item.MaterialName, "cinvm_unit", item.Unit);
                Put(row, "cbatch", item.BatchNo, "iquantity", Convert.ToDouble(item.Quantity));
                Put(row, "dduedate", DateValue(item.DueDate));
                body.Rows.Add(row);
            }

            call.BusinessObjects.Add(body);
        }

        private static object DateValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            DateTime parsed;
            if (DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out parsed))
            {
                return parsed;
            }

            return DateTime.TryParse(value, out parsed) ? parsed : (object)value;
        }

    }
}
