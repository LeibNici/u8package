# U8 Bridge 字段映射说明

## 1. 目标

本文定义信川业务 JSON 与 U8 官方 API `domHead` / `domBody` / 普通参数之间的初始映射。字段映射必须经过 U8 登录测试和真实账套验证后才能定版。

## 2. 通用规则

* 信川系统传业务语义字段，Bridge 负责转换为 U8 字段名。
* 信川业务系统不直接读取或写入 U8 数据库，所有 U8 能力统一通过 Bridge API 暴露。
* U8 基础档案编码必须传编码，不传显示名称作为主键。
* 名称字段可作为辅助字段传入，但不能替代编码字段。
* 单据明细必须支持多行。
* 日期格式统一为 `yyyy-MM-dd`。
* 金额、数量使用 decimal，不用浮点字符串。
* 字段为空时，Bridge 应按 U8API 要求传空字符串、空对象或不赋值，不能随意猜默认值。

## 3. U8 登录参数映射

| Bridge 配置字段 | U8Login 参数 | 来源 |
| --- | --- | --- |
| `subId` | `sSubId` | 按模块配置，销售类官方示例为 `AS` |
| `accountId` | `sAccID` | 常见格式 `(default)@100`，以登录测试为准 |
| `year` | `sYear` | U8 年度账 |
| `userId` | `sUserID` | U8 操作员 |
| `password` | `sPassword` | U8 操作员密码 |
| `loginDate` | `sDate` | 默认当天或业务单据日期 |
| `server` | `sServer` | U8 登录界面服务器/数据源下拉值 |
| `serial` | `sSerial` | 通常为空 |

## 4. 销售订单新增

内部 U8 API：`U8API/SaleOrder/Save`

### 4.1 普通参数

| U8 参数 | 建议值 | 说明 |
| --- | --- | --- |
| `VoucherState` | `0` | 0 增加，1 修改 |
| `vNewID` | 空字符串 | INOUT，成功后读取 U8 主键 |
| `DomConfig` | 空 XMLDOM | ATO/PTO 选配，首期不启用时传空 |

### 4.2 表头映射

| 信川字段 | U8 字段 | 必填 | 说明 |
| --- | --- | --- | --- |
| `orderNo` | `domHead[0]["csocode"]` | 是 | 销售订单号，建议用信川订单号保持幂等 |
| `orderDate` | `domHead[0]["ddate"]` | 是 | 订单日期 |
| `businessType` | `domHead[0]["cbustype"]` | 待确认 | U8 业务类型，官方示例标为 int |
| `salesTypeName` | `domHead[0]["cstname"]` | 是 | 销售类型名称 |
| `salesTypeCode` | `domHead[0]["cstcode"]` | 是 | 销售类型编码 |
| `customerCode` | `domHead[0]["ccuscode"]` | 是 | U8 客户编码 |
| `customerName` | `domHead[0]["ccusname"]` | 建议 | 客户名称 |
| `customerName` | `domHead[0]["ccusabbname"]` | 是 | 客户简称；若 U8 要求简称，应优先从客户档案取 |
| `departmentCode` | `domHead[0]["cdepcode"]` | 是 | 销售部门编码 |
| `departmentName` | `domHead[0]["cdepname"]` | 是 | 销售部门名称 |
| `taxRate` | `domHead[0]["itaxrate"]` | 是 | 税率 |
| `currency` | `domHead[0]["cexch_name"]` | 是 | 币种，常见为人民币 |
| `maker` | `domHead[0]["cmaker"]` | 是 | 制单人 |
| `returnFlag` | `domHead[0]["breturnflag"]` | 是 | 退货标志，默认值需 U8 确认 |
| `contactPhone` | `domHead[0]["ccushand"]` | 否 | 客户联系人手机 |
| `memo` | `domHead[0]["cmemo"]` | 否 | 备注 |
| `deliveryDate` | `domHead[0]["dpredatebt"]` | 否 | 表头预发货日期 |

### 4.3 表体映射

| 信川字段 | U8 字段 | 必填 | 说明 |
| --- | --- | --- | --- |
| `items[].lineNo` | `domBody[i]["irowno"]` | 是 | 行号 |
| `items[].materialCode` | `domBody[i]["cinvcode"]` | 是 | 存货编码 |
| `items[].materialName` | `domBody[i]["cinvname"]` | 是 | 存货名称 |
| `items[].quantity` | `domBody[i]["iquantity"]` | 是 | 数量 |
| `items[].deliveryDate` | `domBody[i]["dpredate"]` | 是 | 预发货日期 |
| `items[].deliveryDate` | `domBody[i]["dpremodate"]` | 待确认 | 预完工日期 |
| `items[].unitCode` | `domBody[i]["cunitid"]` | 待确认 | 销售单位编码 |
| `items[].unit` | `domBody[i]["cinva_unit"]` | 待确认 | 销售单位 |
| `items[].unit` | `domBody[i]["cinvm_unit"]` | 是 | 主计量单位 |
| `items[].taxUnitPrice` | `domBody[i]["itaxunitprice"]` | 否 | 含税单价 |
| `items[].taxAmount` | `domBody[i]["isum"]` | 否 | 价税合计 |
| `items[].specification` | `domBody[i]["cinvstd"]` | 否 | 规格型号 |
| - | `domBody[i]["editprop"]` | 是 | 新增传 `A` |

## 5. 销售订单审核

内部 U8 API：`U8API/SaleOrder/Audit`

| 信川字段 | U8 字段或参数 | 必填 | 说明 |
| --- | --- | --- | --- |
| `u8Id` | `domHead[0]["id"]` 或相关主键 | 是 | 以新增返回值和 U8 示例验证为准 |
| `orderNo` | `domHead[0]["csocode"]` | 是 | 销售订单号 |
| `verify` | `bVerify` | 是 | `true` 审核，`false` 弃审 |
| `verifier` | `domHead[0]["cverifier"]` | 待确认 | 审核人 |

## 6. 销售发货单新增

内部 U8 API：`U8API/Consignment/Save`

是否首期启用取决于客户流程。如果 WMS 发货后 U8 要先生成发货单，再生成销售出库单，则启用此接口。

### 6.1 表头建议映射

| 信川字段 | U8 字段 | 说明 |
| --- | --- | --- |
| `deliveryNo` | `domHead[0]["cdlcode"]` | 发货单号 |
| `deliveryDate` | `domHead[0]["ddate"]` | 发货日期 |
| `customerCode` | `domHead[0]["ccuscode"]` | 客户编码 |
| `customerName` | `domHead[0]["ccusname"]` / `ccusabbname` | 客户名称 / 简称 |
| `departmentCode` | `domHead[0]["cdepcode"]` | 部门编码 |
| `salesTypeCode` | `domHead[0]["cstcode"]` | 销售类型编码 |
| `maker` | `domHead[0]["cmaker"]` | 制单人 |

### 6.2 表体建议映射

| 信川字段 | U8 字段 | 说明 |
| --- | --- | --- |
| `items[].materialCode` | `domBody[i]["cinvcode"]` | 存货编码 |
| `items[].materialName` | `domBody[i]["cinvname"]` | 存货名称 |
| `items[].quantity` | `domBody[i]["iquantity"]` | 发货数量 |
| `items[].unit` | `domBody[i]["cinvm_unit"]` | 主计量单位 |
| `items[].lineNo` | `domBody[i]["irowno"]` | 行号 |

## 7. 销售出库单新增

内部 U8 API：`U8API/saleout/Add`

### 7.1 普通参数

| U8 参数 | 建议值 | 说明 |
| --- | --- | --- |
| `sVouchType` | `32` 或 U8 顾问确认值 | 官方示例说明销售出库单类型为 32 |
| `domPosition` | 空对象 | 货位，首期不启用货位时传空 |
| `cnnFrom` | 空连接 | 内部事务模式 |
| `VouchId` | 空字符串 | INOUT，成功后读取 |
| `domMsg` | XMLDOM | OUT，接收 U8 消息 |
| `bCheck` | 按配置 | 是否控制可用量 |
| `bBeforCheckStock` | 按配置 | 是否检查可用量 |
| `bIsRedVouch` | false | 是否红字单据 |
| `sAddedState` | 空字符串 | 按官方示例传空 |
| `bReMote` | false | 是否远程 |

### 7.2 表头映射

| 信川字段 | U8 字段 | 必填 | 说明 |
| --- | --- | --- | --- |
| `outboundNo` | `DomHead[0]["ccode"]` | 是 | 出库单号 |
| `outboundDate` | `DomHead[0]["ddate"]` | 是 | 出库日期 |
| `warehouseCode` | `DomHead[0]["cwhcode"]` | 是 | 仓库编码 |
| `warehouseName` | `DomHead[0]["cwhname"]` | 是 | 仓库名称 |
| `businessType` | `DomHead[0]["cbustype"]` | 是 | 业务类型，取值待确认 |
| `customerCode` | `DomHead[0]["ccuscode"]` | 是 | 客户编码 |
| `customerName` | `DomHead[0]["ccusname"]` | 建议 | 客户名称 |
| `customerName` | `DomHead[0]["ccusabbname"]` | 是 | 客户简称 |
| `maker` | `DomHead[0]["cmaker"]` | 是 | 制单人 |
| `departmentCode` | `DomHead[0]["cdepcode"]` | 否 | 销售部门编码 |
| `rdCode` | `DomHead[0]["crdcode"]` | 待确认 | 出库类别编码 |
| `memo` | `DomHead[0]["cmemo"]` | 否 | 备注 |
| - | `DomHead[0]["cvouchtype"]` | 是 | 单据类型 |
| - | `DomHead[0]["brdflag"]` | 是 | 收发标志，取值待确认 |

### 7.3 表体映射

| 信川字段 | U8 字段 | 必填 | 说明 |
| --- | --- | --- | --- |
| `items[].lineNo` | `domBody[i]["irowno"]` | 是 | 行号 |
| `items[].materialCode` | `domBody[i]["cinvcode"]` | 是 | 存货编码 |
| `items[].quantity` | `domBody[i]["iquantity"]` | 是 | 数量 |
| `items[].unit` | `domBody[i]["cinvm_unit"]` | 是 | 主计量单位 |
| `items[].memo` | `domBody[i]["cbmemo"]` | 否 | 行备注 |
| - | `domBody[i]["editprop"]` | 是 | 新增传 `A` |

## 8. 销售出库单审核

内部 U8 API：`U8API/saleout/Audit`

| 信川字段 | U8 字段或参数 | 必填 | 说明 |
| --- | --- | --- | --- |
| `u8Id` | `VouchId` | 是 | 出库单 U8 主键 |
| `vouchType` | `sVouchType` | 是 | 单据类型 |
| `checkStock` | `bCheck` | 是 | 是否控制可用量 |
| `beforeCheckStock` | `bBeforCheckStock` | 是 | 是否检查可用量 |
| `requestId` | - | 是 | Bridge 日志关联 |

## 9. 材料出库单新增

内部 U8 API：`U8API/MaterialOut/Add`

### 9.1 普通参数

与销售出库 `Add` 类似，但 `sVouchType` 取值应由 U8 顾问确认。官方示例注释说明材料出库单类型为 `11`。

### 9.2 表头映射

| 信川字段 | U8 字段 | 必填 | 说明 |
| --- | --- | --- | --- |
| `materialOutNo` | `DomHead[0]["ccode"]` | 是 | 材料出库单号 |
| `outDate` | `DomHead[0]["ddate"]` | 是 | 出库日期 |
| `warehouseCode` | `DomHead[0]["cwhcode"]` | 是 | 仓库编码 |
| `warehouseName` | `DomHead[0]["cwhname"]` | 是 | 仓库名称 |
| `rdCode` | `DomHead[0]["crdcode"]` | 建议 | 出库类别编码 |
| `rdName` | `DomHead[0]["crdname"]` | 建议 | 出库类别 |
| `departmentCode` | `DomHead[0]["cdepcode"]` | 建议 | 部门编码 |
| `departmentName` | `DomHead[0]["cdepname"]` | 建议 | 部门名称 |
| `maker` | `DomHead[0]["cmaker"]` | 是 | 制单人 |
| `sourceNo` | `DomHead[0]["cmpocode"]` | 待确认 | 生产订单号或业务号，需 U8 顾问确认 |
| `productCode` | `DomHead[0]["cpspcode"]` | 待确认 | 产品编码 |
| `memo` | `DomHead[0]["cmemo"]` | 否 | 备注 |

### 9.3 表体映射

| 信川字段 | U8 字段 | 必填 | 说明 |
| --- | --- | --- | --- |
| `items[].lineNo` | `domBody[i]["irowno"]` | 是 | 行号 |
| `items[].materialCode` | `domBody[i]["cinvcode"]` | 是 | 材料编码 |
| `items[].quantity` | `domBody[i]["iquantity"]` | 是 | 数量，字段名需现场验证 |
| `items[].batchNo` | `domBody[i]["cBatch"]` 或批次相关字段 | 待确认 | 需按 U8 字段实际名称确认 |
| `items[].workOrderNo` | `domBody[i]["csourcemocode"]` | 待确认 | 源生产订单号 |
| `items[].sourceDetailId` | `domBody[i]["isourcemodetailsid"]` | 待确认 | 源订单子表标识 |
| - | `domBody[i]["editprop"]` | 是 | 新增传 `A` |

## 10. 领料申请单新增

内部 U8 API：`U8API/materialapp/Add`

是否启用取决于客户 U8 是否要求“先领料申请，再材料出库”。

### 10.1 普通参数

| U8 参数 | 建议值 | 说明 |
| --- | --- | --- |
| `sVouchType` | `64` | 官方示例说明领料申请单类型为 64 |
| `domPosition` | 空对象 | 货位，首期不启用货位时传空 |
| `cnnFrom` | 空连接 | 内部事务模式 |
| `VouchId` | 空字符串 | INOUT，成功后读取 |
| `domMsg` | XMLDOM | OUT，接收 U8 消息 |
| `bCheck` | `true` | 是否控制可用量 |
| `bBeforCheckStock` | `true` | 是否检查可用量 |
| `bIsRedVouch` | `false` | 是否红字单据 |
| `sAddedState` | 空字符串 | 按官方示例传空 |
| `bReMote` | `false` | 是否远程 |

### 10.2 表头映射

| 信川字段 | U8 字段 | 必填 | 说明 |
| --- | --- | --- | --- |
| `applicationNo` | `DomHead[0]["ccode"]` | 是 | 领料申请单号 |
| `applicationDate` | `DomHead[0]["ddate"]` | 是 | 申请日期 |
| `rdCode` | `DomHead[0]["crdcode"]` | 建议 | 出库类别编码 |
| `departmentCode` | `DomHead[0]["cdepcode"]` | 建议 | 部门编码 |
| `maker` | `DomHead[0]["cmaker"]` | 是 | 制单人 |
| `memo` | `DomHead[0]["cmemo"]` | 否 | 备注 |

### 10.3 表体映射

| 信川字段 | U8 字段 | 必填 | 说明 |
| --- | --- | --- | --- |
| `items[].lineNo` | `domBody[i]["irowno"]` | 是 | 行号 |
| `items[].materialCode` | `domBody[i]["cinvcode"]` | 是 | 存货编码 |
| `items[].materialName` | `domBody[i]["cinvname"]` | 建议 | 存货名称 |
| `items[].unit` | `domBody[i]["cinvm_unit"]` | 建议 | 主计量单位 |
| `items[].batchNo` | `domBody[i]["cbatch"]` | 否 | 批号 |
| `items[].quantity` | `domBody[i]["iquantity"]` | 是 | 申请数量 |
| `items[].dueDate` | `domBody[i]["dduedate"]` | 否 | 需求日期 |

## 11. 生产订单新增

内部 U8 API：`U8API/MOrder/MOrderAdd`

生产订单示例使用扩展业务对象 `extbo`，包含 `Mom_Order`、`Mom_OrderDetail`、`Mom_MoAllocate` 等节点。

首期不建议直接纳入，除非客户明确要求 APS/MES 发布生产订单到 U8。

## 12. 待确认基础档案

Bridge 实施前必须由客户/U8 顾问提供下列编码：

| 类型 | 示例字段 | 说明 |
| --- | --- | --- |
| 销售部门 | `departmentCode` | 销售订单、出库单常用 |
| 生产部门 | `departmentCode` | 材料出库、生产订单常用 |
| 销售类型 | `salesTypeCode` | 销售订单必填 |
| 仓库 | `warehouseCode` | 销售出库、材料出库必填 |
| 出库类别 | `rdCode` | 材料出库、销售出库可能需要 |
| 制单人 | `maker` | 通常为 U8 操作员 |
| 审核人 | `verifier` | 自动审核时使用 |
| 币种 | `currency` | 销售订单必填 |
| 税率 | `taxRate` | 销售订单必填 |
| 单位编码 | `unitCode` | 如果 U8 要求单位编码，不能只传名称 |

## 13. 不允许的映射方式

* 不允许直接写 U8 业务表生成单据。
* 不允许用客户名称代替客户编码。
* 不允许用物料名称代替物料编码。
* 不允许 Bridge 随机生成 U8 基础档案编码。
* 不允许在字段缺失时静默生成不完整单据。
* 不允许忽略 U8 返回错误并向 Java 返回成功。
