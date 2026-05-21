using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Xinchuan.U8Bridge.Configuration;
using Xinchuan.U8Bridge.Models;

namespace Xinchuan.U8Bridge.Services
{
    public sealed partial class U8DatabaseQueryService
    {
        private const int MaxPageSize = 500;
        private const int DefaultCommandTimeoutSeconds = 15;
        private readonly U8DatabaseOptions options;

        public U8DatabaseQueryService(U8DatabaseOptions options)
        {
            this.options = options ?? new U8DatabaseOptions();
        }

        public QueryResult QueryCustomers(MasterQueryRequest request)
        {
            var columns = new[]
            {
                Col("cCusCode", "customerCode"), Col("cCusName", "customerName"),
                Col("cCusAbbName", "shortName"), Col("cCCCode", "categoryCode"),
                Col("cCusAddress", "address"), Col("cCusPhone", "phone"),
                Col("cCusPerson", "contact"), Col("dCusCreateDatetime", "createdAt"),
                Col("dModifyDate", "updatedAt")
            };
            return QuerySingle("Customer", columns, request, BuildMasterWhere(request, "cCusCode", "cCusName", "cCusAbbName"), "cCusCode");
        }

        public QueryResult QueryMaterials(MasterQueryRequest request)
        {
            var columns = new[]
            {
                Col("cInvCode", "materialCode"), Col("cInvName", "materialName"),
                Col("cInvStd", "specification"), Col("cInvCCode", "categoryCode"),
                Col("cComUnitCode", "unitCode"), Col("cInvAddCode", "drawingNo"),
                Col("bSale", "saleEnabled"), Col("bPurchase", "purchaseEnabled"),
                Col("bSelf", "selfMade"), Col("dModifyDate", "updatedAt")
            };
            return QuerySingle("Inventory", columns, request, BuildMasterWhere(request, "cInvCode", "cInvName", "cInvStd"), "cInvCode");
        }

        public QueryResult QuerySuppliers(MasterQueryRequest request)
        {
            var columns = new[]
            {
                Col("cVenCode", "supplierCode"), Col("cVenName", "supplierName"),
                Col("cVenAbbName", "shortName"), Col("cVCCode", "categoryCode"),
                Col("cVenAddress", "address"), Col("cVenPhone", "phone"),
                Col("cVenPerson", "contact"), Col("dVenDevDate", "createdAt"),
                Col("dModifyDate", "updatedAt")
            };
            return QuerySingle("Vendor", columns, request, BuildMasterWhere(request, "cVenCode", "cVenName", "cVenAbbName"), "cVenCode");
        }

        public QueryResult QueryInventory(InventoryQueryRequest request)
        {
            var filters = new List<SqlFilter>();
            AddEquals(filters, "cWhCode", request.WarehouseCode);
            AddEquals(filters, "cInvCode", request.MaterialCode);
            AddEquals(filters, "cBatch", request.BatchNo);
            var columns = new[]
            {
                Col("cWhCode", "warehouseCode"), Col("cInvCode", "materialCode"),
                Col("cBatch", "batchNo"), Col("iQuantity", "quantity"),
                Col("fOutQuantity", "outQuantity"), Col("fInQuantity", "inQuantity"),
                Col("dVDate", "validDate")
            };
            return QuerySingle("CurrentStock", columns, request, filters, "cInvCode");
        }

        public QueryResult QueryInTransit(InTransitQueryRequest request)
        {
            var filters = new List<SqlFilter>();
            AddEquals(filters, "m.cVenCode", request.SupplierCode);
            AddEquals(filters, "d.cInvCode", request.MaterialCode);
            AddDateRange(filters, "m.dPODate", request.DateFrom, request.DateTo);
            var columns = new[]
            {
                Col("m.cPOID", "purchaseOrderNo"), Col("m.dPODate", "purchaseDate"),
                Col("m.cVenCode", "supplierCode"), Col("d.cInvCode", "materialCode"),
                Col("d.iQuantity", "quantity"), Col("d.iArrQTY", "arrivedQuantity"),
                Col("d.dArriveDate", "plannedArriveDate")
            };
            return QueryJoin("PO_Podetails", "d", "PO_Pomain", "m", "d.POID = m.POID", columns, request, filters, "m.dPODate");
        }

        public QueryResult QueryMaterialPrice(MaterialPriceQueryRequest request)
        {
            var filters = new List<SqlFilter>();
            AddEquals(filters, "cInvCode", request.MaterialCode);
            var columns = new[]
            {
                Col("cInvCode", "materialCode"), Col("cInvName", "materialName"),
                Col("iInvSCost", "saleCost"), Col("iInvSPrice", "salePrice"),
                Col("iInvRCost", "referenceCost"), Col("iInvNCost", "newCost"),
                Col("dModifyDate", "updatedAt")
            };
            return QuerySingle("Inventory", columns, request, filters, "cInvCode");
        }

        private QueryResult QuerySingle(
            string table,
            ColumnSpec[] columns,
            BaseBusinessRequest request,
            IList<SqlFilter> filters,
            string orderColumn)
        {
            EnsureEnabled();
            using (var connection = new SqlConnection(BuildConnectionString()))
            {
                connection.Open();
                var schema = LoadColumns(connection, table);
                ColumnSpec[] selected = SelectExisting(columns, schema, null);
                filters = SelectExistingFilters(filters, schema, null);
                EnsureColumns(table, selected);
                string order = ResolveOrder(orderColumn, schema, null, selected[0].Expression);
                string sql = BuildPagedSql(table, null, null, null, selected, filters, order);
                return ExecuteQuery(connection, sql, filters, request);
            }
        }

        private QueryResult QueryJoin(
            string leftTable,
            string leftAlias,
            string rightTable,
            string rightAlias,
            string joinCondition,
            ColumnSpec[] columns,
            BaseBusinessRequest request,
            IList<SqlFilter> filters,
            string orderColumn)
        {
            EnsureEnabled();
            using (var connection = new SqlConnection(BuildConnectionString()))
            {
                connection.Open();
                var schemas = new Dictionary<string, ISet<string>>
                {
                    { leftAlias, LoadColumns(connection, leftTable) },
                    { rightAlias, LoadColumns(connection, rightTable) }
                };
                ColumnSpec[] selected = SelectExisting(columns, null, schemas);
                filters = SelectExistingFilters(filters, null, schemas);
                EnsureColumns(leftTable + "/" + rightTable, selected);
                string order = ResolveOrder(orderColumn, null, schemas, selected[0].Expression);
                string sql = BuildPagedSql(leftTable, leftAlias, rightTable, rightAlias + " ON " + joinCondition, selected, filters, order);
                return ExecuteQuery(connection, sql, filters, request);
            }
        }

        private QueryResult ExecuteQuery(SqlConnection connection, string sql, IList<SqlFilter> filters, BaseBusinessRequest request)
        {
            QueryResult result = CreateResult(request);
            using (var command = new SqlCommand(sql, connection))
            {
                command.CommandTimeout = DefaultCommandTimeoutSeconds;
                AddPagingParameters(command, result);
                foreach (SqlFilter filter in filters)
                {
                    command.Parameters.Add(filter.Parameter, filter.DbType).Value = filter.Value;
                }

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Items.Add(ReadRow(reader));
                    }
                }
            }

            result.HasMore = result.Items.Count > result.PageSize;
            if (result.HasMore)
            {
                result.Items.RemoveAt(result.Items.Count - 1);
            }

            return result;
        }
    }
}
