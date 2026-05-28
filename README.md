# Xinchuan U8 Bridge

## 定位

`xinchuan-u8-bridge` 是信川系统与用友 U8 官方 API 之间的独立桥接服务工作区，目录层级与 `xinchuan-backend`、`xinchuan-frontend` 同级管理。

桥接服务部署在客户 U8 Windows 环境中，对信川 Java 后端暴露 RESTful HTTP JSON API，内部调用 U8 官方 `U8Login`、`U8ApiBroker` 和 U8API DLL 完成单据新增、审核等写入动作。

## 为什么独立成目录

* U8 官方 API 依赖 Windows、U8 客户端或服务端组件、COM/DLL 运行环境，不适合直接放进 Java 后端进程。
* 桥接服务有独立部署生命周期，正式环境通常是 Windows Server。
* 桥接 API 合同、字段映射、错误码、部署资料需要和实现代码一起管理，方便客户 IT、U8 顾问、后端开发共同联调。

## 建议技术栈

首选 C# / .NET Framework 4.8。

原因：

* 当前官方示例是 C# 风格，围绕 U8 DLL 调用。
* 更容易处理 COM、x86/x64、IIS/Windows Service 等 U8 本地运行约束。
* 对 Java 后端只暴露 HTTP JSON，后端不感知 DLL 细节。

## 目录规划

```text
xinchuan-u8-bridge/
  README.md
  docs/
    README.md
    u8-bridge-api.md
    u8-bridge-openapi.yaml
    u8-bridge-field-mapping.md
    u8-bridge-error-codes.md
  vendor/
    U8APIFramework/
      # 官方 U8API Framework 目录，打包时复制到 release 根目录
  src/
    Xinchuan.U8Bridge.sln
    Xinchuan.U8Bridge/
      # .NET Framework 4.8 OWIN SelfHost Bridge 源码
  tests/
    # 后续放单元测试、接口测试、联调脚本
  deploy/
    windows/
      build.ps1
      appsettings.prod.example.json
```

当前首版已补齐 REST 服务边界、配置、安全、标准响应、U8 登录和 U8ApiBroker 写单据/审核调用。真实账套联调时，若 U8 返回字段必填、档案不存在或单据类型不匹配，需要按 `rawMessage` 与客户 U8 单据模板继续补字段。

## 对接边界

* 读写数据：统一由 Java 后端调用本 Bridge REST API，不再让业务系统直连 U8 SQL Server。
* Bridge 写入/审核底层接 U8 官方 API / U8 客户端组件；读类接口可由 Bridge 持有只读数据库配置统一封装，业务系统不感知表结构。
* 缺官方示例的剩余写入接口先返回 `U8_API_NOT_SUPPORTED`，不在业务系统侧猜 U8 表结构。
* 登录参数：Bridge 使用配置化 U8 Profile，`server` 值应来自 U8 客户端登录界面的服务器/数据源下拉框，并通过 `login-test` 接口验证。
* 对外接口：Bridge 提供 `GET /openapi.yaml` 与 `GET /swagger`，后端按文档调用。

## 当前资料

* [REST API 对接文档](./docs/u8-bridge-api.md)
* [OpenAPI/Swagger 合同](./docs/u8-bridge-openapi.yaml)
* [字段映射](./docs/u8-bridge-field-mapping.md)
* [错误码与联调约定](./docs/u8-bridge-error-codes.md)

## 后续实施入口

首版开发已补齐：

* `src/`：Bridge REST 服务骨架。
* `src/Xinchuan.U8Bridge.Manager/`：Windows 图形化配置、启动、测试和日志查看工具。
* `deploy/windows/`：Windows 构建脚本和生产配置模板。
* 统一 Bridge API 契约，覆盖清单中的读写接口；读类接口已封装只读数据库查询，缺 U8 官方示例的剩余写入接口有稳定占位返回。
* `tests/`：`login-test`、销售订单新增/审核、出库新增/审核等接口测试。

## 本地/Windows 构建

本服务目标运行环境是 Windows Server + .NET Framework 4.8。

在 Windows 开发机或 U8 客户端机器上：

```powershell
cd xinchuan-u8-bridge\deploy\windows
.\build.ps1
```

构建依赖：

* Visual Studio Build Tools 或 MSBuild。
* NuGet CLI。若本机没有，`build.ps1` 会自动下载到 `deploy/windows/.tools/nuget.exe`。
* .NET Framework 4.8 Developer Pack。

建议在开发/打包用 Windows 机器上执行构建，然后把 `bin/Release` 打成 zip 交付到客户 U8 机器。客户 U8 机器只负责运行 Bridge，不要求安装 NuGet 或 Visual Studio Build Tools。

## GitHub Actions 打包

推荐把 Bridge 代码推到 `LeibNici/u8package`，通过 GitHub Actions 在 `windows-latest` 上打包。仓库可以使用两种结构：

```text
# 结构 A：独立仓库根目录就是 Bridge
src/Xinchuan.U8Bridge.sln
deploy/windows/build.ps1
docs/

# 结构 B：保持当前项目子目录
xinchuan-u8-bridge/src/Xinchuan.U8Bridge.sln
xinchuan-u8-bridge/deploy/windows/build.ps1
xinchuan-u8-bridge/docs/
```

推送 `.github/workflows/u8-bridge-package.yml` 后，在 GitHub 页面进入：

```text
Actions -> Package U8 Bridge -> Run workflow
```

运行完成后，在 workflow run 页面下载 artifact：

```text
Xinchuan.U8Bridge-Release-<run_number>.zip
```

这个 zip 才是交给客户 U8 机器解压运行的包；客户机器不需要 NuGet、MSBuild 或 Visual Studio Build Tools。
Release 构建会同时发布 GitHub Release，管理器默认通过 GitHub Release latest API 检测更新。

当前 Actions 固定使用 `windows-2022`，避免 `windows-latest` 迁移期间影响 .NET Framework 4.8 打包稳定性。
如果仓库存在 `vendor/U8APIFramework/`，Actions 会把完整官方 U8API Framework 目录复制到 release 包根目录，Bridge 会优先从 `程序目录\U8APIFramework` 加载 `UFIDA.U8.U8APIFramework.dll` 等依赖。

## Windows 运行与日志

推荐现场先打开图形界面：

```powershell
.\Xinchuan.U8Bridge.Manager.exe
```

管理器可完成：

* 填写并保存 `appsettings.json`。
* 一键配置 Windows `HttpListener` 监听权限。
* 启动/停止 `Xinchuan.U8Bridge.exe`；停止时会清理已有 Bridge 进程，避免 8081 端口残留。
* 测试 `/health` 和 `/api/u8/login-test`。
* 检查新版本；默认支持 GitHub Release，也可改成自定义 JSON manifest 地址。
* 查看并打开 `logs` 日志目录。
* 关闭窗口时驻留到任务栏通知区域，右键托盘图标可打开、停止服务或退出。

管理器行为：

* 最小化：保持普通最小化。
* 点击窗口关闭：隐藏到任务栏通知区域，不停止 Bridge。
* 托盘图标双击：恢复管理器窗口。
* 托盘右键退出：停止 Bridge 并退出管理器。

如果启动日志出现 `System.Net.HttpListenerException: 拒绝访问`，说明当前 Windows 用户没有监听
`baseUrl` 的 URL ACL 权限。先在管理器里点击“配置监听权限”，在弹出的管理员授权窗口中确认，
再点击“启动服务”。等价手工命令如下：

```powershell
netsh http add urlacl url="http://+:8081/" sddl="D:(A;;GX;;;WD)"
```

如果启动日志出现 `OwinServerFactory.cs:100`、`侦听失败` 或 8081 未释放，通常是旧的
`Xinchuan.U8Bridge.exe` 还在运行。先在管理器里点击“停止服务”，再启动；手工兜底命令：

```powershell
taskkill /F /IM Xinchuan.U8Bridge.exe
netstat -ano | findstr :8081
```

解压 artifact 后，先复制一份配置：

```powershell
copy .\appsettings.sample.json .\appsettings.json
```

然后修改 `appsettings.json`：

* `apiKey`：信川后端调用 Bridge 时传入的 `X-API-KEY`。
* `u8Mode`：正式联调用 `official`，只验证 REST 服务启动可用时用 `dryRun`。
* `database.server`、`database.database`、`database.user`、`database.password`：U8 只读 SQL Server 连接信息，统一由 Bridge 持有，Java 后端不直接连库。
* `server`：U8 登录界面服务器/数据源下拉框中选中的值。
* `userId`、`password`、`loginDate`：按客户 U8 登录信息填写。
* `update.sourceType`：更新检测来源，`githubRelease` 或 `manifest`。
* `update.checkUrl`：更新检测地址。GitHub Release 可填 `https://api.github.com/repos/<owner>/<repo>/releases/latest`；自定义地址返回 JSON manifest。
* `update.githubProxyPrefix`：GitHub 加速代理前缀。国内现场可填 `https://v4.gh-proxy.org/`；留空则保持直连。

`subId`、`accountId`、`year`、`serial` 不需要现场用户配置。Bridge 内部默认 `subId=AS`、`serial` 为空，并会从库名
`ufdata_100_2018` 推导账套 `100` 与年度 `2018`；如果客户库名不符合该格式，再按现场 U8 登录失败日志补充兼容逻辑。
DLL 目录也不需要配置，release 包已随带 `U8APIFramework`，运行时会优先从程序目录加载，再回退到 U8 标准安装目录。

`official` 模式需要满足：

* 在安装了 U8 客户端或 U8 API 组件的 Windows 机器运行。
* `U8Login.clsLogin` 已注册。
* Bridge 以 x86 进程运行，以兼容常见 32 位 U8 COM 组件。
* `server` 必须填写 U8 登录界面服务器/数据源下拉框的原始值，不能只凭数据库名猜。
* `database.database` 应填写目标账套库名，例如 `ufdata_100_2018`，用于推导 U8 登录账套与年度。
* `userId`、`password`、`loginDate` 与 U8 客户端可登录信息一致。
* 先用管理器“测试 U8 登录”，返回 `U8 登录成功` 后再做写单据联调。
* 写单据会通过 `U8ApiBroker` 调用 `SaleOrder/Save`、`Consignment/Save`、`saleout/Add`、`MaterialOut/Add` 以及对应审核 API；若 U8 返回字段/档案错误，按 `rawMessage` 补齐字段或基础档案。

正式启动：

```powershell
.\Xinchuan.U8Bridge.exe .\appsettings.json
```

只验证服务能启动时，可以先执行：

```powershell
copy .\appsettings.dryrun.example.json .\appsettings.dryrun.json
.\Xinchuan.U8Bridge.exe .\appsettings.dryrun.json
```

运行日志默认写入：

```text
logs\u8-bridge-yyyyMMdd.log
```

如果需要改日志目录，可设置环境变量 `U8_BRIDGE_LOG_DIR`。启动失败、配置缺失、JSON 格式错误、U8 API 调用异常都会写入日志。

## 更新检测

管理器“检查更新”只做检测和提示，不会静默替换正在运行的程序。

推荐把正式交付包发布成 GitHub Release，或由内网文件服务提供 manifest。GitHub Actions artifact
更适合临时下载和联调，通常有过期时间，不建议作为现场长期自动更新源。

默认 GitHub Release 配置：

```json
{
  "configSchemaVersion": 1,
  "update": {
    "enabled": true,
    "sourceType": "githubRelease",
    "checkUrl": "https://api.github.com/repos/LeibNici/u8package/releases/latest",
    "githubProxyPrefix": "https://v4.gh-proxy.org/",
    "includePrerelease": false
  }
}
```

在管理器 GUI 中，`检测地址` 保持填写原始 GitHub API，例如
`https://api.github.com/repos/<owner>/<repo>/releases/latest`；`GitHub加速` 填
`https://v4.gh-proxy.org/`。管理器会在实际检测和下载时临时拼成
`https://v4.gh-proxy.org/https://api.github.com/repos/...` 或
`https://v4.gh-proxy.org/https://github.com/...`，配置文件仍保存原始 `checkUrl`
与单独的 `githubProxyPrefix`。

如果不用 GitHub Release，可以把 `sourceType` 改成 `manifest`，`checkUrl` 指向自定义 JSON：

```json
{
  "version": "22",
  "configSchemaVersion": 1,
  "minConfigSchemaVersion": 1,
  "releaseUrl": "https://example.com/u8-bridge/releases/22",
  "downloadUrl": "https://example.com/u8-bridge/Xinchuan.U8Bridge-Release-22.zip",
  "notes": "更新说明"
}
```

Release 包会随带 `package-version.json`，管理器只从该文件读取当前版本；若旧包缺少有效版本号，会回退到程序 Assembly 版本。
当前版本是只读运行信息，不允许通过 `appsettings.json` 改写。

配置大版本变更时，不建议直接静默升级。Bridge 配置使用 `configSchemaVersion` 标识结构版本；
manifest 或 GitHub Release notes 可声明 `ConfigSchemaVersion: 2` 与 `MinConfigSchemaVersion: 1`。
管理器检测更新时会提示配置是否兼容、是否需要迁移；真正替换程序前应先备份 `appsettings.json`。

检测到兼容的新版本后，“更新升级”按钮会变为可点击。点击后管理器会：

* 下载检测结果中的 zip 包到 `updates` 目录。
* 生成并启动本地升级脚本。
* 停止当前 Bridge 进程和管理器进程。
* 备份当前程序目录到 `backup-before-update-yyyyMMddHHmmss`。
* 解压新包并覆盖程序文件，但保留现场 `appsettings.json`。
* 升级完成后重新打开 `Xinchuan.U8Bridge.Manager.exe`。

如果升级脚本失败，窗口会停在命令行并显示备份目录，方便现场手工恢复。

## 当前实现状态

已实现：

* `GET /health`
* `GET /openapi.yaml`
* `GET /swagger`
* `POST /api/u8/login-test`
* `POST /api/u8/customers/query`
* `POST /api/u8/materials/query`
* `POST /api/u8/suppliers/query`
* `POST /api/u8/inventory/query`
* `POST /api/u8/in-transit/query`
* `POST /api/u8/material-price/query`
* `POST /api/u8/sales-order/save`
* `POST /api/u8/sales-order/audit`
* `POST /api/u8/consignment/save`
* `POST /api/u8/consignment/audit`
* `POST /api/u8/saleout/add`
* `POST /api/u8/saleout/audit`
* `POST /api/u8/material-out/add`
* `POST /api/u8/material-out/audit`
* `POST /api/u8/material-out/cancel-audit`
* `POST /api/u8/material-out/delete`
* `POST /api/u8/material-app/add`
* `POST /api/u8/material-app/audit`
* `POST /api/u8/morder/add`
* `POST /api/u8/morder/update`
* `POST /api/u8/morder/audit`
* `POST /api/u8/morder/unaudit`
* `POST /api/u8/morder/delete`
* `POST /api/u8/morder/load`
* `POST /api/u8/purchase-order/confirm`
* `POST /api/u8/purchase-order/cancel-confirm`
* `POST /api/u8/inbound/add`
* `POST /api/u8/production-plan/publish`
* `POST /api/u8/work-report/save`
* `POST /api/u8/material-return/add`

当前 U8 官方适配层已实现 `login-test` 的 `U8Login.clsLogin` COM 调用，并已接入部分 `U8ApiBroker` 写单据/审核调用。客户、物料、供应商、库存、采购在途和物料价格查询已通过 Bridge 持有的只读数据库配置封装为统一 REST API；缺少官方示例的剩余写入接口会返回 `U8_API_NOT_SUPPORTED`，待拿到示例后按同一 REST 契约补真实实现。
