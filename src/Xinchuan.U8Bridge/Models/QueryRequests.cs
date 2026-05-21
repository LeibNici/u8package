namespace Xinchuan.U8Bridge.Models
{
    public sealed class MasterQueryRequest : BaseBusinessRequest
    {
        public string UpdatedFrom { get; set; }

        public string UpdatedTo { get; set; }

        public string Keyword { get; set; }

        public int PageNo { get; set; } = 1;

        public int PageSize { get; set; } = 200;
    }

    public sealed class InventoryQueryRequest : BaseBusinessRequest
    {
        public string WarehouseCode { get; set; }

        public string MaterialCode { get; set; }

        public string BatchNo { get; set; }

        public string AsOfDate { get; set; }

        public int PageNo { get; set; } = 1;

        public int PageSize { get; set; } = 200;
    }

    public sealed class InTransitQueryRequest : BaseBusinessRequest
    {
        public string SupplierCode { get; set; }

        public string MaterialCode { get; set; }

        public string DateFrom { get; set; }

        public string DateTo { get; set; }

        public int PageNo { get; set; } = 1;

        public int PageSize { get; set; } = 200;
    }

    public sealed class MaterialPriceQueryRequest : BaseBusinessRequest
    {
        public string MaterialCode { get; set; }

        public string CustomerCode { get; set; }

        public string SupplierCode { get; set; }

        public string PriceDate { get; set; }

        public int PageNo { get; set; } = 1;

        public int PageSize { get; set; } = 200;
    }
}
