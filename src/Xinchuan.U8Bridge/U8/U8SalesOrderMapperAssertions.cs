using System;
using System.Collections.Generic;
using Xinchuan.U8Bridge.Models;

namespace Xinchuan.U8Bridge.U8
{
    internal static class U8SalesOrderMapperAssertions
    {
        public static void AssertRelease45Sample()
        {
            AssertRelease45Shape(CreateRelease45SampleCall());
        }

        public static U8BrokerCall CreateRelease45SampleCall()
        {
            var request = new SalesOrderSaveRequest
            {
                RequestId = "REQ-MAPPER-SNAPSHOT",
                OrderNo = "SO-MAPPER-SNAPSHOT",
                OrderDate = "2026-05-29",
                CustomerCode = "C001",
                CustomerName = "CustomerA",
                DepartmentCode = "D001",
                DepartmentName = "Sales",
                SalesTypeCode = "01",
                SalesTypeName = "NormalSale",
                Maker = "demo",
                TaxRate = 13,
                Items = new List<SalesOrderItem>
                {
                    new SalesOrderItem
                    {
                        LineNo = 1,
                        MaterialCode = "M001",
                        MaterialName = "MaterialA",
                        Quantity = 2,
                        DeliveryDate = "2026-06-01",
                        TaxUnitPrice = 10,
                        TaxAmount = 20
                    }
                }
            };

            return U8BrokerDocumentMapper.Map(U8ApiCall.Create(
                "U8API/SaleOrder/Save",
                "sales-order-save",
                request.OrderNo,
                request));
        }

        public static void AssertRelease45Shape(U8BrokerCall call)
        {
            AssertValue("VoucherType", call.VoucherType, 12);
            AssertValue("ReturnIdName", call.ReturnIdName, "vNewID");
            AssertNormalSpecial(call, "DomConfig");

            IDictionary<string, object> head = FindFirstRow(call, "domHead");
            AssertValue("domHead.ivtid", head["ivtid"], 131507);
            AssertValue("domHead.iexchrate", head["iexchrate"], 1);

            IDictionary<string, object> body = FindFirstRow(call, "domBody");
            AssertValue("domBody.cunitid", body["cunitid"], "28");
            AssertValue("domBody.cgroupcode", body["cgroupcode"], "1");
            AssertValue("domBody.kl", body["kl"], 100);
            AssertValue("domBody.kl2", body["kl2"], 100);
            AssertValue("domBody.bsaleprice", body["bsaleprice"], true);
            AssertValue("domBody.bgift", body["bgift"], false);
        }

        private static void AssertNormalSpecial(U8BrokerCall call, string name)
        {
            object value;
            if (!call.NormalValues.TryGetValue(name, out value) || !(value is U8BrokerSpecialValue))
            {
                throw new InvalidOperationException("Missing broker normal special value: " + name);
            }
        }

        private static IDictionary<string, object> FindFirstRow(U8BrokerCall call, string name)
        {
            foreach (U8BoObject item in call.BusinessObjects)
            {
                if (item.Name == name && item.Rows.Count > 0)
                {
                    return item.Rows[0];
                }
            }

            throw new InvalidOperationException("Missing broker business object row: " + name);
        }

        private static void AssertValue(string name, object actual, object expected)
        {
            if (!object.Equals(actual, expected))
            {
                throw new InvalidOperationException(
                    "Unexpected " + name + ". Expected: " + expected + ", Actual: " + actual);
            }
        }
    }
}
