using System;
using System.Collections.Generic;

namespace Xinchuan.U8Bridge.U8
{
    internal static class U8BrokerMapperBuilder
    {
        public static U8BrokerCall CreateSaveCall(int voucherType, string returnIdName)
        {
            var call = new U8BrokerCall { VoucherType = voucherType, ReturnIdName = returnIdName };
            call.NormalValues["VoucherState"] = 0;
            call.NormalValues[returnIdName] = string.Empty;
            call.NormalValues["DomConfig"] = U8BrokerSpecialValue.DomDocument();
            return call;
        }

        public static U8BrokerCall CreateStockAddCall(string type)
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

        public static U8BoObject OneRow(string name)
        {
            var bo = new U8BoObject(name);
            bo.Rows.Add(new Dictionary<string, object>());
            return bo;
        }

        public static IDictionary<string, object> NewBodyRow(
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

        public static void Put(U8BoObject bo, params object[] items)
        {
            Put(bo.Rows[0], items);
        }

        public static void Put(IDictionary<string, object> row, params object[] items)
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

        public static void PutAll(IDictionary<string, object> row, params object[] items)
        {
            for (int i = 0; i + 1 < items.Length; i += 2)
            {
                row[Convert.ToString(items[i])] = items[i + 1] ?? string.Empty;
            }
        }

        public static string Any(params string[] values)
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
