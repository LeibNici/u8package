using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Xinchuan.U8Bridge.Models;

namespace Xinchuan.U8Bridge.Services
{
    public sealed partial class U8DatabaseQueryService
    {
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
           b.BomState AS [bomState],
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
    }
}
