# U8 Bridge 与官方 API 对照

本文把 Bridge REST 路径、中文业务分类和官方 C# 示例 API 地址对齐。`status=unsupported-shell` 表示 Bridge 有统一入口但运行时仍返回 `U8_API_NOT_SUPPORTED`。

| 中文分类 | Bridge 路径 | 方法 | 业务动作 | 状态 | 官方 API 地址 | 官方序号 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 系统 | /health | GET | 健康检查 | read-only |  |  | Bridge 自身健康检查 |
| 系统 | /swagger | GET | Swagger UI | read-only |  |  | Bridge 自身文档入口 |
| 系统 | /api/u8/login-test | POST | U8 登录测试 | implemented |  |  | U8Login.clsLogin 环境验证 |
| 主数据查询 | /api/u8/customers/query | POST | 客户档案查询 | read-only-db |  |  | 只读 U8 数据库 Customer |
| 主数据查询 | /api/u8/materials/query | POST | 存货档案查询 | read-only-db |  |  | 只读 U8 数据库 Inventory |
| 主数据查询 | /api/u8/suppliers/query | POST | 供应商档案查询 | read-only-db |  |  | 只读 U8 数据库 Vendor |
| 库存查询 | /api/u8/inventory/query | POST | 现存量查询 | read-only-db |  |  | 只读 U8 数据库 CurrentStock |
| 采购查询 | /api/u8/in-transit/query | POST | 采购在途查询 | read-only-db |  |  | 只读 PO_Pomain / PO_Podetails |
| 主数据查询 | /api/u8/material-price/query | POST | 物料价格查询 | read-only-db |  |  | 只读 U8 数据库 Inventory 价格字段 |
| 生产制造-BOM | /api/u8/bom/add | POST | 新增物料清单 | implemented | U8API/BOM/BomAdd | 227 | 官方 BOM 新增示例 |
| 生产制造-BOM | /api/u8/bom/load | POST | 查询物料清单 | implemented | U8API/BOM/BomLoad | 230 | 官方 BOM 查询示例 |
| 生产制造-BOM | /api/u8/bom/part-lookup | POST | 按存货编码查 BOM 参数 | read-only-db |  |  | 只读 bas_part / bom_parent / bom_bom |
| 生产制造-BOM | /api/u8/bom/load-by-code | POST | 按存货编码查询物料清单 | implemented | U8API/BOM/BomLoad | 230 | 先只读查参数再调用官方 API |
| 生产制造-BOM | /api/u8/bom/tree-query | POST | 按存货编码查 BOM 树 | read-only-db |  |  | 只读 BOM 相关表展开树 |
| 生产制造-BOM | /api/u8/bom/audit | POST | 审核物料清单 | implemented | U8API/BOM/BomAuditing | 228 | 官方 BOM 审核示例 |
| 生产制造-BOM | /api/u8/bom/unaudit | POST | 弃审物料清单 | implemented | U8API/BOM/BomUnauditing | 231 | 官方 BOM 弃审示例 |
| 生产制造-BOM | /api/u8/bom/delete | POST | 删除物料清单 | implemented | U8API/BOM/BomDelete | 229 | 官方 BOM 删除示例 |
| 销售管理 | /api/u8/sales-order/save | POST | 新增或修改销售订单 | implemented | U8API/SaleOrder/Save | 110 | 官方销售订单保存示例 |
| 销售管理 | /api/u8/sales-order/audit | POST | 审核或弃审销售订单 | implemented | U8API/SaleOrder/Audit | 107 | 官方销售订单审核示例 |
| 销售管理 | /api/u8/consignment/save | POST | 新增或修改销售发货单 | implemented | U8API/Consignment/Save | 071 | 官方销售发货单保存示例 |
| 销售管理 | /api/u8/consignment/audit | POST | 审核或弃审销售发货单 | implemented | U8API/Consignment/Audit | 068 | 官方销售发货单审核示例 |
| 库存管理 | /api/u8/saleout/add | POST | 新增销售出库单 | implemented | U8API/saleout/Add | 185 | 官方销售出库单新增示例 |
| 库存管理 | /api/u8/saleout/audit | POST | 审核销售出库单 | implemented | U8API/saleout/Audit | 186 | 官方销售出库单审核示例 |
| 库存管理 | /api/u8/material-out/add | POST | 新增材料出库单 | implemented | U8API/MaterialOut/Add | 149 | 官方材料出库单新增示例 |
| 库存管理 | /api/u8/material-out/audit | POST | 审核材料出库单 | implemented | U8API/MaterialOut/Audit | 150 | 官方材料出库单审核示例 |
| 库存管理 | /api/u8/material-out/cancel-audit | POST | 弃审材料出库单 | implemented | U8API/MaterialOut/CancelAudit | 151 | 官方材料出库单弃审示例 |
| 库存管理 | /api/u8/material-out/delete | POST | 删除材料出库单 | implemented | U8API/MaterialOut/Delete | 152 | 官方材料出库单删除示例 |
| 库存管理 | /api/u8/material-app/add | POST | 新增领料申请单 | implemented | U8API/materialapp/Add | 143 | 官方领料申请单新增示例 |
| 库存管理 | /api/u8/material-app/audit | POST | 审核领料申请单 | implemented | U8API/materialapp/Audit | 144 | 官方领料申请单审核示例 |
| 生产制造-生产订单 | /api/u8/morder/add | POST | 新增生产订单 | implemented | U8API/MOrder/MOrderAdd | 249 | 官方生产订单新增示例 |
| 生产制造-生产订单 | /api/u8/morder/update | POST | 更新生产订单 | implemented | U8API/MOrder/MOrderUpdate | 254 | 官方生产订单更新示例 |
| 生产制造-生产订单 | /api/u8/morder/audit | POST | 审核生产订单 | implemented | U8API/MOrder/MOrderAuditing | 250 | 官方生产订单审核示例 |
| 生产制造-生产订单 | /api/u8/morder/unaudit | POST | 弃审生产订单 | implemented | U8API/MOrder/MOrderUnauditing | 253 | 官方生产订单弃审示例 |
| 生产制造-生产订单 | /api/u8/morder/delete | POST | 删除生产订单 | implemented | U8API/MOrder/MOrderDelete | 251 | 官方生产订单删除示例 |
| 生产制造-生产订单 | /api/u8/morder/load | POST | 查询生产订单 | implemented | U8API/MOrder/MOrderLoad | 252 | 官方生产订单查询示例 |
| 采购管理 | /api/u8/purchase-order/confirm | POST | 采购订单确认 | implemented | U8API/PurchaseOrder/ConfirmPO | 048 | 官方采购订单审核/确认示例 |
| 采购管理 | /api/u8/purchase-order/cancel-confirm | POST | 采购订单取消确认 | implemented | U8API/PurchaseOrder/CancelconfirmPo | 047 | 官方采购订单弃审/取消确认示例 |
| 库存管理 | /api/u8/inbound/add | POST | 新增采购入库单 | unsupported-shell | U8API/PuStoreIn/Add | 173 | 官方地址已确认，字段映射尚未接入 |
| 生产制造-计划 | /api/u8/production-plan/publish | POST | 发布生产计划 | unsupported-shell |  |  | 官方生产计划发布 API 未明确，可能需另按 Forecast/MOrder 场景确认 |
| 生产制造-报工 | /api/u8/work-report/save | POST | 新增工序流转卡完工单 | unsupported-shell | U8API/PFReport/PFReportAdd | 266 | 官方地址已确认，字段映射尚未接入 |
| 生产制造-退料 | /api/u8/material-return/add | POST | 新增生产退料单 | unsupported-shell |  |  | 官方退料单 API 未明确，需 U8 顾问确认 |
