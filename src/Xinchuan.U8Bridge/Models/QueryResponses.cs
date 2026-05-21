using System.Collections.Generic;

namespace Xinchuan.U8Bridge.Models
{
    public sealed class QueryResult
    {
        public int PageNo { get; set; }

        public int PageSize { get; set; }

        public bool HasMore { get; set; }

        public IList<IDictionary<string, object>> Items { get; set; } =
            new List<IDictionary<string, object>>();
    }
}
