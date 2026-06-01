using System;
using System.Collections.Generic;
using Xinchuan.U8Bridge.Models;

namespace Xinchuan.U8Bridge.U8
{
    internal static class U8OfficialMapperAssertions
    {
        public static void AssertExpandedSamples()
        {
            U8SalesOrderMapperAssertions.AssertRelease45Sample();
            AssertConsignmentShape();
            AssertStockAddShape(CreateSaleOutSampleCall(), "32");
            AssertStockAddShape(CreateMaterialOutSampleCall(), "11");
            AssertMaterialAppShape();
            AssertPurchaseConfirmShape();
        }

        private static U8BrokerCall CreateConsignmentSampleCall()
        {
            var request = new ConsignmentSaveRequest
            {
                RequestId = "REQ-CONSIGNMENT-MAPPER",
                DeliveryNo = "DL-MAPPER-SNAPSHOT",
                DeliveryDate = "2026-06-01",
                CustomerCode = "C001",
                CustomerName = "CustomerA",
                DepartmentCode = "D001",
                DepartmentName = "Sales",
                SalesTypeCode = "01",
                SalesTypeName = "NormalSale",
                Maker = "demo",
                Items = CreateOutboundItems()
            };

            return U8BrokerDocumentMapper.Map(U8ApiCall.Create(
                "U8API/Consignment/Save",
                "consignment",
                request.DeliveryNo,
                request));
        }

        private static U8BrokerCall CreateSaleOutSampleCall()
        {
            var request = new SaleOutAddRequest
            {
                RequestId = "REQ-SALEOUT-MAPPER",
                OutboundNo = "SOOUT-MAPPER-SNAPSHOT",
                OutboundDate = "2026-06-01",
                CustomerCode = "C001",
                CustomerName = "CustomerA",
                WarehouseCode = "WH001",
                WarehouseName = "MainWH",
                Maker = "demo",
                Items = CreateOutboundItems()
            };

            return U8BrokerDocumentMapper.Map(U8ApiCall.Create(
                "U8API/saleout/Add",
                "saleout",
                request.OutboundNo,
                request));
        }

        private static U8BrokerCall CreateMaterialOutSampleCall()
        {
            var request = new MaterialOutAddRequest
            {
                RequestId = "REQ-MATERIALOUT-MAPPER",
                MaterialOutNo = "MOUT-MAPPER-SNAPSHOT",
                OutDate = "2026-06-01",
                WarehouseCode = "WH001",
                WarehouseName = "MainWH",
                RdCode = "RD001",
                Maker = "demo",
                Items = new List<MaterialOutItem>
                {
                    new MaterialOutItem
                    {
                        LineNo = 1,
                        MaterialCode = "M001",
                        MaterialName = "MaterialA",
                        Quantity = 2,
                        Unit = "pcs"
                    }
                }
            };

            return U8BrokerDocumentMapper.Map(U8ApiCall.Create(
                "U8API/MaterialOut/Add",
                "material-out",
                request.MaterialOutNo,
                request));
        }

        private static U8BrokerCall CreateMaterialAppSampleCall()
        {
            var request = new MaterialAppAddRequest
            {
                RequestId = "REQ-MATERIALAPP-MAPPER",
                ApplicationNo = "MAPP-MAPPER-SNAPSHOT",
                ApplicationDate = "2026-06-01",
                Maker = "demo",
                Items = new List<MaterialAppItem>
                {
                    new MaterialAppItem
                    {
                        LineNo = 1,
                        MaterialCode = "M001",
                        MaterialName = "MaterialA",
                        Quantity = 2,
                        Unit = "pcs",
                        DueDate = "2026-06-02"
                    }
                }
            };

            return U8BrokerDocumentMapper.Map(U8ApiCall.Create(
                "U8API/materialapp/Add",
                "material-app",
                request.ApplicationNo,
                request));
        }

        private static U8BrokerCall CreatePurchaseConfirmSampleCall()
        {
            var request = new PurchaseOrderConfirmRequest
            {
                RequestId = "REQ-PO-CONFIRM-MAPPER",
                PurchaseOrderNo = "PO-MAPPER-SNAPSHOT"
            };

            return U8BrokerDocumentMapper.Map(U8ApiCall.Create(
                "U8API/PurchaseOrder/ConfirmPO",
                "purchase-order-confirm",
                request.PurchaseOrderNo,
                request));
        }

        private static IList<OutboundItem> CreateOutboundItems()
        {
            return new List<OutboundItem>
            {
                new OutboundItem
                {
                    LineNo = 1,
                    MaterialCode = "M001",
                    MaterialName = "MaterialA",
                    Quantity = 2,
                    Unit = "pcs"
                }
            };
        }

        private static void AssertConsignmentShape()
        {
            U8BrokerCall call = CreateConsignmentSampleCall();
            AssertValue("VoucherType", call.VoucherType, 9);
            AssertValue("ReturnIdName", call.ReturnIdName, "vNewID");
            IDictionary<string, object> head = FindFirstRow(call, "domHead");
            AssertValue("domHead.dlid", head["dlid"], string.Empty);
            AssertValue("domHead.cexch_name", head["cexch_name"], "人民币");
            AssertValue("domHead.iexchrate", head["iexchrate"], 1m);
            AssertValue("domHead.itaxrate", head["itaxrate"], 13m);
            AssertValue("domHead.breturnflag", head["breturnflag"], "0");
        }

        private static void AssertStockAddShape(U8BrokerCall call, string vouchType)
        {
            AssertValue("ReturnIdName", call.ReturnIdName, "VouchId");
            AssertValue("sVouchType", call.NormalValues["sVouchType"], vouchType);
            AssertNormalSpecial(call, "domPosition");
            AssertNormalSpecial(call, "cnnFrom");
            AssertNormalSpecial(call, "domMsg");
            AssertValue("VouchId", call.NormalValues["VouchId"], string.Empty);
            AssertValue("bCheck", call.NormalValues["bCheck"], true);
            AssertValue("bBeforCheckStock", call.NormalValues["bBeforCheckStock"], true);
            IDictionary<string, object> head = FindFirstRow(call, "DomHead");
            AssertValue("DomHead.id", head["id"], string.Empty);
            IDictionary<string, object> body = FindFirstRow(call, "domBody");
            AssertValue("domBody.autoid", body["autoid"], string.Empty);
            AssertValue("domBody.id", body["id"], string.Empty);
            AssertValue("domBody.irowno", body["irowno"], "1");
            AssertValue("domBody.iquantity type", body["iquantity"].GetType(), typeof(double));
            AssertValue("domBody.iinvexchrate", body["iinvexchrate"], 1d);
            AssertValue("domBody.cunitid", body["cunitid"], string.Empty);
            AssertValue("domBody.cassunit", body["cassunit"], string.Empty);
            AssertValue("domBody.cinva_unit", body["cinva_unit"], "pcs");
            AssertValue("domBody.editprop", body["editprop"], "A");
        }

        private static void AssertMaterialAppShape()
        {
            U8BrokerCall call = CreateMaterialAppSampleCall();
            IDictionary<string, object> head = FindFirstRow(call, "DomHead");
            AssertValue("DomHead.id", head["id"], string.Empty);
            AssertValue("DomHead.ddate type", head["ddate"].GetType(), typeof(DateTime));
            IDictionary<string, object> body = FindFirstRow(call, "domBody");
            AssertValue("domBody.autoid", body["autoid"], string.Empty);
            AssertValue("domBody.irowno", body["irowno"], "1");
            AssertValue("domBody.iquantity type", body["iquantity"].GetType(), typeof(double));
            if (body.ContainsKey("id") || body.ContainsKey("editprop"))
            {
                throw new InvalidOperationException("materialapp/Add must not inherit stock id/editprop body fields.");
            }
        }

        private static void AssertPurchaseConfirmShape()
        {
            U8BrokerCall call = CreatePurchaseConfirmSampleCall();
            AssertValue("VoucherType", call.VoucherType, 1);
            AssertValue("BooleanReturn", call.BooleanReturn, true);
            AssertValue("bPositive", call.ContextValues["bPositive"], true);
            AssertValue("sBillType", call.ContextValues["sBillType"], string.Empty);
            AssertValue("sBusType", call.ContextValues["sBusType"], "普通采购");
            IDictionary<string, object> head = FindFirstRow(call, "DomHead");
            AssertValue("DomHead.poid", head["poid"], string.Empty);
            AssertValue("DomHead.cpoid", head["cpoid"], "PO-MAPPER-SNAPSHOT");
            AssertValue("DomHead.ufts", head["ufts"], string.Empty);
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
