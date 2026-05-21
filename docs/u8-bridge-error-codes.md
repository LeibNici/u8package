# U8 Bridge 错误码与联调约定

## 1. 目标

本文定义 U8 Bridge 对外返回的标准错误码、HTTP 状态建议、日志要求和联调排查口径。

Bridge 必须把 U8 官方 API 的系统异常、业务异常、登录错误、幂等冲突转换为稳定的错误码，便于信川 Java 后端记录 `api_log`、进入重试队列或提示人工处理。

## 2. HTTP 状态建议

| 场景 | HTTP 状态 | 说明 |
| --- | --- | --- |
| 调用成功 | `200` | `success=true` |
| 请求字段不合法 | `400` | 缺字段、格式错误、明细为空 |
| 鉴权失败 | `401` | API Key 缺失或错误 |
| 权限不足 | `403` | 来源 IP 不允许或 Profile 禁用 |
| 幂等冲突 | `409` | 同一业务单号请求内容不一致 |
| U8 业务错误 | `422` | U8 返回客户/物料/库存等业务校验失败 |
| U8 不可用 | `503` | 登录失败、U8 组件不可用、账套不可达 |
| Bridge 内部异常 | `500` | 未归类异常 |

不论 HTTP 状态如何，响应体都使用统一 JSON。

## 3. 统一响应

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

## 4. 标准错误码

| 错误码 | HTTP | 是否可重试 | 说明 |
| --- | --- | --- | --- |
| `AUTH_MISSING` | 401 | 否 | 缺少 `X-API-KEY` |
| `AUTH_INVALID` | 401 | 否 | `X-API-KEY` 不正确 |
| `SOURCE_FORBIDDEN` | 403 | 否 | 来源 IP 或调用方不在白名单 |
| `REQUEST_INVALID` | 400 | 否 | 请求 JSON 格式或字段校验失败 |
| `REQUEST_ID_MISMATCH` | 400 | 否 | Header 与 Body 中的 requestId 不一致 |
| `LOGIN_PROFILE_NOT_FOUND` | 400 | 否 | 指定 U8 登录 Profile 不存在 |
| `U8_LOGIN_FAILED` | 503 | 可人工重试 | `U8Login.Login` 返回失败 |
| `U8_COMPONENT_UNAVAILABLE` | 503 | 可重试 | U8 DLL、COM、MSXML、注册组件不可用 |
| `U8_DATABASE_ERROR` | 503 | 视情况 | U8 只读数据库未启用、连接失败或表字段不匹配 |
| `U8_API_NOT_SUPPORTED` | 400 | 否 | Bridge 未启用该 U8 API |
| `U8_SYS_ERROR` | 500 | 视情况 | U8 `MomSysException` 或系统级异常 |
| `U8_BIZ_ERROR` | 422 | 否 | U8 `MomBizException` 或业务校验失败 |
| `U8_RESULT_FAILED` | 422 | 否 | `broker.Invoke()` 成功但返回值表示失败 |
| `U8_RESPONSE_PARSE_ERROR` | 500 | 视情况 | Bridge 无法解析 U8 返回参数 |
| `IDEMPOTENCY_CONFLICT` | 409 | 否 | 同一业务单号重复请求但内容不同 |
| `IDEMPOTENCY_RECORD_LOCKED` | 409 | 可重试 | 同一业务单号正在处理中 |
| `BRIDGE_TIMEOUT` | 503 | 可重试 | Bridge 调 U8 超时 |
| `BRIDGE_INTERNAL_ERROR` | 500 | 视情况 | Bridge 未分类异常 |

## 5. 重试建议

### 可自动重试

* `U8_COMPONENT_UNAVAILABLE`
* `IDEMPOTENCY_RECORD_LOCKED`
* `BRIDGE_TIMEOUT`
* 部分 `U8_SYS_ERROR`

### 不建议自动重试

* `REQUEST_INVALID`
* `AUTH_INVALID`
* `U8_BIZ_ERROR`
* `IDEMPOTENCY_CONFLICT`

### 需要人工处理

* `U8_LOGIN_FAILED`
* 客户编码不存在
* 存货编码不存在
* 仓库编码不存在
* 可用量不足
* 单据已审核不可修改
* U8 账套年度或操作日期不允许

## 6. 日志要求

Bridge 每次调用至少记录：

| 字段 | 说明 |
| --- | --- |
| `requestId` | 全局请求 ID |
| `apiPath` | Bridge REST 路径 |
| `u8ApiAddress` | 内部 U8 API 地址 |
| `businessKey` | 幂等业务键，如 `orderNo` |
| `profileName` | U8 登录 Profile |
| `requestHash` | 请求体摘要，用于幂等冲突判断 |
| `success` | 是否成功 |
| `u8Code` | U8 单据号 |
| `u8Id` | U8 单据 ID |
| `errorCode` | 标准错误码 |
| `rawMessage` | 脱敏后的 U8 原始错误 |
| `durationMs` | 调用耗时 |
| `createdAt` | 创建时间 |

敏感信息禁止记录：

* U8 密码
* API Key
* 数据库密码
* Windows 登录密码

## 7. U8 异常转换规则

Bridge 内部调用 `broker.Invoke()` 后按以下顺序处理：

1. 如果 `U8Login.Login` 失败，返回 `U8_LOGIN_FAILED`。
2. 如果 U8 DLL / COM 初始化失败，返回 `U8_COMPONENT_UNAVAILABLE`。
3. 如果 `broker.Invoke()` 返回 `false`：
   * `broker.GetException()` 是 `MomBizException`，返回 `U8_BIZ_ERROR`。
   * `broker.GetException()` 是 `MomSysException`，返回 `U8_SYS_ERROR`。
   * 其他异常返回 `BRIDGE_INTERNAL_ERROR`。
   * `rawMessage` 优先取 `broker.GetExceptionString()`。
4. 如果 `broker.Invoke()` 返回 `true`，但 `GetReturnValue()` 表示失败，返回 `U8_RESULT_FAILED`。
5. 如果读取 `GetResult("vNewID")`、`GetResult("VouchId")`、`GetResult("errMsg")` 出错，返回 `U8_RESPONSE_PARSE_ERROR`。

## 8. 幂等错误处理

Bridge 必须保存每个业务单号的请求摘要。

同一业务单号再次请求：

* 请求摘要一致且之前成功：返回之前成功结果。
* 请求摘要一致且之前失败：允许再次调用 U8，更新重试次数。
* 请求摘要不同：返回 `IDEMPOTENCY_CONFLICT`。
* 之前请求仍在处理中：返回 `IDEMPOTENCY_RECORD_LOCKED`。

## 9. Java 后端处理建议

Java 后端收到 Bridge 响应后：

* `success=true`：记录 `api_log` 成功，保存 U8 单号/ID。
* `success=false` 且可重试：写入重试队列。
* `success=false` 且不可重试：记录失败明细，按业务规则回滚或提示人工处理。
* 订单确认类接口若要求强一致，P0 阶段可保持失败回滚；若后续改为异步外部同步，需要单独评审。

## 10. 联调检查清单

### 10.1 Bridge 环境

* Windows Server 可手工登录 U8 客户端。
* Bridge 进程位数与 U8 DLL 位数一致。
* `login-test` 成功。
* Bridge 不输出明文密码。
* Bridge 只允许内网访问。

### 10.2 销售订单

* 客户编码存在。
* 存货编码存在。
* 销售部门编码存在。
* 销售类型编码存在。
* 币种、税率符合 U8 账套配置。
* 单行、多行明细都可创建。
* 重复请求同一订单不会重复生成 U8 单据。
* 审核开关行为符合预期。

### 10.3 销售出库 / 发货

* 已确认使用销售发货单、销售出库单，或两者都用。
* 仓库编码存在。
* 出库类别编码存在。
* 可用量不足时 Bridge 返回 `U8_BIZ_ERROR`。
* 自动审核策略符合客户要求。

### 10.4 材料出库

* 已确认材料出库单审核 API 地址。
* 生产领料对应的 U8 源单字段已由 U8 顾问确认。
* 批次、货位、自由项策略已确认。

## 11. 上线前阻断项

以下任一项未完成，不允许上线写 U8：

* `login-test` 未在正式 Windows/U8 环境通过。
* P0 API 没有 OpenAPI / Markdown 文档。
* 幂等逻辑未实现。
* 错误码未稳定。
* 字段映射未经 U8 顾问或真实账套验证。
* 未明确销售发货与销售出库业务路径。
* 未配置访问控制和 API Key。
