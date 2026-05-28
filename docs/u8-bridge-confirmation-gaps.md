# U8 Bridge 待确认与未闭环清单

本清单依据 `u8接口联调清单.xlsx` 与当前 Bridge Release-21 能力整理，用于继续向业务、U8 顾问和后端开发确认。

## 尚未强类型化的接口

| 编号 | 清单能力 | 当前 Bridge 状态 | 需要确认/补充 |
| --- | --- | --- | --- |
| 6 | 入库单同步 U8 | `/api/u8/inbound/add` 仍为业务占位；可先用通用入口 `/api/u8/official/U8API/PuStoreIn/Add` 调官方 API | 官方序号 173；仍需确认客户现场单据类型、字段模板、业务触发点 |
| 10 | 生产计划发布回写 U8 | 可通过 `/api/u8/official/...` 调官方 catalog 中对应地址；业务强类型接口仍待确认 | U8 接收主计划还是计划明细/排产明细，字段模板和业务触发点 |
| 11 | 生产任务报工回写 U8 | `/api/u8/work-report/save` 仍为业务占位；可先用通用入口 `/api/u8/official/U8API/PFReport/PFReportAdd` 调官方 API | 官方序号 266；仍需确认是否等同客户现场报工、字段模板、是否仅推 `COMPLETE` |
| 13 | 生产退料回写 U8 | 可通过 `/api/u8/official/...` 调官方 catalog 中对应地址；业务强类型接口仍待确认 | 退料对应 U8 单据类型、普通退料与总成件退库边界 |

## 已有接口但需业务闭环验收

| 编号 | 清单能力 | 当前 Bridge 状态 | 需要确认/补充 |
| --- | --- | --- | --- |
| 4 | 订单确认推送 U8 | 已有 `/api/u8/sales-order/save`、`/api/u8/sales-order/audit`、`/api/u8/purchase-order/confirm` | 清单方法名 `syncPurchaseOrder` 与 OMS 订单确认语义冲突，需确认 U8 落销售订单还是采购订单；采购确认曾返回 U8 `拒绝访问` |
| 5 | 销售出库确认/发货回写 U8 | 已有 `/api/u8/saleout/add`、`/api/u8/saleout/audit`、`/api/u8/consignment/save`、`/api/u8/consignment/audit` | 用真实 WMS 发货数据确认字段、单据类型和是否需要先发货单后出库单 |
| 12 | 生产领料出库回写 U8 | 已有 `/api/u8/material-out/add`、`/api/u8/material-out/audit` | 明确触发点是 H5 扫码出库还是出库完成；用真实领料数据验字段 |

## 已完成并已测通的接口

| 编号 | 清单能力 | Bridge API | 当前状态 |
| --- | --- | --- | --- |
| 1 | 客户主数据同步 | `/api/u8/customers/query` | 已通过只读数据库查询测通 |
| 2 | 物料主数据同步 | `/api/u8/materials/query` | 已通过只读数据库查询测通 |
| 3 | 供应商主数据同步 | `/api/u8/suppliers/query` | 已通过只读数据库查询测通 |
| 7 | 库存查询 | `/api/u8/inventory/query` | 已通过只读数据库查询测通 |
| 8 | 采购在途量查询 | `/api/u8/in-transit/query` | 已通过只读数据库查询测通 |
| 9 | 物料价格查询 | `/api/u8/material-price/query` | 已通过只读数据库查询测通，Release-21 已修正分页 |
