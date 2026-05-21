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

当前已经开始首版开发。第一版代码目标是先固定 REST 服务边界、配置、安全、标准响应和 U8 API 防腐层；真实 U8 DLL 绑定需要在 Windows/U8 环境继续完成。

## 对接边界

* 读数据：现阶段继续由 Java 后端通过 U8 SQL Server 只读账号查询客户、物料、供应商、库存、在途、价格等数据。
* 写数据：必须通过本 Bridge 调 U8 官方 API，不直接写 U8 数据库表。
* 登录参数：Bridge 使用配置化 U8 Profile，`server` 值应来自 U8 客户端登录界面的服务器/数据源下拉框，并通过 `login-test` 接口验证。
* 对外接口：Bridge 必须提供 OpenAPI/Swagger 文档，后端按文档调用。

## 当前资料

* [REST API 对接文档](./docs/u8-bridge-api.md)
* [OpenAPI 合同](./docs/u8-bridge-openapi.yaml)
* [字段映射](./docs/u8-bridge-field-mapping.md)
* [错误码与联调约定](./docs/u8-bridge-error-codes.md)

## 后续实施入口

首版开发已补齐：

* `src/`：Bridge REST 服务骨架。
* `deploy/windows/`：Windows 构建脚本和生产配置模板。
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

当前 Actions 固定使用 `windows-2022`，避免 `windows-latest` 迁移期间影响 .NET Framework 4.8 打包稳定性。

## 当前实现状态

已实现：

* `GET /health`
* `GET /openapi.yaml`
* `POST /api/u8/login-test`
* `POST /api/u8/sales-order/save`
* `POST /api/u8/sales-order/audit`
* `POST /api/u8/consignment/save`
* `POST /api/u8/consignment/audit`
* `POST /api/u8/saleout/add`
* `POST /api/u8/saleout/audit`
* `POST /api/u8/material-out/add`
* `POST /api/u8/material-out/audit`

当前 U8 官方适配层已先实现 `login-test` 的 `U8Login.clsLogin` COM 调用；写单据的 `U8ApiBroker` 字段映射绑定仍会返回 `U8_API_NOT_SUPPORTED`，防止误以为已经写入 U8。联调 REST 层可临时设置 `u8Mode` 为 `dryRun`，但生产必须使用 `official`。
