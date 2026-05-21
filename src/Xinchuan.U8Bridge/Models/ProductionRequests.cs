using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Xinchuan.U8Bridge.Models
{
    public sealed class ManufactureOrderAddRequest : BaseBusinessRequest
    {
        [Required]
        public string OrderNo { get; set; }

        public string Maker { get; set; }

        public string CreateDate { get; set; }

        public string CreateTime { get; set; }

        [Required]
        public IList<ManufactureOrderItem> Items { get; set; }
    }

    public sealed class ManufactureOrderSimpleRequest : BaseBusinessRequest
    {
        [Required]
        public string OrderNo { get; set; }
    }

    public sealed class ManufactureOrderAuditRequest : BaseBusinessRequest
    {
        [Required]
        public string OrderNo { get; set; }

        public string Verifier { get; set; }

        public string BusinessType { get; set; } = "普通采购";
    }

    public sealed class PurchaseOrderConfirmRequest : BaseBusinessRequest
    {
        [Required]
        public string PurchaseOrderNo { get; set; }

        public string U8Id { get; set; }

        public string TimeStamp { get; set; }

        public string Verifier { get; set; }
    }

    public sealed class ManufactureOrderItem
    {
        public int LineNo { get; set; }

        [Required]
        public string MaterialCode { get; set; }

        public string MaterialName { get; set; }

        [Required]
        public string StartDate { get; set; }

        [Required]
        public string DueDate { get; set; }

        public decimal Quantity { get; set; }

        public int OrderClass { get; set; } = 1;

        public string WarehouseCode { get; set; }

        public string DepartmentCode { get; set; }

        public string OrderTypeCode { get; set; }

        public string LotNo { get; set; }

        public string Remark { get; set; }

        public IList<ManufactureOrderComponentItem> Components { get; set; }
    }

    public sealed class ManufactureOrderComponentItem
    {
        public int LineNo { get; set; }

        public string OperationSeq { get; set; }

        [Required]
        public string MaterialCode { get; set; }

        public string MaterialName { get; set; }

        public decimal BaseQtyNumerator { get; set; }

        public decimal BaseQtyDenominator { get; set; } = 1;

        public decimal Quantity { get; set; }

        public string RequiredDate { get; set; }

        public string WarehouseCode { get; set; }

        public string BatchNo { get; set; }

        public string Remark { get; set; }
    }
}
