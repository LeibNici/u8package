# U8 Bridge 资料索引

本目录保存 U8 Bridge 的设计资料和对外接口合同。

## 文档列表

| 文档 | 用途 |
| --- | --- |
| [u8-bridge-api.md](./u8-bridge-api.md) | RESTful API 对接说明，面向 Java 后端和联调人员 |
| [u8-bridge-openapi.yaml](./u8-bridge-openapi.yaml) | OpenAPI 3.0 合同，可通过 `/openapi.yaml` 或 `/swagger` 使用 |
| [u8-bridge-field-mapping.md](./u8-bridge-field-mapping.md) | 业务 JSON 字段到 U8 `domHead` / `domBody` 字段映射 |
| [u8-bridge-error-codes.md](./u8-bridge-error-codes.md) | 标准错误码、HTTP 状态、重试和日志约定 |
| [u8-bridge-confirmation-gaps.md](./u8-bridge-confirmation-gaps.md) | 基于联调清单整理的未实现与待业务确认项 |
| [u8-official-api-catalog.md](./u8-official-api-catalog.md) | 从官方 C# 示例抽取的 U8 API 中文分类、序号、地址和示例文件索引 |
| [u8-bridge-official-api-map.md](./u8-bridge-official-api-map.md) | Bridge REST 路径与官方 U8 API 地址、中文分类、接入状态对照 |
| [u8-db-schema/README.md](./u8-db-schema/README.md) | `ufdata_001_2018` 账套库表结构导出、字段描述统计和排查建议 |
| [u8-db-samples/README.md](./u8-db-samples/README.md) | `ufdata_001_2018` 候选业务表脱敏样本、字段画像和关联验证 |

## 维护原则

* API 字段变更必须同时更新 Markdown 文档和 OpenAPI 合同。
* 新增 U8 单据接口时，必须补充字段映射和错误码口径。
* 不在文档中记录任何真实登录凭据、数据库凭据或 API Key。
* 联调时的真实环境值放在部署配置或受控交接文档中，不提交到仓库。

当前官方模式已接入 `U8Login.clsLogin` 与 `U8ApiBroker`，对外统一通过 Bridge REST API 对接。业务系统不再直连 U8 数据库；客户、物料、供应商、库存、采购在途和物料价格查询由 Bridge 持有只读库配置后统一封装。

Swagger 已生成所有官方唯一 API 地址的通用入口：`POST /api/u8/official/{官方地址}`。强类型业务接口继续保留原路径；尚未沉淀字段模板的官方接口可先通过通用入口传 `normalValues`、`contextValues`、`businessObjects`、`extensionObjects` 调用，现场联调稳定后再提升为强类型业务接口。
