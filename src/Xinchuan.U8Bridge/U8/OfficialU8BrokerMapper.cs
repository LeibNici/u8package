using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Xinchuan.U8Bridge.Models;

namespace Xinchuan.U8Bridge.U8
{
    public static class OfficialU8BrokerMapper
    {
        public static U8BrokerCall Map(OfficialApiInvokeRequest request)
        {
            var call = new U8BrokerCall
            {
                VoucherType = request.VoucherType,
                ReturnIdName = EmptyToNull(request.ReturnIdName),
                BooleanReturn = request.BooleanReturn
            };
            CopyValues(request.NormalValues, call.NormalValues);
            CopyValues(request.ContextValues, call.ContextValues);
            AddBusinessObjects(request.BusinessObjects, call);
            AddExtensionObjects(request.ExtensionObjects, call);
            AddResultReader(request.ResultNames, call);
            return call;
        }

        private static void CopyValues(
            IDictionary<string, object> source,
            IDictionary<string, object> target)
        {
            if (source == null)
            {
                return;
            }

            foreach (KeyValuePair<string, object> item in source)
            {
                if (!string.IsNullOrWhiteSpace(item.Key))
                {
                    target[item.Key] = ConvertValue(item.Value);
                }
            }
        }

        private static void AddBusinessObjects(
            IList<OfficialBoObjectRequest> source,
            U8BrokerCall call)
        {
            if (source == null)
            {
                return;
            }

            foreach (OfficialBoObjectRequest item in source)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Name))
                {
                    continue;
                }

                call.BusinessObjects.Add(MapBusinessObject(item));
            }
        }

        private static U8BoObject MapBusinessObject(OfficialBoObjectRequest item)
        {
            var target = new U8BoObject(item.Name);
            if (item.Rows == null)
            {
                return target;
            }

            foreach (IDictionary<string, object> row in item.Rows)
            {
                var fields = new Dictionary<string, object>();
                CopyValues(row, fields);
                target.Rows.Add(fields);
            }

            return target;
        }

        private static void AddExtensionObjects(
            IList<OfficialExtBoObjectRequest> source,
            U8BrokerCall call)
        {
            if (source == null)
            {
                return;
            }

            foreach (OfficialExtBoObjectRequest item in source)
            {
                U8ExtBoObject mapped = MapExtensionObject(item);
                if (mapped != null)
                {
                    call.ExtensionObjects.Add(mapped);
                }
            }
        }

        private static U8ExtBoObject MapExtensionObject(OfficialExtBoObjectRequest item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Name))
            {
                return null;
            }

            var target = new U8ExtBoObject(item.Name);
            if (item.Rows == null)
            {
                return target;
            }

            foreach (OfficialExtBoRowRequest row in item.Rows)
            {
                target.Rows.Add(MapExtensionRow(row));
            }

            return target;
        }

        private static U8ExtBoRow MapExtensionRow(OfficialExtBoRowRequest row)
        {
            var target = new U8ExtBoRow();
            if (row == null)
            {
                return target;
            }

            CopyValues(row.Fields, target.Fields);
            AddChildExtensionObjects(row.Children, target);
            return target;
        }

        private static void AddChildExtensionObjects(
            IList<OfficialExtBoObjectRequest> children,
            U8ExtBoRow target)
        {
            if (children == null)
            {
                return;
            }

            foreach (OfficialExtBoObjectRequest child in children)
            {
                U8ExtBoObject mapped = MapExtensionObject(child);
                if (mapped != null)
                {
                    target.Children[mapped.Name] = mapped;
                }
            }
        }

        private static void AddResultReader(IList<string> names, U8BrokerCall call)
        {
            if (names == null || names.Count == 0)
            {
                return;
            }

            call.DataReader = (broker, reflection) => ReadResults(names, broker, reflection);
        }

        private static IDictionary<string, object> ReadResults(
            IList<string> names,
            object broker,
            U8Reflection reflection)
        {
            var results = new Dictionary<string, object>();
            foreach (string name in names)
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    results[name] = reflection.Call(broker, "GetResult", name);
                }
            }

            return results;
        }

        private static object ConvertValue(object value)
        {
            var token = value as JToken;
            if (token != null)
            {
                return ConvertToken(token);
            }

            return value;
        }

        private static object ConvertToken(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                return null;
            }

            var special = token as JObject;
            if (special != null)
            {
                return ConvertSpecialValue(special);
            }

            var value = token as JValue;
            return value == null ? token.ToObject<object>() : value.Value;
        }

        private static object ConvertSpecialValue(JObject value)
        {
            string special = Convert.ToString(value.GetValue("special", StringComparison.OrdinalIgnoreCase));
            if (string.Equals(special, "domDocument", StringComparison.OrdinalIgnoreCase))
            {
                return U8BrokerSpecialValue.DomDocument();
            }

            if (string.Equals(special, "emptyObject", StringComparison.OrdinalIgnoreCase))
            {
                return U8BrokerSpecialValue.EmptyObject();
            }

            if (string.Equals(special, "com", StringComparison.OrdinalIgnoreCase))
            {
                return U8BrokerSpecialValue.Com(Convert.ToString(value["progId"]));
            }

            return value.ToObject<Dictionary<string, object>>();
        }

        private static string EmptyToNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}
