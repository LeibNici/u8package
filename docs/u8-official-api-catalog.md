# U8 官方 API 示例目录

- 来源目录: `/Users/chenming/Downloads/u8api-csharp-export/all-service-csharp-full 2`
- manifest 条目: 334
- 已抽取官方 API 地址: 333

## 模块统计

| 模块 | 数量 |
| --- | ---: |
| 库存管理增和修改志r | 102 |
| 生产制造照档案API.TransVouch.Save_Before | 57 |
| 销售管理理单事务)r | 57 |
| U8WorkFlowPI.TransVouch.Save_Before | 51 |
| 采购管理存事件 | 26 |
| 应付款管理源管理器 | 17 |
| 应收款管理源管理器 | 12 |
| 质量管理存(外部事务)r | 6 |
| 合同管理核日志 | 3 |
| 电商订单中心日志 | 2 |
| 委外管理发票 | 1 |

## 使用方式

- `u8-official-api-catalog.csv` 是全量明细，包含官方序号、handle、中文模块、单据、动作、`U8API/...` 地址和本地示例文件相对路径。
- Swagger 中的 `x-u8-official-api` / `x-u8-official-seq` 字段应优先引用本目录。
- 不把官方示例源码全文提交到仓库；需要看字段级 BO 示例时回到本机来源目录按 `source_file` 查。
