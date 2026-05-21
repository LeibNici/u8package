using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Xinchuan.U8Bridge.Services
{
    internal sealed class ColumnSpec
    {
        public ColumnSpec(string expression, string alias)
        {
            Expression = expression;
            Alias = alias;
        }

        public string Expression { get; }

        public string Alias { get; }

        public string SelectExpression => Expression + " AS [" + Alias + "]";

        public bool ExistsIn(ISet<string> schema, IDictionary<string, ISet<string>> schemas)
        {
            string[] parts = Expression.Split('.');
            if (parts.Length == 2 && schemas != null && schemas.TryGetValue(parts[0], out var tableSchema))
            {
                return tableSchema.Contains(parts[1]);
            }

            return schema != null && schema.Contains(Expression);
        }
    }

    internal sealed class SqlFilter
    {
        public SqlFilter(string expression, string parameter, SqlDbType dbType, object value, params string[] requiredColumns)
        {
            Expression = expression;
            Parameter = parameter;
            DbType = dbType;
            Value = value;
            RequiredColumns = requiredColumns ?? new string[0];
        }

        public string Expression { get; }

        public string Parameter { get; }

        public SqlDbType DbType { get; }

        public object Value { get; }

        private IEnumerable<string> RequiredColumns { get; }

        public bool ExistsIn(ISet<string> schema, IDictionary<string, ISet<string>> schemas)
        {
            return RequiredColumns.All(c => new ColumnSpec(c, c).ExistsIn(schema, schemas));
        }
    }
}
