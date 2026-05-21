using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xinchuan.U8Bridge.Models;

namespace Xinchuan.U8Bridge.Services
{
    public sealed partial class U8DatabaseQueryService
    {
        private static IList<SqlFilter> BuildMasterWhere(MasterQueryRequest request, params string[] keywordColumns)
        {
            var filters = new List<SqlFilter>();
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                string expression = "(" + string.Join(" OR ", keywordColumns.Select(c => c + " LIKE @Keyword")) + ")";
                filters.Add(new SqlFilter(
                    expression,
                    "@Keyword",
                    SqlDbType.NVarChar,
                    "%" + request.Keyword.Trim() + "%",
                    keywordColumns));
            }

            AddDateRange(filters, "dModifyDate", request.UpdatedFrom, request.UpdatedTo);
            return filters;
        }

        private static void AddEquals(ICollection<SqlFilter> filters, string column, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            string name = "@P" + filters.Count;
            filters.Add(new SqlFilter(column + " = " + name, name, SqlDbType.NVarChar, value.Trim(), column));
        }

        private static void AddDateRange(ICollection<SqlFilter> filters, string column, string from, string to)
        {
            DateTime parsed;
            if (DateTime.TryParse(from, out parsed))
            {
                AddDateFilter(filters, column, ">=", parsed);
            }

            if (DateTime.TryParse(to, out parsed))
            {
                AddDateFilter(filters, column, "<=", parsed);
            }
        }

        private static void AddDateFilter(ICollection<SqlFilter> filters, string column, string op, DateTime value)
        {
            string name = "@P" + filters.Count;
            filters.Add(new SqlFilter(column + " " + op + " " + name, name, SqlDbType.DateTime, value, column));
        }
    }
}
