using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Xinchuan.U8Bridge.Models;

namespace Xinchuan.U8Bridge.Services
{
    public sealed partial class U8DatabaseQueryService
    {
        private static IDictionary<string, object> ReadRow(IDataRecord reader)
        {
            var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < reader.FieldCount; index++)
            {
                object value = reader.GetValue(index);
                row[reader.GetName(index)] = value == DBNull.Value ? null : value;
            }

            return row;
        }

        private static string BuildPagedSql(
            string table,
            string tableAlias,
            string joinTable,
            string joinClause,
            ColumnSpec[] columns,
            IList<SqlFilter> filters,
            string orderColumn)
        {
            string from = "[" + table + "]" + Alias(tableAlias);
            if (!string.IsNullOrWhiteSpace(joinTable))
            {
                from += " INNER JOIN [" + joinTable + "] " + joinClause;
            }

            string where = filters.Count == 0 ? string.Empty : " WHERE " + string.Join(" AND ", filters.Select(f => f.Expression));
            string inner = string.Join(", ", columns.Select(c => c.SelectExpression));
            string outer = string.Join(", ", columns.Select(c => "q.[" + c.Alias + "]"));
            return "SELECT " + outer + " FROM (SELECT " + inner + ", ROW_NUMBER() OVER (ORDER BY "
                + orderColumn + ") AS rn FROM " + from + where + ") q WHERE q.rn BETWEEN @Offset AND @Limit ORDER BY q.rn";
        }

        private static string BuildBomSql(BomQueryRequest request)
        {
            string versionFilter = string.IsNullOrWhiteSpace(request.Version)
                ? string.Empty
                : " AND CAST(b.Version AS NVARCHAR(50)) = @Version";
            return @"
SELECT q.rootMaterialCode,
       q.productModel,
       q.version,
       q.versionDate,
       q.parentMaterialCode,
       q.parentMaterialName,
       q.parentSpecification,
       q.parentUnit,
       q.materialCode,
       q.materialName,
       q.materialSpecification,
       q.unit,
       q.quantity,
       q.baseQuantity,
       q.baseBaseQuantity,
       q.lossRate,
       q.processLineNo,
       q.effectiveDate,
       q.expireDate,
       q.levelNo,
       q.sortOrder
FROM (
    SELECT pp.InvCode AS rootMaterialCode,
           pp.InvCode AS productModel,
           CAST(b.Version AS NVARCHAR(50)) AS version,
           b.VersionEffDate AS versionDate,
           pp.InvCode AS parentMaterialCode,
           pi.cInvName AS parentMaterialName,
           pi.cInvStd AS parentSpecification,
           pi.cComUnitCode AS parentUnit,
           cp.InvCode AS materialCode,
           ci.cInvName AS materialName,
           ci.cInvStd AS materialSpecification,
           ci.cComUnitCode AS unit,
           COALESCE(c.BaseQtyN / NULLIF(c.BaseQtyD, 0), c.BaseQtyN, 0) AS quantity,
           c.BaseQtyN AS baseQuantity,
           c.BaseQtyD AS baseBaseQuantity,
           c.CompScrap AS lossRate,
           oc.OpSeq AS processLineNo,
           c.EffBegDate AS effectiveDate,
           c.EffEndDate AS expireDate,
           1 AS levelNo,
           oc.SortSeq AS sortOrder,
           ROW_NUMBER() OVER (ORDER BY b.VersionEffDate DESC, b.BomId DESC, oc.SortSeq ASC) AS rn
    FROM bom_bom b
    INNER JOIN bom_parent p ON p.BomId = b.BomId
    INNER JOIN bas_part pp ON pp.PartId = p.ParentId
    INNER JOIN bom_opcomponent oc ON oc.BomId = b.BomId
    INNER JOIN bom_component c ON c.ComponentId = oc.ComponentId
    INNER JOIN bas_part cp ON cp.PartId = c.PartId
    LEFT JOIN Inventory pi ON pi.cInvCode = pp.InvCode
    LEFT JOIN Inventory ci ON ci.cInvCode = cp.InvCode
    WHERE pp.InvCode = @ProductModel" + versionFilter + @"
) q
WHERE q.rn BETWEEN @Offset AND @Limit
ORDER BY q.rn";
        }

        private static string Alias(string tableAlias)
        {
            return string.IsNullOrWhiteSpace(tableAlias) ? string.Empty : " " + tableAlias;
        }

        private static QueryResult CreateResult(BaseBusinessRequest request)
        {
            int pageNo = ReadIntProperty(request, "PageNo", 1);
            int pageSize = ReadIntProperty(request, "PageSize", 200);
            return new QueryResult
            {
                PageNo = Math.Max(1, pageNo),
                PageSize = Math.Min(MaxPageSize, Math.Max(1, pageSize))
            };
        }

        private static int ReadIntProperty(object target, string propertyName, int defaultValue)
        {
            var property = target.GetType().GetProperty(propertyName);
            object value = property == null ? null : property.GetValue(target, null);
            return value is int ? (int)value : defaultValue;
        }

        private static void AddPagingParameters(SqlCommand command, QueryResult result)
        {
            int offset = ((result.PageNo - 1) * result.PageSize) + 1;
            command.Parameters.Add("@Offset", SqlDbType.Int).Value = offset;
            command.Parameters.Add("@Limit", SqlDbType.Int).Value = offset + result.PageSize;
        }

        private ISet<string> LoadColumns(SqlConnection connection, string table)
        {
            const string sql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName";
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@TableName", SqlDbType.NVarChar, 128).Value = table;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columns.Add(Convert.ToString(reader[0]));
                    }
                }
            }

            return columns;
        }

        private static ColumnSpec[] SelectExisting(
            IEnumerable<ColumnSpec> candidates,
            ISet<string> schema,
            IDictionary<string, ISet<string>> schemas)
        {
            return candidates.Where(c => c.ExistsIn(schema, schemas)).ToArray();
        }

        private static IList<SqlFilter> SelectExistingFilters(
            IEnumerable<SqlFilter> candidates,
            ISet<string> schema,
            IDictionary<string, ISet<string>> schemas)
        {
            return candidates.Where(c => c.ExistsIn(schema, schemas)).ToList();
        }

        private static string ResolveOrder(
            string candidate,
            ISet<string> schema,
            IDictionary<string, ISet<string>> schemas,
            string fallback)
        {
            return new ColumnSpec(candidate, "order").ExistsIn(schema, schemas) ? candidate : fallback;
        }

        private static void EnsureColumns(string table, ColumnSpec[] selected)
        {
            if (selected.Length == 0)
            {
                throw new InvalidOperationException("U8 数据表字段不匹配: " + table);
            }
        }

        private void EnsureEnabled()
        {
            if (!options.Enabled)
            {
                throw new InvalidOperationException("U8 只读数据库配置未启用");
            }
        }

        private string BuildConnectionString()
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = options.Server,
                InitialCatalog = options.Database,
                UserID = options.User,
                Password = options.Password,
                IntegratedSecurity = false,
                ConnectTimeout = 10,
                Encrypt = false
            };
            return builder.ConnectionString;
        }

        private static ColumnSpec Col(string expression, string alias)
        {
            return new ColumnSpec(expression, alias);
        }
    }
}
