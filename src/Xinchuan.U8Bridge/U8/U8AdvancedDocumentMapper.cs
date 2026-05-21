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
