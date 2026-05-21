using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Xinchuan.U8Bridge.Models
{
    public class BaseBusinessRequest
    {
        [Required]
        public string RequestId { get; set; }

        public string ProfileName { get; set; }
    }

    public sealed class LoginTestRequest : BaseBusinessRequest
    {
    }

    public sealed class AuditRequest : BaseBusinessRequest
    {
        public string OrderNo { get; set; }
        public string DeliveryNo { get; set; }
        public string OutboundNo { get; set; }
        public string U8Id { get; set; }
        public bool Verify { get; set; }
        public string Verifier { get; set; }
    }

    public sealed class StockAuditRequest : BaseBusinessRequest
    {
        public string OutboundNo { get; set; }
        public string MaterialOutNo { get; set; }
        [Required]
        public string U8Id { get; set; }
        public string VouchType { get; set; }
        public string Verifier { get; set; }
        public bool CheckStock { get; set; } = true;
        public bool BeforeCheckStock { get; set; } = true;
    }

    public sealed class SalesOrderSaveRequest : BaseBusinessRequest
    {
        [Required]
        public string OrderNo { get; set; }
        [Required]
        public string OrderDate { get; set; }
        [Required]
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        [Required]
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        [Required]
        public string SalesTypeCode { get; set; }
        public string SalesTypeName { get; set; }
        [Required]
        public string Maker { get; set; }
        public string Currency { get; set; } = "人民币";
        public decimal TaxRate { get; set; }
        public bool AutoAudit { get; set; }
        public string Memo { get; set; }
        [Required]
        public IList<SalesOrderItem> Items { get; set; }
    }

    public sealed class SalesOrderItem
    {
        public int LineNo { get; set; }
        [Required]
        public string MaterialCode { get; set; }
        public string MaterialName { get; set; }
        public string Specification { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public string UnitCode { get; set; }
        public string DeliveryDate { get; set; }
        public decimal TaxUnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
    }

    public sealed class ConsignmentSaveRequest : BaseBusinessRequest
    {
        [Required]
        public string DeliveryNo { get; set; }
        public string OrderNo { get; set; }
        [Required]
        public string DeliveryDate { get; set; }
        [Required]
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string DepartmentCode { get; set; }
        public string SalesTypeCode { get; set; }
        [Required]
        public string Maker { get; set; }
        public bool AutoAudit { get; set; }
        public string Memo { get; set; }
        [Required]
        public IList<OutboundItem> Items { get; set; }
    }

    public sealed class SaleOutAddRequest : BaseBusinessRequest
    {
        [Required]
        public string OutboundNo { get; set; }
        public string OrderNo { get; set; }
        [Required]
        public string OutboundDate { get; set; }
        [Required]
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        [Required]
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string DepartmentCode { get; set; }
        public string RdCode { get; set; }
        [Required]
        public string Maker { get; set; }
        public bool AutoAudit { get; set; }
        public string Memo { get; set; }
        [Required]
        public IList<OutboundItem> Items { get; set; }
    }

    public sealed class MaterialOutAddRequest : BaseBusinessRequest
    {
        [Required]
        public string MaterialOutNo { get; set; }
        public string SourceNo { get; set; }
        [Required]
        public string OutDate { get; set; }
        [Required]
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string RdCode { get; set; }
        public string RdName { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        [Required]
        public string Maker { get; set; }
        public bool AutoAudit { get; set; }
        public string Memo { get; set; }
        [Required]
        public IList<MaterialOutItem> Items { get; set; }
    }

    public sealed class OutboundItem
    {
        public int LineNo { get; set; }
        [Required]
        public string MaterialCode { get; set; }
        public string MaterialName { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public string BatchNo { get; set; }
        public string SourceOrderNo { get; set; }
        public int? SourceLineNo { get; set; }
    }

    public sealed class MaterialOutItem
    {
        public int LineNo { get; set; }
        [Required]
        public string MaterialCode { get; set; }
        public string MaterialName { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public string BatchNo { get; set; }
        public string WorkOrderNo { get; set; }
        public string SourceDetailId { get; set; }
    }
}
