# Tests

U8 Bridge 测试资料放在本目录。

建议测试范围：

* `login-test` 验证 U8 登录 Profile。
* 客户、物料、供应商、库存、采购在途、物料价格读库查询。
* 销售订单新增与审核。
* 发货/出库新增与审核。
* 幂等重复提交。
* U8 业务错误、登录失败、网络超时、API Key 鉴权失败。

当前已有 HTTP 冒烟请求：

* [http/u8-bridge-smoke.http](./http/u8-bridge-smoke.http)

静态合同检查：

* `python3 tests/check-u8-api-contract.py`
* 检查 Controller 路由统一挂在 `/api/u8` 前缀下。
* 检查 generic official 入口保留兼容但已标记 deprecated/internal。
* 检查 OpenAPI 继续推荐强类型业务路径，例如 `/api/u8/sales-order/save`。

Mapper 快照检查：

* Windows/MSBuild 环境构建后运行 `Xinchuan.U8Bridge.exe --assert-sales-order-mapper`。
* 该检查只构造销售订单 `U8BrokerCall`，不登录 U8、不调用 COM broker，用于保护 Release 45 已验证字段形态。
* 当前断言覆盖 `VoucherType=12`、`ReturnIdName=vNewID`、`DomConfig`、`domHead.ivtid=131507`、`domHead.iexchrate=1`、`domBody.cunitid=28`、`cgroupcode=1`、`kl/kl2=100`、`bsaleprice=true`、`bgift=false`。

后续接入 U8 官方 DLL 后，再补充 Windows 环境的真实 U8 联调用例。
