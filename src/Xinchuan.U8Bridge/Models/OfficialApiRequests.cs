using System.Collections.Generic;

namespace Xinchuan.U8Bridge.Models
{
    public sealed class OfficialApiInvokeRequest : BaseBusinessRequest
    {
        public string BusinessNo { get; set; }

        public string DocumentType { get; set; }

        public bool Idempotent { get; set; }

        public int? VoucherType { get; set; }

        public string ReturnIdName { get; set; }

        public bool BooleanReturn { get; set; }

        public IList<string> ResultNames { get; set; }

        public IDictionary<string, object> NormalValues { get; set; }

        public IDictionary<string, object> ContextValues { get; set; }

        public IList<OfficialBoObjectRequest> BusinessObjects { get; set; }

        public IList<OfficialExtBoObjectRequest> ExtensionObjects { get; set; }
    }

    public sealed class OfficialBoObjectRequest
    {
        public string Name { get; set; }

        public IList<IDictionary<string, object>> Rows { get; set; }
    }

    public sealed class OfficialExtBoObjectRequest
    {
        public string Name { get; set; }

        public IList<OfficialExtBoRowRequest> Rows { get; set; }
    }

    public sealed class OfficialExtBoRowRequest
    {
        public IDictionary<string, object> Fields { get; set; }

        public IList<OfficialExtBoObjectRequest> Children { get; set; }
    }
}
