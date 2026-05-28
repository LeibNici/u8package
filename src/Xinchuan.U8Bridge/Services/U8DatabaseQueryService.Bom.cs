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
        private const int DefaultBomTreeDepth = 2;
        private const int MaxBomTreeDepth = 8;
        private const int DefaultBomTreeRows = 5000;

        private const string BomPartLookupSql = @"
SELECT q.[partId], q.[materialCode], q.[materialName], q.[specification],
       q.[bomId], q.[bomType], q.[versionOrIdentCode], q.[versionEffDate], q.[bomState]
FROM (
    SELECT p.PartId AS [partId],
           p.InvCode AS [materialCode],
           i.cInvName AS [materialName],
           i.cInvStd AS [specification],
           b.BomId AS [bomId],
           b.BomType AS [bomType],
           CAST(CASE WHEN b.BomType = 1 THEN b.Version ELSE b.IdentCode END AS NVARCHAR(64)) AS [versionOrIdentCode],
           b.VersionEffDate AS [versionEffDate],
           CAST(NULL AS INT) AS [bomState],
           ROW_NUMBER() OVER (ORDER BY b.VersionEffDate DESC, b.BomId DESC) AS rn
    FROM bas_part p
    INNER JOIN bom_parent bp ON bp.ParentId = p.PartId
    INNER JOIN bom_bom b ON b.BomId = bp.BomId
    LEFT JOIN Inventory i ON i.cInvCode = p.InvCode
    WHERE p.InvCode = @MaterialCode
      AND (@BomType IS NULL OR b.BomType = @BomType)
      AND (@VersionOrIdentCode IS NULL
           OR CAST(b.Version AS NVARCHAR(64)) = @VersionOrIdentCode
           OR b.IdentCode = @VersionOrIdentCode)
) q
WHERE q.rn BETWEEN @Offset AND @Limit
ORDER BY q.rn";

        private const string BomTreeLevelSqlTemplate = @"
WITH BomHeader AS (
    SELECT p.PartId AS ParentPartId,
           p.InvCode AS ParentCode,
           b.BomId,
           b.BomType,
           b.Version,
           b.IdentCode,
           b.VersionEffDate,
           ROW_NUMBER() OVER (
               PARTITION BY p.PartId
               ORDER BY b.VersionEffDate DESC, b.BomId DESC
           ) AS rn
    FROM bas_part p
    INNER JOIN bom_parent bp ON bp.ParentId = p.PartId
    INNER JOIN bom_bom b ON b.BomId = bp.BomId
    WHERE p.InvCode IN ({0})
      AND (@BomType IS NULL OR b.BomType = @BomType)
)
SELECT @RootMaterialCode AS [rootMaterialCode],
       h.ParentCode AS [parentMaterialCode],
       pi.cInvName AS [parentMaterialName],
       pi.cInvStd AS [parentSpecification],
       COALESCE(pcu.cComUnitName, NULLIF(pi.cSTComUnitCode, ''), NULLIF(pi.cComUnitCode, '')) AS [parentUnit],
       c.InvCode AS [materialCode],
       ci.cInvName AS [materialName],
       ci.cInvStd AS [materialSpecification],
       COALESCE(ccu.cComUnitName, NULLIF(ci.cSTComUnitCode, ''), NULLIF(ci.cComUnitCode, '')) AS [unit],
       CAST(CASE WHEN h.BomType = 1 THEN h.Version ELSE h.IdentCode END AS NVARCHAR(64)) AS [version],
       oc.OpSeq AS [processLineNo],
       CAST(oc.BaseQtyN AS DECIMAL(18, 6)) AS [quantity],
       CAST(oc.BaseQtyN AS DECIMAL(18, 6)) AS [baseQuantity],
       CAST(oc.BaseQtyD AS DECIMAL(18, 6)) AS [baseBaseQuantity],
       CAST(oc.CompScrap AS DECIMAL(18, 6)) AS [lossRate],
       oc.SortSeq AS [sortOrder]
FROM BomHeader h
INNER JOIN bom_opcomponent oc ON oc.BomId = h.BomId
INNER JOIN bas_part c ON c.PartId = oc.ComponentId
LEFT JOIN Inventory pi ON pi.cInvCode = h.ParentCode
LEFT JOIN Inventory ci ON ci.cInvCode = c.InvCode
LEFT JOIN ComputationUnit pcu
       ON pcu.cComunitCode = COALESCE(NULLIF(pi.cComUnitCode, ''), NULLIF(pi.cSTComUnitCode, ''))
LEFT JOIN ComputationUnit ccu
       ON ccu.cComunitCode = COALESCE(NULLIF(ci.cComUnitCode, ''), NULLIF(ci.cSTComUnitCode, ''))
WHERE h.rn = 1
  AND (@FilterVersion = 0
       OR @VersionOrIdentCode IS NULL
       OR CAST(h.Version AS NVARCHAR(64)) = @VersionOrIdentCode
       OR h.IdentCode = @VersionOrIdentCode)
ORDER BY h.ParentCode, oc.SortSeq";

        public QueryResult QueryBomParts(BomPartLookupRequest request)
        {
            EnsureBomLookupRequest(request);
            EnsureEnabled();
            using (var connection = new SqlConnection(BuildConnectionString()))
            {
                connection.Open();
                return ExecuteBomPartLookup(connection, request);
            }
        }

        public QueryResult QueryBomTree(BomPartLookupRequest request)
        {
            EnsureBomLookupRequest(request);
            EnsureEnabled();
            using (var connection = new SqlConnection(BuildConnectionString()))
            {
                connection.Open();
                return ExecuteBomTreeQuery(connection, request);
            }
        }

        private QueryResult ExecuteBomPartLookup(SqlConnection connection, BomPartLookupRequest request)
        {
            QueryResult result = CreateResult(request);
            using (var command = new SqlCommand(BomPartLookupSql, connection))
            {
                command.CommandTimeout = DefaultCommandTimeoutSeconds;
                AddPagingParameters(command, result);
                command.Parameters.Add("@MaterialCode", SqlDbType.NVarChar, 60).Value = request.MaterialCode.Trim();
                command.Parameters.Add("@BomType", SqlDbType.Int).Value = NullIfMissing(request.BomType);
                command.Parameters.Add("@VersionOrIdentCode", SqlDbType.NVarChar, 64).Value =
                    NullIfMissing(request.VersionOrIdentCode);

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

        private QueryResult ExecuteBomTreeQuery(SqlConnection connection, BomPartLookupRequest request)
        {
            var result = new QueryResult { PageNo = 1, PageSize = ResolveBomTreeRows(request) };
            string rootCode = request.MaterialCode.Trim();
            List<IDictionary<string, object>> rows = LoadBomTreeRows(connection, request, rootCode);
            AppendBomTreeRows(rootCode, GroupBomRows(rows), result, ResolveBomTreeDepth(request));
            return result;
        }

        private List<IDictionary<string, object>> LoadBomTreeRows(
            SqlConnection connection,
            BomPartLookupRequest request,
            string rootCode)
        {
            var rows = new List<IDictionary<string, object>>();
            var parents = new List<string> { rootCode };
            var expanded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int maxDepth = ResolveBomTreeDepth(request);
            for (int level = 1; level <= maxDepth && parents.Count > 0; level++)
            {
                List<IDictionary<string, object>> levelRows =
                    ExecuteBomTreeLevel(connection, request, rootCode, parents, level == 1);
                rows.AddRange(levelRows);
                foreach (string parent in parents)
                {
                    expanded.Add(parent);
                }

                parents = levelRows.Select(ReadMaterialCode)
                    .Where(code => !string.IsNullOrWhiteSpace(code) && !expanded.Contains(code))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            return rows;
        }

        private List<IDictionary<string, object>> ExecuteBomTreeLevel(
            SqlConnection connection,
            BomPartLookupRequest request,
            string rootCode,
            IList<string> parents,
            bool filterVersion)
        {
            string sql = string.Format(BomTreeLevelSqlTemplate, BuildParentParameterList(parents));
            var rows = new List<IDictionary<string, object>>();
            using (var command = new SqlCommand(sql, connection))
            {
                command.CommandTimeout = DefaultCommandTimeoutSeconds;
                for (int index = 0; index < parents.Count; index++)
                {
                    command.Parameters.Add("@Parent" + index, SqlDbType.NVarChar, 60).Value = parents[index];
                }

                command.Parameters.Add("@RootMaterialCode", SqlDbType.NVarChar, 60).Value = rootCode;
                command.Parameters.Add("@BomType", SqlDbType.Int).Value = NullIfMissing(request.BomType);
                command.Parameters.Add("@FilterVersion", SqlDbType.Bit).Value = filterVersion;
                command.Parameters.Add("@VersionOrIdentCode", SqlDbType.NVarChar, 64).Value =
                    NullIfMissing(request.VersionOrIdentCode);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rows.Add(ReadRow(reader));
                    }
                }
            }

            return rows;
        }

        private static IDictionary<string, IList<IDictionary<string, object>>> GroupBomRows(
            IEnumerable<IDictionary<string, object>> rows)
        {
            var grouped = new Dictionary<string, IList<IDictionary<string, object>>>(StringComparer.OrdinalIgnoreCase);
            foreach (IDictionary<string, object> row in rows)
            {
                string parentCode = Convert.ToString(row["parentMaterialCode"]);
                if (!grouped.TryGetValue(parentCode, out IList<IDictionary<string, object>> children))
                {
                    children = new List<IDictionary<string, object>>();
                    grouped[parentCode] = children;
                }

                children.Add(row);
            }

            return grouped;
        }

        private static void AppendBomTreeRows(
            string parentCode,
            IDictionary<string, IList<IDictionary<string, object>>> grouped,
            QueryResult result,
            int maxDepth)
        {
            AppendBomTreeRows(parentCode, grouped, result, maxDepth, 1,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }

        private static void AppendBomTreeRows(
            string parentCode,
            IDictionary<string, IList<IDictionary<string, object>>> grouped,
            QueryResult result,
            int maxDepth,
            int level,
            ISet<string> path)
        {
            if (level > maxDepth || !grouped.TryGetValue(parentCode, out var rows))
            {
                return;
            }

            if (result.Items.Count >= result.PageSize)
            {
                result.HasMore = true;
                return;
            }

            path.Add(parentCode);
            foreach (IDictionary<string, object> row in rows)
            {
                if (result.Items.Count >= result.PageSize)
                {
                    result.HasMore = true;
                    break;
                }

                string materialCode = ReadMaterialCode(row);
                row["level"] = level;
                result.Items.Add(row);
                if (!string.IsNullOrWhiteSpace(materialCode) && !path.Contains(materialCode))
                {
                    AppendBomTreeRows(materialCode, grouped, result, maxDepth, level + 1, path);
                }
            }

            path.Remove(parentCode);
        }

        private static void EnsureBomLookupRequest(BomPartLookupRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.MaterialCode))
            {
                throw new InvalidOperationException("U8 BOM 参数查询缺少 materialCode");
            }
        }

        private static object NullIfMissing(int? value)
        {
            return value.HasValue ? (object)value.Value : DBNull.Value;
        }

        private static object NullIfMissing(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim();
        }

        private static int ResolveBomTreeDepth(BomPartLookupRequest request)
        {
            int depth = request.MaxDepth <= 0 ? DefaultBomTreeDepth : request.MaxDepth;
            return Math.Min(MaxBomTreeDepth, depth);
        }

        private static int ResolveBomTreeRows(BomPartLookupRequest request)
        {
            return request.PageSize <= 0 ? DefaultBomTreeRows : request.PageSize;
        }

        private static string BuildParentParameterList(IList<string> parents)
        {
            return string.Join(",", Enumerable.Range(0, parents.Count).Select(i => "@Parent" + i));
        }

        private static string ReadMaterialCode(IDictionary<string, object> row)
        {
            return row.TryGetValue("materialCode", out object value) ? Convert.ToString(value) : null;
        }
    }
}
