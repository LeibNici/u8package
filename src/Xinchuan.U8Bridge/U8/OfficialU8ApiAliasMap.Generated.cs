using System;
using System.Collections.Generic;

namespace Xinchuan.U8Bridge.U8
{
    internal static class OfficialU8ApiAliasMap
    {
        private const string RoutePrefix = "api/u8/";
        private static readonly IDictionary<string, string> Aliases = CreateAliases();

        public static bool TryGetOfficialApi(string bridgePath, out string officialApi)
        {
            return Aliases.TryGetValue(Normalize(bridgePath), out officialApi);
        }

        private static IDictionary<string, string> CreateAliases()
        {
            var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in Entries)
            {
                aliases[entry.Key] = entry.Value;
            }

            return aliases;
        }

        private static string Normalize(string bridgePath)
        {
            if (string.IsNullOrWhiteSpace(bridgePath))
            {
                return string.Empty;
            }

            string normalized = bridgePath.Trim().Trim('/');
            return normalized.StartsWith(RoutePrefix, StringComparison.OrdinalIgnoreCase)
                ? normalized.Substring(RoutePrefix.Length)
                : normalized;
        }

        private static KeyValuePair<string, string> Pair(string bridgePath, string officialApi)
        {
            return new KeyValuePair<string, string>(bridgePath, officialApi);
        }

        private static readonly KeyValuePair<string, string>[] Entries = new[]
        {
            Pair("ap-apply-pay/cancel-sign", "U8API/APApplyPay/CancelSign"), Pair("ap-apply-pay/delete-vouch", "U8API/APApplyPay/DeleteVouch"),
            Pair("ap-apply-pay/get-vouch-data", "U8API/APApplyPay/GetVouchData"), Pair("ap-apply-pay/save-vouch", "U8API/APApplyPay/SaveVouch"),
            Pair("ap-apply-pay/sign", "U8API/APApplyPay/Sign"), Pair("ap-close/cancel-sign", "U8API/APClose/CancelSign"),
            Pair("ap-close/delete-vouch", "U8API/APClose/DeleteVouch"), Pair("ap-close/get-vouch-data", "U8API/APClose/GetVouchData"),
            Pair("ap-close/save-vouch", "U8API/APClose/SaveVouch"), Pair("ap-close/sign", "U8API/APClose/Sign"),
            Pair("ap-vouch/cancel-sign", "U8API/APVouch/CancelSign"), Pair("ap-vouch/delete-vouch", "U8API/APVouch/DeleteVouch"),
            Pair("ap-vouch/get-vouch-data", "U8API/APVouch/GetVouchData"), Pair("ap-vouch/save-vouch", "U8API/APVouch/SaveVouch"),
            Pair("ap-vouch/sign", "U8API/APVouch/Sign"), Pair("ap/ap-work-flow-srv-cls-budget-plug", "U8ERP_V8.70/AP/APWorkFlowSrv.clsBudgetPlug"),
            Pair("ap/ap-work-flow-srv-cls-close-final-verify-srv", "U8ERP_V8.70/AP/APWorkFlowSrv.clsCloseFinalVerifySrv"), Pair("ap/ap-work-flow-srv-cls-un-budget-plug", "U8ERP_V8.70/AP/APWorkFlowSrv.clsUnBudgetPlug"),
            Pair("ar-close/cancel-sign", "U8API/ARClose/CancelSign"), Pair("ar-close/delete-vouch", "U8API/ARClose/DeleteVouch"),
            Pair("ar-close/get-vouch-data", "U8API/ARClose/GetVouchData"), Pair("ar-close/save-vouch", "U8API/ARClose/SaveVouch"),
            Pair("ar-close/sign", "U8API/ARClose/Sign"), Pair("ar-vouch/cancel-sign", "U8API/ARVouch/CancelSign"),
            Pair("ar-vouch/delete-vouch", "U8API/ARVouch/DeleteVouch"), Pair("ar-vouch/get-vouch-data", "U8API/ARVouch/GetVouchData"),
            Pair("ar-vouch/save-vouch", "U8API/ARVouch/SaveVouch"), Pair("ar-vouch/sign", "U8API/ARVouch/Sign"),
            Pair("ar/ap-work-flow-srv-cls-close-final-verify-srv", "U8ERP_V8.70/AR/APWorkFlowSrv.clsCloseFinalVerifySrv"), Pair("arr-check/voucher-operate", "U8API/ArrCheck/VoucherOperate"),
            Pair("arr-inspect/voucher-operate", "U8API/ArrInspect/VoucherOperate"), Pair("arrived-goods/cancelconfirm-arr", "U8API/ArrivedGoods/CancelconfirmArr"),
            Pair("arrived-goods/confirm-arr", "U8API/ArrivedGoods/ConfirmArr"), Pair("arrived-goods/delete", "U8API/ArrivedGoods/Delete"),
            Pair("arrived-goods/get-voucher-data", "U8API/ArrivedGoods/GetVoucherData"), Pair("arrived-goods/voucher-save", "U8API/ArrivedGoods/VoucherSave"),
            Pair("audit/gs-work-flow-srv-cls-gs-work-flow-srv", "U8ERP_V8.70/Audit/GSWorkFlowSrv.clsGSWorkFlowSrv"), Pair("audit/hyep-pu-wf-audit-srv-cls-last-audit-msg-api", "U8ERP_V8.70/Audit/HYEP_PuWFAuditSrv.clsLastAuditMsgAPI"),
            Pair("audit/om-work-flow-srv-cls-om-work-flow-srv-auto", "U8ERP_V8.70/Audit/OMWorkFlowSrv.clsOMWorkFlowSrvAuto"), Pair("audit/pu-work-flow-srv-cls-budget-service", "U8ERP_V8.70/Audit/PUWorkFlowSrv.clsBudgetService"),
            Pair("audit/pu-work-flow-srv-cls-pu-work-flow-srv-auto", "U8ERP_V8.70/Audit/PUWorkFlowSrv.clsPUWorkFlowSrvAuto"), Pair("audit/pu-work-flow-srv-cls-un-budget-service", "U8ERP_V8.70/Audit/PUWorkFlowSrv.clsUnBudgetService"),
            Pair("audit/qm-work-flow-srv-cls-qm-work-flow-srv", "U8ERP_V8.70/Audit/QMWorkFlowSrv.clsQMWorkFlowSrv"), Pair("audit/sa-work-flow-srv-cls-date-cus-auto-service", "U8ERP_V8.70/Audit/SAWorkFlowSrv.clsDateCusAutoService"),
            Pair("audit/sa-work-flow-srv-cls-date-dep-auto-service", "U8ERP_V8.70/Audit/SAWorkFlowSrv.clsDateDepAutoService"), Pair("audit/sa-work-flow-srv-cls-date-per-auto-service", "U8ERP_V8.70/Audit/SAWorkFlowSrv.clsDatePerAutoService"),
            Pair("audit/sa-work-flow-srv-cls-min-price-auto-service", "U8ERP_V8.70/Audit/SAWorkFlowSrv.clsMinPriceAutoService"), Pair("audit/sa-work-flow-srv-cls-money-cus-auto-service", "U8ERP_V8.70/Audit/SAWorkFlowSrv.clsMoneyCusAutoService"),
            Pair("audit/sa-work-flow-srv-cls-money-dep-auto-service", "U8ERP_V8.70/Audit/SAWorkFlowSrv.clsMoneyDepAutoService"), Pair("audit/sa-work-flow-srv-cls-money-per-auto-service", "U8ERP_V8.70/Audit/SAWorkFlowSrv.clsMoneyPerAutoService"),
            Pair("audit/sa-work-flow-srv-cls-sa-auto-srv", "U8ERP_V8.70/Audit/SAWorkFlowSrv.clsSAAutoSrv"), Pair("audit/st-work-flow-srv-cls-st-vouch-auto-srv", "U8ERP_V8.70/Audit/STWorkFlowSrv.clsSTVouchAutoSrv"),
            Pair("audit/szdz-hu-work-flow-srv-cls-work-flow-srv-auto", "U8ERP_V8.70/Audit/SZDZ_HU_WorkFlowSrv.clsWorkFlowSrvAuto"), Pair("audit/turbo-crm-work-f-low-srv-cls-crm-verify-srv", "U8ERP_V8.70/Audit/TurboCRMWorkFLowSrv.clsCRMVerifySrv"),
            Pair("audit/u-fsoft-u8-ecn-service-ecn-apply-service-audit", "U8ERP_V8.70/Audit/UFsoft.U8.ECN.Service.EcnApplyService.Audit"), Pair("audit/uf-soft-u8-ex-credit-audit-service-audit", "U8ERP_V8.70/Audit/UFSoft.U8.EX.Credit.AuditService.Audit"),
            Pair("audit/uf-soft-u8-ex-credit-credit-service-get-credit-date-service", "U8ERP_V8.70/Audit/UFSoft.U8.EX.Credit.CreditService.GetCreditDateService"), Pair("audit/uf-soft-u8-ex-credit-credit-service-get-credit-money-service", "U8ERP_V8.70/Audit/UFSoft.U8.EX.Credit.CreditService.GetCreditMoneyService"),
            Pair("audit/uf-soft-u8-ex-credit-lowest-price-service-get-lowest-price-service", "U8ERP_V8.70/Audit/UFSoft.U8.EX.Credit.LowestPriceService.GetLowestPriceService"), Pair("audit/uf-soft-u8-im-work-flow-services-audit-service", "U8ERP_V8.70/Audit/UFSoft.U8.IM.WorkFlow.Services.AuditService"),
            Pair("audit/uf-soft-u8-u8-m-plugins-u8-m-service-audit", "U8ERP_V8.70/Audit/UFSoft.U8.U8M.Plugins.U8MService.Audit"), Pair("audit/ufdia-u8-bm-audit-plugs-audit-service-auto-audition", "U8ERP_V8.70/Audit/UFDIA.U8.BM.AuditPlugs.AuditService.AutoAudition"),
            Pair("audit/ufdia-u8-bm-audit-plugs-audit-service-auto-over-budget-service", "U8ERP_V8.70/Audit/UFDIA.U8.BM.AuditPlugs.AuditService.AutoOverBudgetService"), Pair("audit/ufida-u8-audit-change-service-voucher-change-service", "U8ERP_V8.70/Audit/UFIDA.U8.Audit.ChangeService.VoucherChangeService"),
            Pair("audit/ufida-u8-audit-change-service-voucher-change-service-do-action", "U8ERP_V8.70/Audit/UFIDA.U8.Audit.ChangeService.VoucherChangeService.DoAction"), Pair("audit/ufida-u8-hr-work-flow-hm", "U8ERP_V8.70/Audit/UFIDA.U8.HR.WorkFlow.HM"),
            Pair("audit/ufida-u8-hr-work-flow-tm", "U8ERP_V8.70/Audit/UFIDA.U8.HR.WorkFlow.TM"), Pair("audit/ufida-u8-pf-work-flow-services-audit-service", "U8ERP_V8.70/Audit/UFIDA.U8.PF.WorkFlow.Services.AuditService"),
            Pair("bom/bom-update", "U8API/BOM/BomUpdate"), Pair("checkvouch/add", "U8API/checkvouch/Add"),
            Pair("checkvouch/audit", "U8API/checkvouch/Audit"), Pair("checkvouch/cancel-audit", "U8API/checkvouch/CancelAudit"),
            Pair("checkvouch/delete", "U8API/checkvouch/Delete"), Pair("checkvouch/load", "U8API/checkvouch/Load"),
            Pair("checkvouch/update", "U8API/checkvouch/Update"), Pair("cm/cm-work-flow-srv-cls-cm-final-verify-pi", "U8ERP_V8.70/CM/CMWorkFlowSrv.clsCMFinalVerifyPI"),
            Pair("cm/delete-vouch", "U8API/CM/DeleteVouch"), Pair("cm/query-vouch", "U8API/CM/QueryVouch"),
            Pair("cm/save-vouch", "U8API/CM/SaveVouch"), Pair("consignment-sign-in/audit", "U8API/ConsignmentSignIn/Audit"),
            Pair("consignment-sign-in/load", "U8API/ConsignmentSignIn/Load"), Pair("consignment-sign-in/save", "U8API/ConsignmentSignIn/Save"),
            Pair("consignment/delete", "U8API/Consignment/Delete"), Pair("consignment/load", "U8API/Consignment/Load"),
            Pair("delegate-consignment/audit", "U8API/DelegateConsignment/Audit"), Pair("delegate-consignment/delete", "U8API/DelegateConsignment/Delete"),
            Pair("delegate-consignment/load", "U8API/DelegateConsignment/Load"), Pair("delegate-consignment/save", "U8API/DelegateConsignment/Save"),
            Pair("delegate-return-order/audit", "U8API/DelegateReturnOrder/Audit"), Pair("delegate-return-order/delete", "U8API/DelegateReturnOrder/Delete"),
            Pair("delegate-return-order/load", "U8API/DelegateReturnOrder/Load"), Pair("delegate-return-order/save", "U8API/DelegateReturnOrder/Save"),
            Pair("eb/eb-item-add", "U8API/EB/EBItem_Add"), Pair("eb/eb-trade-add", "U8API/EB/EBTrade_Add"),
            Pair("ecn-doc/ecn-doc-add", "U8API/EcnDoc/EcnDocAdd"), Pair("ecn-doc/ecn-doc-auditing", "U8API/EcnDoc/EcnDocAuditing"),
            Pair("ecn-doc/ecn-doc-delete", "U8API/EcnDoc/EcnDocDelete"), Pair("ecn-doc/ecn-doc-load", "U8API/EcnDoc/EcnDocLoad"),
            Pair("ecn-doc/ecn-doc-modify", "U8API/EcnDoc/EcnDocModify"), Pair("ecn-doc/ecn-doc-unauditing", "U8API/EcnDoc/EcnDocUnauditing"),
            Pair("fb/fb-audit-plug-audit-service", "U8ERP_V8.70/FB/FBAuditPlug.AuditService"), Pair("fc-resource/fc-resource-add", "U8API/FCResource/FCResourceAdd"),
            Pair("fc-resource/fc-resource-delete", "U8API/FCResource/FCResourceDelete"), Pair("fc-resource/fc-resource-load", "U8API/FCResource/FCResourceLoad"),
            Pair("fc-resource/fc-resource-modify", "U8API/FCResource/FCResourceModify"), Pair("fd/fd-work-flow", "U8ERP_V8.70/FD/FDWorkFlow"),
            Pair("forecast/forecast-add", "U8API/Forecast/ForecastAdd"), Pair("forecast/forecast-auditing", "U8API/Forecast/ForecastAuditing"),
            Pair("forecast/forecast-delete", "U8API/Forecast/ForecastDelete"), Pair("forecast/forecast-load", "U8API/Forecast/ForecastLoad"),
            Pair("forecast/forecast-modify", "U8API/Forecast/ForecastModify"), Pair("forecast/forecast-unauditing", "U8API/Forecast/ForecastUnauditing"),
            Pair("group-vouch/add", "U8API/GroupVouch/Add"), Pair("group-vouch/audit", "U8API/GroupVouch/Audit"),
            Pair("group-vouch/cancel-audit", "U8API/GroupVouch/CancelAudit"), Pair("group-vouch/delete", "U8API/GroupVouch/Delete"),
            Pair("group-vouch/load", "U8API/GroupVouch/Load"), Pair("group-vouch/update", "U8API/GroupVouch/Update"),
            Pair("hr/hm-workflow-get-duty-level", "U8ERP_V8.70/HR/HMWorkflowGetDutyLevel"), Pair("hr/hm-workflow-get-job-grade", "U8ERP_V8.70/HR/HMWorkflowGetJobGrade"),
            Pair("hr/ufida-u8-hb-work-flow-services", "U8ERP_V8.70/HR/UFIDA.U8.HB.WorkFlow.Services"), Pair("hr/ufida-u8-workflow-hr-desiger-find-attribute-designer", "U8ERP_V8.70/HR/UFIDA.U8.Workflow.HR.Desiger.FindAttributeDesigner"),
            Pair("hr/ufida-u8-workflow-hr-desiger-find-psn-designer", "U8ERP_V8.70/HR/UFIDA.U8.Workflow.HR.Desiger.FindPsnDesigner"), Pair("matchvouch/add", "U8API/matchvouch/Add"),
            Pair("matchvouch/audit", "U8API/matchvouch/Audit"), Pair("matchvouch/delete", "U8API/matchvouch/Delete"),
            Pair("matchvouch/load", "U8API/matchvouch/Load"), Pair("matchvouch/split-vouch", "U8API/matchvouch/SplitVouch"),
            Pair("matchvouch/update", "U8API/matchvouch/Update"), Pair("material-out/load", "U8API/MaterialOut/Load"),
            Pair("material-out/update", "U8API/MaterialOut/Update"), Pair("materialapp/cancel-audit", "U8API/materialapp/CancelAudit"),
            Pair("materialapp/delete", "U8API/materialapp/Delete"), Pair("materialapp/load", "U8API/materialapp/Load"),
            Pair("materialapp/update", "U8API/materialapp/Update"), Pair("mo-routing-bill/mo-routing-bill-add", "U8API/MoRoutingBill/MoRoutingBillAdd"),
            Pair("mo-routing/mo-routing-load", "U8API/MoRouting/MoRoutingLoad"), Pair("ne/ne-audit-plug-audit-service", "U8ERP_V8.70/NE/NEAuditPlug.AuditService"),
            Pair("ne/ne-audit-plug-budget-service", "U8ERP_V8.70/NE/NEAuditPlug.BudgetService"), Pair("ne/ne-audit-plug-un-budget-service", "U8ERP_V8.70/NE/NEAuditPlug.UnBudgetService"),
            Pair("ne/ne-get-over-budget-info", "U8ERP_V8.70/NE/NEGetOverBudgetInfo"), Pair("normal-invoice/audit", "U8API/NormalInvoice/Audit"),
            Pair("normal-invoice/delete", "U8API/NormalInvoice/Delete"), Pair("normal-invoice/load", "U8API/NormalInvoice/Load"),
            Pair("normal-invoice/save", "U8API/NormalInvoice/Save"), Pair("om-order/voucher-operate", "U8API/omOrder/VoucherOperate"),
            Pair("op-transform/op-transform-add", "U8API/OpTransform/OpTransformAdd"), Pair("op-transform/op-transform-delete", "U8API/OpTransform/OpTransformDelete"),
            Pair("op-transform/op-transform-load", "U8API/OpTransform/OpTransformLoad"), Pair("order-bom/order-bom-add", "U8API/OrderBom/OrderBomAdd"),
            Pair("order-bom/order-bom-auditing", "U8API/OrderBom/OrderBomAuditing"), Pair("order-bom/order-bom-delete", "U8API/OrderBom/OrderBomDelete"),
            Pair("order-bom/order-bom-load", "U8API/OrderBom/OrderBomLoad"), Pair("order-bom/order-bom-unauditing", "U8API/OrderBom/OrderBomUnauditing"),
            Pair("order-bom/order-bom-update", "U8API/OrderBom/OrderBomUpdate"), Pair("otherin/add", "U8API/otherin/Add"),
            Pair("otherin/audit", "U8API/otherin/Audit"), Pair("otherin/cancel-audit", "U8API/otherin/CancelAudit"),
            Pair("otherin/delete", "U8API/otherin/Delete"), Pair("otherin/load", "U8API/otherin/Load"),
            Pair("otherin/update", "U8API/otherin/Update"), Pair("otherout/add", "U8API/otherout/Add"),
            Pair("otherout/audit", "U8API/otherout/Audit"), Pair("otherout/cancel-audit", "U8API/otherout/CancelAudit"),
            Pair("otherout/delete", "U8API/otherout/Delete"), Pair("otherout/load", "U8API/otherout/Load"),
            Pair("otherout/update", "U8API/otherout/Update"), Pair("per-check/voucher-operate", "U8API/PerCheck/VoucherOperate"),
            Pair("per-inspect/voucher-operate", "U8API/PerInspect/VoucherOperate"), Pair("pf-report/pf-report-add", "U8API/PFReport/PFReportAdd"),
            Pair("pf-report/pf-report-delete", "U8API/PFReport/PFReportDelete"), Pair("pf-report/pf-report-load", "U8API/PFReport/PFReportLoad"),
            Pair("pf-report/pf-report-update", "U8API/PFReport/PFReportUpdate"), Pair("pf-report/pf-report2-work-hr-note", "U8API/PFReport/PFReport2WorkHRNote"),
            Pair("plan-range/plan-range-add", "U8API/PlanRange/PlanRangeAdd"), Pair("plan-range/plan-range-delete", "U8API/PlanRange/PlanRangeDelete"),
            Pair("plan-range/plan-range-load", "U8API/PlanRange/PlanRangeLoad"), Pair("plan-range/plan-range-update", "U8API/PlanRange/PlanRangeUpdate"),
            Pair("ppurbill/delete", "U8API/ppurbill/Delete"), Pair("ppurbill/get-voucher-data", "U8API/ppurbill/GetVoucherData"),
            Pair("ppurbill/voucher-save", "U8API/ppurbill/VoucherSave"), Pair("pro-check/voucher-operate", "U8API/ProCheck/VoucherOperate"),
            Pair("pro-inspect/voucher-operate", "U8API/ProInspect/VoucherOperate"), Pair("product-in/audit", "U8API/ProductIn/Audit"),
            Pair("product-in/cancel-audit", "U8API/ProductIn/CancelAudit"), Pair("product-in/delete", "U8API/ProductIn/Delete"),
            Pair("product-in/load", "U8API/ProductIn/Load"), Pair("product-in/update", "U8API/ProductIn/Update"),
            Pair("pu-invoice/cancel-sign", "U8API/PUInvoice/CancelSign"), Pair("pu-invoice/sign", "U8API/PUInvoice/Sign"),
            Pair("pu-store-in/add", "U8API/PuStoreIn/Add"), Pair("pu-store-in/audit", "U8API/PuStoreIn/Audit"),
            Pair("pu-store-in/cancel-audit", "U8API/PuStoreIn/CancelAudit"), Pair("pu-store-in/delete", "U8API/PuStoreIn/Delete"),
            Pair("pu-store-in/load", "U8API/PuStoreIn/Load"), Pair("pu-store-in/update", "U8API/PuStoreIn/Update"),
            Pair("pu/voucher-save2", "U8API/PU/VoucherSave2"), Pair("pur-bill/delete", "U8API/PurBill/Delete"),
            Pair("pur-bill/get-voucher-data", "U8API/PurBill/GetVoucherData"), Pair("pur-bill/voucher-save", "U8API/PurBill/VoucherSave"),
            Pair("purchase-order/delete", "U8API/PurchaseOrder/Delete"), Pair("purchase-order/get-voucher-data", "U8API/PurchaseOrder/GetVoucherData"),
            Pair("purchase-order/voucher-save", "U8API/PurchaseOrder/VoucherSave"), Pair("purchase-requisition/cancelconfirm-app", "U8API/PurchaseRequisition/CancelconfirmApp"),
            Pair("purchase-requisition/confirm-app", "U8API/PurchaseRequisition/ConfirmApp"), Pair("purchase-requisition/delete", "U8API/PurchaseRequisition/Delete"),
            Pair("purchase-requisition/get-voucher-data", "U8API/PurchaseRequisition/GetVoucherData"), Pair("purchase-requisition/voucher-save", "U8API/PurchaseRequisition/VoucherSave"),
            Pair("qc-scrap-vouch/add", "U8API/QcScrapVouch/Add"), Pair("qc-scrap-vouch/audit", "U8API/QcScrapVouch/Audit"),
            Pair("qc-scrap-vouch/cancel-audit", "U8API/QcScrapVouch/CancelAudit"), Pair("qc-scrap-vouch/delete", "U8API/QcScrapVouch/Delete"),
            Pair("qc-scrap-vouch/load", "U8API/QcScrapVouch/Load"), Pair("qc-scrap-vouch/update", "U8API/QcScrapVouch/Update"),
            Pair("red-normal-invoice/audit", "U8API/RedNormalInvoice/Audit"), Pair("red-normal-invoice/delete", "U8API/RedNormalInvoice/Delete"),
            Pair("red-normal-invoice/load", "U8API/RedNormalInvoice/Load"), Pair("red-normal-invoice/save", "U8API/RedNormalInvoice/Save"),
            Pair("red-retaildailyreport/audit", "U8API/RedRetaildailyreport/Audit"), Pair("red-retaildailyreport/delete", "U8API/RedRetaildailyreport/Delete"),
            Pair("red-retaildailyreport/load", "U8API/RedRetaildailyreport/Load"), Pair("red-retaildailyreport/save", "U8API/RedRetaildailyreport/Save"),
            Pair("red-special-invoice/audit", "U8API/RedSpecialInvoice/Audit"), Pair("red-special-invoice/delete", "U8API/RedSpecialInvoice/Delete"),
            Pair("red-special-invoice/load", "U8API/RedSpecialInvoice/Load"), Pair("red-special-invoice/save", "U8API/RedSpecialInvoice/Save"),
            Pair("retaildailyreport/audit", "U8API/Retaildailyreport/Audit"), Pair("retaildailyreport/delete", "U8API/Retaildailyreport/Delete"),
            Pair("retaildailyreport/load", "U8API/Retaildailyreport/Load"), Pair("retaildailyreport/save", "U8API/Retaildailyreport/Save"),
            Pair("return-order/audit", "U8API/ReturnOrder/Audit"), Pair("return-order/delete", "U8API/ReturnOrder/Delete"),
            Pair("return-order/load", "U8API/ReturnOrder/Load"), Pair("return-order/save", "U8API/ReturnOrder/Save"),
            Pair("routing/routing-add", "U8API/Routing/RoutingAdd"), Pair("routing/routing-auditing", "U8API/Routing/RoutingAuditing"),
            Pair("routing/routing-delete", "U8API/Routing/RoutingDelete"), Pair("routing/routing-load", "U8API/Routing/RoutingLoad"),
            Pair("routing/routing-unauditing", "U8API/Routing/RoutingUnauditing"), Pair("routing/routing-update", "U8API/Routing/RoutingUpdate"),
            Pair("sa-invoice/cancel-sign", "U8API/SAInvoice/CancelSign"), Pair("sa-invoice/sign", "U8API/SAInvoice/Sign"),
            Pair("sa/sa-voucher-audit", "U8API/SA/SAVoucher_Audit"), Pair("sa/sa-voucher-delete", "U8API/SA/SAVoucher_Delete"),
            Pair("sa/sa-voucher-load", "U8API/SA/SAVoucher_Load"), Pair("sa/sa-voucher-save", "U8API/SA/SAVoucher_Save"),
            Pair("sale-order/close", "U8API/SaleOrder/Close"), Pair("sale-order/delete", "U8API/SaleOrder/Delete"),
            Pair("sale-order/load", "U8API/SaleOrder/Load"), Pair("sale-order/lock", "U8API/SaleOrder/Lock"),
            Pair("sale-quotation/audit", "U8API/SaleQuotation/Audit"), Pair("sale-quotation/delete", "U8API/SaleQuotation/Delete"),
            Pair("sale-quotation/load", "U8API/SaleQuotation/Load"), Pair("sale-quotation/save", "U8API/SaleQuotation/Save"),
            Pair("saleout/cancel-audit", "U8API/saleout/CancelAudit"), Pair("saleout/delete", "U8API/saleout/Delete"),
            Pair("saleout/load", "U8API/saleout/Load"), Pair("saleout/update", "U8API/saleout/Update"),
            Pair("scrap-out/add", "U8API/ScrapOut/Add"), Pair("scrap-out/audit", "U8API/ScrapOut/Audit"),
            Pair("scrap-out/cancel-audit", "U8API/ScrapOut/CancelAudit"), Pair("scrap-out/delete", "U8API/ScrapOut/Delete"),
            Pair("scrap-out/load", "U8API/ScrapOut/Load"), Pair("scrap-out/update", "U8API/ScrapOut/Update"),
            Pair("scrap-vouch/add", "U8API/ScrapVouch/Add"), Pair("scrap-vouch/audit", "U8API/ScrapVouch/Audit"),
            Pair("scrap-vouch/cancel-audit", "U8API/ScrapVouch/CancelAudit"), Pair("scrap-vouch/delete", "U8API/ScrapVouch/Delete"),
            Pair("scrap-vouch/load", "U8API/ScrapVouch/Load"), Pair("scrap-vouch/update", "U8API/ScrapVouch/Update"),
            Pair("sep-vouch/add", "U8API/SepVouch/Add"), Pair("sep-vouch/audit", "U8API/SepVouch/Audit"),
            Pair("sep-vouch/cancel-audit", "U8API/SepVouch/CancelAudit"), Pair("sep-vouch/delete", "U8API/SepVouch/Delete"),
            Pair("sep-vouch/load", "U8API/SepVouch/Load"), Pair("sep-vouch/update", "U8API/SepVouch/Update"),
            Pair("shape-chang-vouch/add", "U8API/ShapeChangVouch/Add"), Pair("shape-chang-vouch/audit", "U8API/ShapeChangVouch/Audit"),
            Pair("shape-chang-vouch/cancel-audit", "U8API/ShapeChangVouch/CancelAudit"), Pair("shape-chang-vouch/delete", "U8API/ShapeChangVouch/Delete"),
            Pair("shape-chang-vouch/load", "U8API/ShapeChangVouch/Load"), Pair("shape-chang-vouch/update", "U8API/ShapeChangVouch/Update"),
            Pair("special-invoice/audit", "U8API/SpecialInvoice/Audit"), Pair("special-invoice/delete", "U8API/SpecialInvoice/Delete"),
            Pair("special-invoice/load", "U8API/SpecialInvoice/Load"), Pair("special-invoice/save", "U8API/SpecialInvoice/Save"),
            Pair("trans-request-vouch/add", "U8API/TransRequestVouch/Add"), Pair("trans-request-vouch/audit", "U8API/TransRequestVouch/Audit"),
            Pair("trans-request-vouch/cancel-audit", "U8API/TransRequestVouch/CancelAudit"), Pair("trans-request-vouch/delete", "U8API/TransRequestVouch/Delete"),
            Pair("trans-request-vouch/load", "U8API/TransRequestVouch/Load"), Pair("trans-request-vouch/update", "U8API/TransRequestVouch/Update"),
            Pair("trans-vouch/add", "U8API/TransVouch/Add"), Pair("trans-vouch/audit", "U8API/TransVouch/Audit"),
            Pair("trans-vouch/cancel-audit", "U8API/TransVouch/CancelAudit"), Pair("trans-vouch/delete", "U8API/TransVouch/Delete"),
            Pair("trans-vouch/load", "U8API/TransVouch/Load"), Pair("trans-vouch/update", "U8API/TransVouch/Update"),
            Pair("uap/uap-final-audit-service", "U8ERP_V8.70/UAP/UAPFinalAuditService"), Pair("ven-inv-price/confirm-ven-inv-price", "U8API/VenInvPrice/ConfirmVenInvPrice"),
            Pair("work-hr-note/work-hr-note-add", "U8API/WorkHrNote/WorkHrNoteAdd"), Pair("work-hr-note/work-hr-note-delete", "U8API/WorkHrNote/WorkHrNoteDelete"),
            Pair("work-hr-note/work-hr-note-load", "U8API/WorkHrNote/WorkHrNoteLoad"), Pair("ypurbill/delete", "U8API/ypurbill/Delete"),
            Pair("ypurbill/get-voucher-data", "U8API/ypurbill/GetVoucherData"), Pair("ypurbill/voucher-save", "U8API/ypurbill/VoucherSave"),
        };
    }
}
