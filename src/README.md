# Source

U8 Bridge 源码放在本目录。

当前规划建议：

* C# / .NET Framework 4.8。
* REST API 层只暴露 JSON 合同。
* U8 调用层封装 `U8Login`、`U8ApiBroker` 和各 U8API 地址。
* 配置层管理 U8 登录 Profile、API Key、日志路径和幂等存储。

当前首版代码位于：

* [Xinchuan.U8Bridge.sln](./Xinchuan.U8Bridge.sln)
* [Xinchuan.U8Bridge/](./Xinchuan.U8Bridge/)

真实 U8 DLL 绑定集中在 `Xinchuan.U8Bridge/U8/OfficialU8ApiClient.cs`，REST 层不直接依赖 U8 DLL。
