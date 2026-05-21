# Tests

U8 Bridge 测试资料放在本目录。

建议测试范围：

* `login-test` 验证 U8 登录 Profile。
* 销售订单新增与审核。
* 发货/出库新增与审核。
* 幂等重复提交。
* U8 业务错误、登录失败、网络超时、API Key 鉴权失败。

当前已有 HTTP 冒烟请求：

* [http/u8-bridge-smoke.http](./http/u8-bridge-smoke.http)

后续接入 U8 官方 DLL 后，再补充 Windows 环境的真实 U8 联调用例。
