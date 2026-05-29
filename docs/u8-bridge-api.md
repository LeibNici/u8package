# U8 Bridge REST API 对接文档

## 1. 目标

U8 Bridge 是部署在客户 U8 Windows 环境上的转接服务。信川 Java 后端通过 RESTful HTTP JSON 调用 Bridge，Bridge 内部调用 U8 官方 `U8Login` / `U8ApiBroker` / U8API DLL 完成 U8 单据新增、审核等操作。

本文定义 Bridge 对外 REST API 契约。Bridge 内部实现语言建议为 C# / .NET Framework 4.8，但对信川系统只暴露 HTTP JSON。

## 2. 总体架构

```text
信川 Java 后端
  -> HTTP JSON
      -> U8 Bridge REST 服务
          -> U8Login 登录账套
          -> U8ApiBroker 调官方 U8API
              -> U8 账套
```

## 3. 环境与部署约束

Bridge 必须部署在能正常运行 U8 官方 API 的 Windows 环境中。

最低要求：

* Windows Server 正式环境。
* 已安装 U8 客户端或 U8 服务端组件。
* 当前机器可通过 U8 客户端登录目标账套。
* U8 API 相关 DLL 可引用或已注册。
* 如 U8 组件为 32 位，Bridge 进程或 IIS 应用池必须启用 32 位。
* Bridge 运行账号具备访问 U8 组件、U8 安装目录、账套服务的权限。

## 4. 登录参数设计

Bridge 不应把 U8 登录参数写死在代码里，应通过配置文件或管理界面维护登录 Profile。

现场可见配置项：

| 字段 | U8Login 参数 | 说明 |
| --- | --- | --- |
| `profileName` | - | 环境名称，例如 `prod-100` |
| `server` | `sServer` | U8 登录界面“服务器/数据源”下拉框选中值，必须原样配置 |
| `userId` | `sUserID` | U8 操作员 |
| `password` | `sPassword` | U8 密码，必须加密或由密钥服务注入 |
| `loginDate` | `sDate` | U8 登录业务日期 |
| `database.server` | - | U8 只读 SQL Server 地址 |
| `database.database` | - | 账套库名，例如 `ufdata_100_2018` |
| `database.user` | - | U8 只读 SQL Server 用户 |
| `database.password` | - | U8 只读 SQL Server 密码 |

重要规则：

* `server` 必须来自 U8 客户端登录界面下拉框，不从 SQL Server JDBC URL 推导。
* `subId`、`accountId`、`year`、`serial` 不作为现场可见配置；Bridge 内部默认 `subId=AS`、`serial` 为空，并从库名 `ufdata_100_2018` 推断账套 `100` / 年度 `2018`。
* 如果客户库名不符合 `ufdata_<账套>_<年度>` 格式，最终以 `login-test` 的 U8 返回为准，再补兼容规则。
* 不允许在源码、文档、日志中输出明文密码。

## 5. 通用请求头

所有业务写接口必须携带：

```http
Content-Type: application/json
X-API-KEY: <bridge-api-key>
X-Request-ID: <global-request-id>
```

说明：

* `X-API-KEY` 用于 Bridge 入站鉴权。
* `X-Request-ID` 用于链路追踪。若请求体内也包含 `requestId`，两者必须一致，否则返回 `REQUEST_ID_MISMATCH`。

## 6. 通用响应格式

成功：

```json
{
  "success": true,
  "requestId": "OMS-ORDER-202605210001",
  "u8Code": "SO202605210001",
  "u8Id": "123456",
  "message": "销售订单新增成功",
  "errorCode": null,
  "rawMessage": null
}
```

失败：

```json
{
  "success": false,
  "requestId": "OMS-ORDER-202605210001",
  "u8Code": null,
  "u8Id": null,
  "message": "U8销售订单新增失败",
  "errorCode": "U8_BIZ_ERROR",
  "rawMessage": "客户编码不存在"
}
```

字段说明：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `success` | boolean | 是否成功 |
| `requestId` | string | 请求追踪 ID |
| `u8Code` | string | U8 单据号，若 U8 返回或可确定则填充 |
| `u8Id` | string | U8 单据主键 ID，例如 `vNewID` 或 `VouchId` |
| `message` | string | 面向调用方的摘要消息 |
| `errorCode` | string | 标准错误码，见 `u8-bridge-error-codes.md` |
| `rawMessage` | string | U8 原始错误或 `broker.GetExceptionString()`，需脱敏 |
| `data` | object | 查询类接口返回分页数据；写入/审核类接口通常为空 |

## 7. 幂等规则

Bridge 必须按业务单号做幂等，避免重复生成 U8 单据。

建议幂等键：

| 接口 | 幂等键 |
| --- | --- |
| 销售订单新增 | `orderNo` |
| 销售订单审核 | `orderNo` 或 `u8Id` |
| 销售发货单新增 | `deliveryNo` |
| 销售出库单新增 | `outboundNo` |
| 材料出库单新增 | `materialOutNo` |
| 领料申请单新增 | `applicationNo` |
| 生产订单新增 | `moCode` |
| 主数据、库存、价格查询 | 不做幂等，按 `requestId` 追踪 |

重复请求处理：

* 若之前成功，直接返回原 `u8Code` / `u8Id`。
* 若之前失败，允许重试，但必须记录 retry 次数。
* 若同一幂等键请求内容发生变化，返回 `IDEMPOTENCY_CONFLICT`。

## 8. API 清单

对外 REST 路径统一采用 `POST /api/u8/<business-resource>/<action>`。稳定业务流程必须优先使用强类型业务接口，
例如 `POST /api/u8/sales-order/save`、`POST /api/u8/material-out/add`、`POST /api/u8/bom/add`。
U8 官方地址 `U8API/...` 只作为内部映射信息记录在文档和 OpenAPI 扩展字段中，不作为新系统对接路径。

Swagger 按中文业务分类维护接口：系统、主数据查询、采购查询、销售管理、库存管理、生产制造-BOM、
生产制造-生产订单等。官方 C# 示例生成的 `/api/u8/official/...` 通用入口不再统一放入“官方 U8 API”，
而是优先使用 `x-u8-document` 作为 Swagger tag；为空时使用 `x-u8-category`；仍为空才兜底为
`官方U8 API`。这样付款申请单源管理器、应收单核日志等官方单据/分类会在 Swagger UI 左侧归到各自分组。
官方 C# 示例索引见 `u8-official-api-catalog.md`，Bridge REST 与官方地址对照见 `u8-bridge-official-api-map.md`。

本版本仍保留所有官方唯一 API 地址的通用入口，作为 deprecated/internal 兼容入口：

```http
POST /api/u8/official/{官方地址}
```

例如：

```http
POST /api/u8/official/U8API/APApplyPay/SaveVouch
```

通用入口直接对接 `U8ApiBroker`，请求体需按官方示例传入 `normalValues`、`contextValues`、
`businessObjects` 或 `extensionObjects`。它只用于旧调用兼容、内部排查或尚未沉淀 DTO 的临时验证；
新业务对接不得把 `/api/u8/official/U8API/...` 当作推荐路径，稳定后应提升为强类型
`/api/u8/<business-resource>/<action>` 接口。

通用入口请求体示例：

```json
{
  "requestId": "REQ-202605280001",
  "profileName": "default",
  "businessNo": "PAY-001",
  "documentType": "ap-apply-pay-save",
  "idempotent": true,
  "voucherType": 0,
  "returnIdName": "vNewID",
  "resultNames": ["vNewID"],
  "normalValues": {
    "DomConfig": { "special": "domDocument" }
  },
  "businessObjects": [
    {
      "name": "domHead",
      "rows": [
        {
          "ccode": "PAY-001"
        }
      ]
    }
  ]
}
```

### 8.1 健康检查

```http
GET /health
```

用途：检查 Bridge 进程是否存活，不调用 U8。

接口文档：

```http
GET /openapi.yaml
GET /swagger
```

`/openapi.yaml` 可导入 Swagger Editor、Apifox、Postman；`/swagger` 是 Bridge 自带的 Swagger UI 页面。

成功响应：

```json
{
  "success": true,
  "message": "OK"
}
```

### 8.2 U8 登录测试

```http
POST /api/u8/login-test
```

用途：验证当前 Bridge Profile 是否可以通过 `U8Login.Login` 登录 U8。

请求：

```json
{
  "requestId": "LOGIN-TEST-001",
  "profileName": "prod-100"
}
```

成功响应：

```json
{
  "success": true,
  "requestId": "LOGIN-TEST-001",
  "u8Code": null,
  "u8Id": null,
  "message": "U8登录成功",
  "errorCode": null,
  "rawMessage": null
}
```

失败响应中 `rawMessage` 应包含 `u8Login.ShareString` 或等效错误信息。

### 8.3 主数据、库存、价格查询

统一原则：业务系统只调用 Bridge，不再直接连接 U8 SQL Server。Bridge 持有 U8 只读数据库配置，把读类接口统一封装成 REST；写入和审核仍通过 U8 官方 API。

| 接口 | 用途 | 当前实现 |
| --- | --- | --- |
| `POST /api/u8/customers/query` | 客户主数据同步 | 读 `Customer` |
| `POST /api/u8/materials/query` | 物料主数据同步 | 读 `Inventory` |
| `POST /api/u8/suppliers/query` | 供应商主数据同步 | 读 `Vendor` |
| `POST /api/u8/inventory/query` | 库存现存量查询 | 读 `CurrentStock` |
| `POST /api/u8/in-transit/query` | 采购在途查询 | 读 `PO_Podetails` + `PO_Pomain` |
| `POST /api/u8/material-price/query` | 物料价格查询 | 读 `Inventory` 价格字段 |

### 8.4 BOM 官方 API

BOM 明细不走 U8 账套表直读。Bridge 只封装 U8 官方 API；为了让业务系统可以按存货编码调用，
`part-lookup` 仅只读查询 U8 `bas_part` / `bom_parent` / `bom_bom` 表头定位参数，
不读取 BOM 子件明细。

| 接口 | U8 API | 用途 |
| --- | --- | --- |
| `POST /api/u8/bom/add` | `U8API/BOM/BomAdd` | 新增物料清单 |
| `POST /api/u8/bom/load` | `U8API/BOM/BomLoad` | 查询物料清单 |
| `POST /api/u8/bom/part-lookup` | 只读参数查询 | 按存货编码查询 `partId`、`bomType`、`versionOrIdentCode` |
| `POST /api/u8/bom/load-by-code` | `U8API/BOM/BomLoad` | 先按存货编码查参数，再调用官方 API 查询物料清单 |
| `POST /api/u8/bom/tree-query` | 只读树查询 | 按存货编码展开 BOM 树，用于本地排查和对账 |
| `POST /api/u8/bom/audit` | `U8API/BOM/BomAuditing` | 审核物料清单 |
| `POST /api/u8/bom/unaudit` | `U8API/BOM/BomUnauditing` | 弃审物料清单 |
| `POST /api/u8/bom/delete` | `U8API/BOM/BomDelete` | 删除物料清单 |

`bom/load`、`bom/audit`、`bom/unaudit`、`bom/delete` 按 U8 官方参数传 `partId`、`bomType`、`versionOrIdentCode`；
不能用 APS 型号或 U8 存货编码替代 `partId`。如果只知道 U8 存货编码，先调用 `bom/part-lookup`，
或直接调用 `bom/load-by-code`。

通用主数据查询请求：

```json
{
  "requestId": "U8-MASTER-QUERY-001",
  "profileName": "prod-100",
  "updatedFrom": "2026-05-01T00:00:00",
  "updatedTo": "2026-05-21T23:59:59",
  "keyword": "",
  "pageNo": 1,
  "pageSize": 200
}
```

成功响应：

```json
{
  "success": true,
  "requestId": "U8-MASTER-QUERY-001",
  "u8Code": null,
  "u8Id": null,
  "message": "U8 客户主数据查询成功",
  "errorCode": null,
  "rawMessage": null,
  "data": {
    "pageNo": 1,
    "pageSize": 200,
    "hasMore": false,
    "items": [
      {
        "customerCode": "C001",
        "customerName": "示例客户"
      }
    ]
  }
}
```

如果数据库未启用、连接失败、现场表字段与当前适配口径不一致，返回 `U8_DATABASE_ERROR`，`rawMessage` 中记录实际原因供联调定位。

按编码查询 BOM 请求：

```json
{
  "requestId": "U8-BOM-LOAD-BY-CODE-001",
  "profileName": "prod-100",
  "materialCode": "01001001",
  "bomType": 1,
  "versionOrIdentCode": "10",
  "pageNo": 1,
  "pageSize": 20
}
```

`bom/part-lookup` 返回 `data.items[]`，字段包括：`partId`、`materialCode`、`materialName`、
`specification`、`bomId`、`bomType`、`versionOrIdentCode`、`versionEffDate`、`bomState`。
`bom/load-by-code` 返回结构与 `bom/load` 一致，BOM 明细来自 U8 官方 `BomLoad`。

### 8.4 销售订单新增

```http
POST /api/u8/sales-order/save
```

内部 U8 API：`U8API/SaleOrder/Save`

用途：OMS 订单确认后，在 U8 创建或更新销售订单。

请求：

```json
{
  "requestId": "OMS-ORDER-202605210001",
  "orderNo": "SO202605210001",
  "orderDate": "2026-05-21",
  "customerCode": "C001",
  "customerName": "某客户",
  "departmentCode": "01",
  "departmentName": "销售部",
  "salesTypeCode": "01",
  "salesTypeName": "普通销售",
  "maker": "168",
  "currency": "人民币",
  "taxRate": 13,
  "autoAudit": false,
  "memo": "OMS订单确认同步",
  "items": [
    {
      "lineNo": 1,
      "materialCode": "M001",
      "materialName": "设备A",
      "specification": "XC-001",
      "quantity": 2,
      "unit": "台",
      "deliveryDate": "2026-06-01",
      "taxUnitPrice": 10000,
      "taxAmount": 20000
    }
  ]
}
```

成功响应：

```json
{
  "success": true,
  "requestId": "OMS-ORDER-202605210001",
  "u8Code": "SO202605210001",
  "u8Id": "1000123",
  "message": "销售订单新增成功",
  "errorCode": null,
  "rawMessage": null
}
```

### 8.5 销售订单审核

```http
POST /api/u8/sales-order/audit
```

内部 U8 API：`U8API/SaleOrder/Audit`

请求：

```json
{
  "requestId": "OMS-ORDER-AUDIT-202605210001",
  "orderNo": "SO202605210001",
  "u8Id": "1000123",
  "verify": true,
  "verifier": "168"
}
```

说明：

* `verify=true` 表示审核。
* `verify=false` 表示弃审，是否允许由业务和 U8 权限决定。

### 8.6 销售发货单新增

```http
POST /api/u8/consignment/save
```

内部 U8 API：`U8API/Consignment/Save`

用途：如果客户 U8 流程要求先生成销售发货单，则 WMS 发货前或发货时调用此接口。

请求字段与销售出库相近，但 U8 字段映射不同，详见 `u8-bridge-field-mapping.md`。是否首期启用需业务确认。

### 8.7 销售发货单审核

```http
POST /api/u8/consignment/audit
```

内部 U8 API：`U8API/Consignment/Audit`

### 8.8 销售出库单新增

```http
POST /api/u8/saleout/add
```

内部 U8 API：`U8API/saleout/Add`

用途：WMS 发货完成后，在 U8 创建销售出库单。

请求：

```json
{
  "requestId": "OMS-DELIVERY-202605210001",
  "outboundNo": "DO202605210001",
  "orderNo": "SO202605210001",
  "outboundDate": "2026-05-21",
  "customerCode": "C001",
  "customerName": "某客户",
  "warehouseCode": "01",
  "warehouseName": "成品库",
  "departmentCode": "01",
  "departmentName": "销售部",
  "maker": "168",
  "autoAudit": false,
  "memo": "WMS发货完成同步",
  "items": [
    {
      "lineNo": 1,
      "materialCode": "M001",
      "materialName": "设备A",
      "quantity": 2,
      "unit": "台",
      "batchNo": "",
      "sourceOrderNo": "SO202605210001",
      "sourceLineNo": 1
    }
  ]
}
```

### 8.9 销售出库单审核

```http
POST /api/u8/saleout/audit
```

内部 U8 API：`U8API/saleout/Audit`

请求：

```json
{
  "requestId": "OMS-SALEOUT-AUDIT-202605210001",
  "outboundNo": "DO202605210001",
  "u8Id": "2000123",
  "verify": true,
  "verifier": "168",
  "checkStock": true
}
```

### 8.10 材料出库单新增

```http
POST /api/u8/material-out/add
```

内部 U8 API：`U8API/MaterialOut/Add`

用途：生产领料实际出库后，在 U8 创建材料出库单。

请求：

```json
{
  "requestId": "WMS-MATERIAL-OUT-202605210001",
  "materialOutNo": "MO202605210001",
  "sourceNo": "WO202605210001",
  "outDate": "2026-05-21",
  "warehouseCode": "07",
  "warehouseName": "原材料库",
  "rdCode": "201",
  "rdName": "生产领料",
  "departmentCode": "02",
  "departmentName": "生产部",
  "maker": "168",
  "autoAudit": false,
  "memo": "生产领料同步",
  "items": [
    {
      "lineNo": 1,
      "materialCode": "RM001",
      "materialName": "原材料A",
      "quantity": 10,
      "unit": "件",
      "batchNo": "",
      "workOrderNo": "WO202605210001"
    }
  ]
}
```

### 8.11 材料出库单审核

```http
POST /api/u8/material-out/audit
```

内部 U8 API：`U8API/MaterialOut/Audit`

同类接口：

```http
POST /api/u8/material-out/cancel-audit
POST /api/u8/material-out/delete
```

内部 U8 API：

* `U8API/MaterialOut/CancelAudit`
* `U8API/MaterialOut/Delete`

### 8.12 领料申请单新增

```http
POST /api/u8/material-app/add
```

内部 U8 API：`U8API/materialapp/Add`

是否需要取决于客户 U8 流程。若 U8 要求“先申请、再材料出库”，此接口应在材料出库前调用。

### 8.13 领料申请单审核

```http
POST /api/u8/material-app/audit
```

内部 U8 API：`U8API/materialapp/Audit`

### 8.14 生产订单新增

```http
POST /api/u8/morder/add
```

内部 U8 API：`U8API/MOrder/MOrderAdd`

生产订单接口来自 `生产订单.txt` 这一类官方示例，新增/更新使用 `extbo`，包含表头、`Mom_OrderDetail`、`Mom_MoAllocate` 子件结构。

最小请求：

```json
{
  "requestId": "MO-ADD-202605210001",
  "orderNo": "MO202605210001",
  "maker": "168",
  "createDate": "2026-05-21",
  "items": [
    {
      "lineNo": 1,
      "materialCode": "FG001",
      "materialName": "成品A",
      "startDate": "2026-05-22",
      "dueDate": "2026-05-30",
      "quantity": 10,
      "orderClass": 1,
      "warehouseCode": "01",
      "departmentCode": "02",
      "components": [
        {
          "lineNo": 1,
          "materialCode": "RM001",
          "materialName": "原材料A",
          "baseQtyNumerator": 1,
          "baseQtyDenominator": 1,
          "quantity": 10,
          "requiredDate": "2026-05-22"
        }
      ]
    }
  ]
}
```

同类接口：

```http
POST /api/u8/morder/update
POST /api/u8/morder/load
POST /api/u8/morder/delete
```

内部 U8 API：

* `U8API/MOrder/MOrderUpdate`
* `U8API/MOrder/MOrderLoad`
* `U8API/MOrder/MOrderDelete`

### 8.15 生产订单审核

```http
POST /api/u8/morder/audit
```

内部 U8 API：`U8API/MOrder/MOrderAuditing`

弃审接口：

```http
POST /api/u8/morder/unaudit
```

内部 U8 API：`U8API/MOrder/MOrderUnauditing`

### 8.16 采购订单确认/弃审

```http
POST /api/u8/purchase-order/confirm
POST /api/u8/purchase-order/cancel-confirm
```

内部 U8 API：

* `U8API/PurchaseOrder/ConfirmPO`
* `U8API/PurchaseOrder/CancelconfirmPo`

说明：该接口来自 `采购订单.txt` 示例，文件名虽然是采购订单，但示例内容是采购订单确认/弃审。

### 8.17 入库单新增

```http
POST /api/u8/inbound/add
```

当前状态：稳定占位，返回 `U8_API_NOT_SUPPORTED`。

官方候选：`U8API/PuStoreIn/Add`（官方示例序号 `173`，采购入库单新增）。

缺少资料：客户现场入库到底使用采购入库、产成品入库还是其他入库；当前请求模型字段尚未完成
`PuStoreIn` 表头/表体映射，因此仍不启用写入。

### 8.18 生产计划发布

```http
POST /api/u8/production-plan/publish
```

当前状态：稳定占位，返回 `U8_API_NOT_SUPPORTED`。

缺少资料：生产计划发布到 U8 的官方接口路径、字段模板和业务触发时点。

### 8.19 生产报工回写

```http
POST /api/u8/work-report/save
```

当前状态：稳定占位，返回 `U8_API_NOT_SUPPORTED`。

官方候选：`U8API/PFReport/PFReportAdd`（官方示例序号 `266`，工序流转卡完工单新增）。

缺少资料：客户现场报工业务是否等同于工序流转卡完工单，字段模板和必填项尚未完成确认。

### 8.20 生产退料回写

```http
POST /api/u8/material-return/add
```

当前状态：稳定占位，返回 `U8_API_NOT_SUPPORTED`。

缺少资料：生产退料对应红字材料出库、退料申请或其他 U8 单据的官方 API 示例。

## 9. 联调范围建议

对外接口可以一次性按本文档接入，但真实联调建议分批：

1. `login-test`
2. 主数据查询接口：客户、物料、供应商
3. 销售链路：`sales-order/save`、`sales-order/audit`、`saleout/add` 或 `consignment/save`
4. 生产领料链路：`material-app/add`、`material-app/audit`、`material-out/add`
5. 生产订单新增/审核/弃审/更新/删除
6. 入库、报工、退料等需等官方示例补齐后再联调

## 10. Java 后端调用建议

Java 后端保留 `U8Adapter` 抽象，所有 U8 读写方法都通过 Bridge HTTP API 实现。

建议配置：

```yaml
xinchuan:
  integration:
    u8:
      enabled: true
      base-url: http://u8-bridge-host:8081
      headers:
        X-API-KEY: ${U8_BRIDGE_API_KEY}
```

系统不再保留 `xinchuan.u8.datasource.*` 这类 U8 数据库直连配置。即使底层未来由 Bridge 内部采用某种读取方式，对业务系统也只暴露本文档中的 REST API。

## 11. 安全要求

* Bridge 不在日志中输出 U8 密码、API Key、数据库密码。
* Bridge 不允许公网直接访问。
* 推荐只允许信川后端服务器 IP 访问。
* 所有失败日志保留 U8 原始错误，但必须脱敏。
* Bridge 配置文件里的密码必须加密或由系统环境变量注入。

## 12. 待确认事项

* U8 登录界面“服务器/数据源”下拉框的实际可登录值。
* 账套是否固定 `100`，年度是否固定 `2018`。
* OMS 订单确认后是否自动审核 U8 销售订单。
* WMS 发货后生成“销售发货单”“销售出库单”还是两者都生成。
* U8 部门、仓库、销售类型、出库类别、制单人、币种、税率等基础档案编码。
* 客户、物料、供应商、库存、采购在途、价格查询的官方 U8API 示例。
* 入库类单据是否使用 `U8API/PuStoreIn/Add`，以及采购入库/产成品入库/其他入库的业务边界。
* 生产计划是否有独立“发布”接口，或应拆分为预测订单 / 生产订单接口。
* 生产报工是否使用 `U8API/PFReport/PFReportAdd`，以及字段模板。
* 生产退料对应红字材料出库、退料申请或其他 U8 单据的官方 API 示例。
* 生产订单 `extbo` 中哪些字段为客户现场必填，尤其是生产部门、预入仓库、生产订单类别、子件用量。
