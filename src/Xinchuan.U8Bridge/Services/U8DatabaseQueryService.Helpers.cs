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
