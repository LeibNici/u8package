using System;
using System.Collections.Generic;
using Xinchuan.U8Bridge.Models;

namespace Xinchuan.U8Bridge.U8
{
    public static class U8BomDocumentMapper
    {
        public static U8BrokerCall MapAdd(BomAddRequest request)
        {
            var call = new U8BrokerCall { BooleanReturn = true };
            var extbo = new U8ExtBoObject("extbo");
            var head = new U8ExtBoRow();
            Put(head.Fields, "BomId", 0, "InvCode", request.ParentMaterialCode);
            Put(head.Fields, "InvName", request.ParentMaterialName, "InvStd", request.ParentSpecification);
            Put(head.Fields, "InvUnitName", request.ParentUnitName, "InvUnit", request.ParentUnitCode);
            Put(head.Fields, "CreateUser", request.Maker, "ModifyUser", request.Maker);
            Put(head.Fields, "CreateDate", request.CreateDate, "CreateTime", request.CreateTime);
            Put(head.Fields, "BomType", request.BomType, "Version", request.Version);
            Put(head.Fields, "VersionDesc", request.VersionDesc, "VersionEffDate", request.VersionEffDate);
            Put(head.Fields, "ParentScrap", request.ParentScrap);
            head.Children["Bom_Component"] = MapComponents(request);
            extbo.Rows.Add(head);
            call.ExtensionObjects.Add(extbo);
            return call;
        }

        public static U8BrokerCall MapAction(BomActionRequest request, bool readData)
        {
            var call = new U8BrokerCall { BooleanReturn = true };
            call.NormalValues["partid"] = request.PartId.Value;
            call.NormalValues["bomtype"] = request.BomType;
            call.NormalValues["versionoridencode"] = request.VersionOrIdentCode;
            if (readData)
            {
                call.DataReader = ReadData;
            }

            return call;
        }

        private static U8ExtBoObject MapComponents(BomAddRequest request)
        {
            var components = new U8ExtBoObject("Bom_Component");
            foreach (BomComponentItem item in request.Items)
            {
                var row = new U8ExtBoRow();
                Put(row.Fields, "DSortSeq", item.LineNo, "DOpSeq", Any(item.OperationSeq, "0000"));
                Put(row.Fields, "DInvCode", item.MaterialCode, "DInvName", item.MaterialName);
                Put(row.Fields, "DInvStd", item.Specification, "DInvUnit", item.UnitCode);
                Put(row.Fields, "DInvUnitName", item.UnitName, "DBaseQtyN", item.BaseQtyNumerator);
                Put(row.Fields, "DBaseQtyD", item.BaseQtyDenominator, "DQty", item.Quantity);
                Put(row.Fields, "DCompScrap", item.ScrapRate, "DFVFlag", item.FixedQtyFlag);
                Put(row.Fields, "DWIPType", item.SupplyType, "DEffBegDate", Any(item.EffectiveDate, request.VersionEffDate));
                Put(row.Fields, "DEffEndDate", Any(item.ExpireDate, "2099-12-31"), "DPlanRate", item.PlanRate);
                Put(row.Fields, "DByproductFlag", 0, "DAccuCostFlag", 1, "DOptionalFlag", 0);
                Put(row.Fields, "DMutexRule", 0, "DProductType", 0, "DWhCode", item.WarehouseCode);
                Put(row.Fields, "DDeptCode", item.DepartmentCode, "DRemark", item.Remark);
                components.Rows.Add(row);
            }

            return components;
        }

        private static object ReadData(object broker, U8Reflection reflection)
        {
            object extbo = reflection.GetExtBoEntity(broker, "extbo");
            var rows = new List<Dictionary<string, object>>();
            int count = reflection.GetExtItemCount(extbo);
            for (int index = 0; index < count; index++)
            {
                object item = reflection.GetExtItem(extbo, index);
                var row = ReadHead(item, reflection);
                row["components"] = ReadComponents(item, reflection);
                rows.Add(row);
            }

            return new Dictionary<string, object> { { "items", rows } };
        }

        private static Dictionary<string, object> ReadHead(object item, U8Reflection reflection)
        {
            return new Dictionary<string, object>
            {
                { "bomId", Value(item, reflection, "BomId") },
                { "partId", Value(item, reflection, "PartId") },
                { "parentMaterialCode", Value(item, reflection, "InvCode") },
                { "parentMaterialName", Value(item, reflection, "InvName") },
                { "parentSpecification", Value(item, reflection, "InvStd") },
                { "parentUnit", Value(item, reflection, "InvUnitName") },
                { "version", Value(item, reflection, "Version") },
                { "versionDesc", Value(item, reflection, "VersionDesc") },
                { "versionEffDate", Value(item, reflection, "VersionEffDate") },
                { "bomType", Value(item, reflection, "BomType") },
                { "bomState", Value(item, reflection, "BomState") }
            };
        }

        private static IList<Dictionary<string, object>> ReadComponents(object item, U8Reflection reflection)
        {
            var rows = new List<Dictionary<string, object>>();
            object components = reflection.GetSubEntity(item, "Bom_Component");
            int count = reflection.GetExtItemCount(components);
            for (int index = 0; index < count; index++)
            {
                object component = reflection.GetExtItem(components, index);
                rows.Add(ReadComponent(component, reflection));
            }

            return rows;
        }

        private static Dictionary<string, object> ReadComponent(object component, U8Reflection reflection)
        {
            return new Dictionary<string, object>
            {
                { "lineNo", Value(component, reflection, "DSortSeq") },
                { "operationSeq", Value(component, reflection, "DOpSeq") },
                { "materialCode", Value(component, reflection, "DInvCode") },
                { "materialName", Value(component, reflection, "DInvName") },
                { "specification", Value(component, reflection, "DInvStd") },
                { "unit", Value(component, reflection, "DInvUnitName") },
                { "baseQtyNumerator", Value(component, reflection, "DBaseQtyN") },
                { "baseQtyDenominator", Value(component, reflection, "DBaseQtyD") },
                { "quantity", Value(component, reflection, "DQty") },
                { "scrapRate", Value(component, reflection, "DCompScrap") },
                { "effectiveDate", Value(component, reflection, "DEffBegDate") },
                { "expireDate", Value(component, reflection, "DEffEndDate") },
                { "warehouseCode", Value(component, reflection, "DWhCode") },
                { "departmentCode", Value(component, reflection, "DDeptCode") },
                { "remark", Value(component, reflection, "DRemark") }
            };
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

        private static object Value(object item, U8Reflection reflection, string field)
        {
            try
            {
                return reflection.GetExtValue(item, field);
            }
            catch
            {
                return null;
            }
        }
    }
}
