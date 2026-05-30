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
* `python3 tests/check-u8-runtime-contract.py`
* 检查 Controller 路由统一挂在 `/api/u8` 前缀下。
* 检查 OpenAPI 不再生成 `/api/u8/official/U8API/...` 主路径。
* 检查官方目录生成的迁移 alias 与 catalog 扣除强类型覆盖后一致，例如 `/api/u8/ap-apply-pay/cancel-sign`。
* 检查生成接口不再逐条重复 legacy/internal compatibility 说明，Swagger 只展示迁移后的业务化路径。
* 检查 legacy generic official 入口 `/api/u8/official/{官方地址}` 保留兼容但标记为 internal。
* 检查 OpenAPI 继续推荐强类型业务路径，例如 `/api/u8/sales-order/save`。
* 检查 Bridge 运行环境固定程序目录、`TEMP`/`TMP` 和内置 U8APIFramework 相关编译项。

Mapper 快照检查：

* Windows/MSBuild 环境构建后运行 `Xinchuan.U8Bridge.exe --assert-sales-order-mapper`。
* 该检查构造销售订单 `U8BrokerCall`，不登录 U8、不调用 COM broker，用于保护 Release 45 已验证字段形态。
* Release 46 起，该检查还会初始化 Bridge 运行环境，确认进程当前目录固定为程序目录，并确认 `TEMP`/`TMP` 指向程序目录下的 `temp` 子目录。
* 当前断言覆盖 `VoucherType=12`、`ReturnIdName=vNewID`、`DomConfig`、`domHead.ivtid=131507`、`domHead.iexchrate=1`、`domBody.cunitid=28`、`cgroupcode=1`、`kl/kl2=100`、`bsaleprice=true`、`bgift=false`。

Release 46 拒绝访问排查记录：

* `/api/u8/sales-order/save` 已进入 `U8API/SaleOrder/Save`，失败点固定在 `MSXML2.DOMDocumentClass.save -> BOToRsConverter.ConvertToRSDOM -> U8ApiBroker.Invoke`。
* 根因假设是 U8APIFramework/MSXML 在序列化临时 DOM 时使用了进程当前目录或系统临时目录；Bridge 服务态与最小 exe 的启动目录/临时目录不同，导致写入不可访问位置。
* Bridge 启动时会创建程序目录下的 `temp`，并把进程 `TEMP`/`TMP` 指向该目录；每次 U8 broker 调用期间会临时把 `Environment.CurrentDirectory` 切回程序目录，调用结束后恢复。
* Windows 200 机器验证：替换 Release 后先运行 `/health` 和 `/api/u8/login-test`，再调用 `/api/u8/sales-order/save`；若仍失败，查看 Bridge 日志中是否有 runtime environment 初始化失败或 current directory setup failed。

后续接入 U8 官方 DLL 后，再补充 Windows 环境的真实 U8 联调用例。
