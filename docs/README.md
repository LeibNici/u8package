# U8 Bridge 资料索引

本目录保存 U8 Bridge 的设计资料和对外接口合同。

## 文档列表

| 文档 | 用途 |
| --- | --- |
| [u8-bridge-api.md](./u8-bridge-api.md) | RESTful API 对接说明，面向 Java 后端和联调人员 |
| [u8-bridge-openapi.yaml](./u8-bridge-openapi.yaml) | OpenAPI 3.0 合同，后续用于生成 Swagger/Apifox/Postman |
| [u8-bridge-field-mapping.md](./u8-bridge-field-mapping.md) | 业务 JSON 字段到 U8 `domHead` / `domBody` 字段映射 |
| [u8-bridge-error-codes.md](./u8-bridge-error-codes.md) | 标准错误码、HTTP 状态、重试和日志约定 |

## 维护原则

* API 字段变更必须同时更新 Markdown 文档和 OpenAPI 合同。
* 新增 U8 单据接口时，必须补充字段映射和错误码口径。
* 不在文档中记录任何真实登录凭据、数据库凭据或 API Key。
* 联调时的真实环境值放在部署配置或受控交接文档中，不提交到仓库。

当前官方模式已接入 `U8Login.clsLogin` 与 `U8ApiBroker`，支持销售订单、销售发货单、销售出库单、材料出库单的新增/审核首轮联调。
