# Deploy

U8 Bridge Windows Server 部署资料放在本目录。

建议包含：

* Windows Service 或 IIS 部署步骤。
* x86/x64 运行模式说明。
* 配置文件模板。
* 防火墙和内网访问白名单。
* 日志路径与轮转策略。
* OpenAPI/Swagger 暴露地址。

当前已提供 Windows 构建入口：

* [windows/build.ps1](./windows/build.ps1)
* [windows/appsettings.prod.example.json](./windows/appsettings.prod.example.json)

后续接入 U8 官方 DLL 后，再补充 Windows Service 安装脚本。
