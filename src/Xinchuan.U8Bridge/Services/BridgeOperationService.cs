using System;
using System.Collections.Generic;
using Xinchuan.U8Bridge.Configuration;
using Xinchuan.U8Bridge.Models;
using Xinchuan.U8Bridge.U8;

namespace Xinchuan.U8Bridge.Services
{
    public sealed class BridgeOperationService
    {
        private readonly BridgeOptions options;
        private readonly IU8ApiClient apiClient;
        private readonly IdempotencyStore idempotencyStore;
        private readonly U8DatabaseQueryService databaseQueryService;

        public BridgeOperationService(
            BridgeOptions options,
            IU8ApiClient apiClient,
            IdempotencyStore idempotencyStore,
            U8DatabaseQueryService databaseQueryService)
        {
            this.options = options;
            this.apiClient = apiClient;
            this.idempotencyStore = idempotencyStore;
            this.databaseQueryService = databaseQueryService;
        }

        public BridgeResponse LoginTest(string headerRequestId, LoginTestRequest request)
        {
            BridgeResponse invalid = ValidateRequestId(headerRequestId, request);
            if (invalid != null)
            {
                return invalid;
            }

            return ResolveProfile(request, out var profile)
                ?? apiClient.LoginTest(request.RequestId, profile);
        }

        public BridgeResponse Invoke(
            string headerRequestId,
            BaseBusinessRequest request,
            U8ApiCall call,
            bool idempotent)
        {
            BridgeResponse invalid = ValidateRequestId(headerRequestId, request);
            if (invalid != null)
            {
                return invalid;
            }

            BridgeResponse missingProfile = ResolveProfile(request, out var profile);
            if (missingProfile != null)
            {
                return missingProfile;
            }

            string key = call.ApiAddress + ":" + call.BusinessNo;
            if (idempotent)
            {
                BridgeResponse existing = idempotencyStore.TryGetOrBegin(key, request, request.RequestId);
                if (existing != null)
                {
                    return existing;
                }
            }

            BridgeResponse response = SafeInvoke(request, profile, call);
            if (idempotent)
            {
                idempotencyStore.Complete(key, response);
            }

            return response;
        }

        public BridgeResponse QueryCustomers(string headerRequestId, MasterQueryRequest request)
        {
            return Query(headerRequestId, request, () => databaseQueryService.QueryCustomers(request), "U8 客户主数据查询成功");
        }

        public BridgeResponse QueryMaterials(string headerRequestId, MasterQueryRequest request)
        {
            return Query(headerRequestId, request, () => databaseQueryService.QueryMaterials(request), "U8 物料主数据查询成功");
        }

        public BridgeResponse QuerySuppliers(string headerRequestId, MasterQueryRequest request)
        {
            return Query(headerRequestId, request, () => databaseQueryService.QuerySuppliers(request), "U8 供应商主数据查询成功");
        }

        public BridgeResponse QueryInventory(string headerRequestId, InventoryQueryRequest request)
        {
            return Query(headerRequestId, request, () => databaseQueryService.QueryInventory(request), "U8 现存量查询成功");
        }

        public BridgeResponse QueryInTransit(string headerRequestId, InTransitQueryRequest request)
        {
            return Query(headerRequestId, request, () => databaseQueryService.QueryInTransit(request), "U8 采购在途查询成功");
        }

        public BridgeResponse QueryMaterialPrice(string headerRequestId, MaterialPriceQueryRequest request)
        {
            return Query(headerRequestId, request, () => databaseQueryService.QueryMaterialPrice(request), "U8 物料价格查询成功");
        }

        public BridgeResponse QueryBomParts(string headerRequestId, BomPartLookupRequest request)
        {
            return Query(headerRequestId, request, () => databaseQueryService.QueryBomParts(request), "U8 BOM 参数查询成功");
        }

        public BridgeResponse QueryBomTree(string headerRequestId, BomPartLookupRequest request)
        {
            return Query(headerRequestId, request, () => databaseQueryService.QueryBomTree(request), "U8 BOM 树查询成功");
        }

        public BridgeResponse LoadBomByCode(string headerRequestId, BomPartLookupRequest request)
        {
            BridgeResponse invalid = ValidateRequestId(headerRequestId, request);
            if (invalid != null)
            {
                return invalid;
            }

            BridgeResponse missingProfile = ResolveProfile(request, out var profile);
            if (missingProfile != null)
            {
                return missingProfile;
            }

            try
            {
                QueryResult lookup = databaseQueryService.QueryBomParts(FirstBomPartLookup(request));
                if (lookup.Items.Count == 0)
                {
                    return BridgeResponse.Fail(
                        request.RequestId,
                        BridgeErrorCodes.U8ResultFailed,
                        "U8 BOM 参数未找到",
                        "materialCode=" + request.MaterialCode);
                }

                BomActionRequest action = CreateBomLoadRequest(request, lookup.Items[0]);
                var call = U8ApiCall.Create(
                    "U8API/BOM/BomLoad",
                    "bom-load-by-code",
                    request.MaterialCode + ":" + action.VersionOrIdentCode,
                    action);
                return SafeInvoke(request, profile, call);
            }
            catch (Exception ex)
            {
                BridgeLogger.Error("U8 BOM load by code failed. RequestId=" + request.RequestId, ex);
                return BridgeResponse.Fail(request.RequestId, BridgeErrorCodes.U8DatabaseError, "U8 BOM 参数查询失败", ex.Message);
            }
        }

        private BridgeResponse Query(
            string headerRequestId,
            BaseBusinessRequest request,
            Func<QueryResult> query,
            string message)
        {
            BridgeResponse invalid = ValidateRequestId(headerRequestId, request);
            if (invalid != null)
            {
                return invalid;
            }

            try
            {
                return BridgeResponse.OkData(request.RequestId, message, query());
            }
            catch (Exception ex)
            {
                BridgeLogger.Error("U8 database query failed. RequestId=" + request.RequestId, ex);
                return BridgeResponse.Fail(request.RequestId, BridgeErrorCodes.U8DatabaseError, "U8 只读数据库查询失败", ex.Message);
            }
        }

        private BridgeResponse SafeInvoke(BaseBusinessRequest request, U8ProfileOptions profile, U8ApiCall call)
        {
            try
            {
                return apiClient.Invoke(request.RequestId, profile, call);
            }
            catch (Exception ex)
            {
                BridgeLogger.Error(
                    "U8 API call failed. RequestId="
                    + request.RequestId
                    + ", ApiAddress="
                    + call.ApiAddress
                    + ", BusinessNo="
                    + call.BusinessNo,
                    ex);
                return BridgeResponse.Fail(
                    request.RequestId,
                    BridgeErrorCodes.BridgeInternalError,
                    "U8 Bridge 内部异常",
                    ex.Message);
            }
        }

        private BridgeResponse ResolveProfile(BaseBusinessRequest request, out U8ProfileOptions profile)
        {
            string profileName = string.IsNullOrWhiteSpace(request.ProfileName)
                ? options.DefaultProfileName
                : request.ProfileName;
            if (!options.Profiles.TryGetValue(profileName, out profile))
            {
                return BridgeResponse.Fail(
                    request.RequestId,
                    BridgeErrorCodes.LoginProfileNotFound,
                    "指定 U8 登录 Profile 不存在");
            }

            return null;
        }

        private static BridgeResponse ValidateRequestId(string headerRequestId, BaseBusinessRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RequestId))
            {
                return BridgeResponse.Fail(headerRequestId, BridgeErrorCodes.RequestInvalid, "requestId 不能为空");
            }

            if (!string.Equals(headerRequestId, request.RequestId, StringComparison.Ordinal))
            {
                return BridgeResponse.Fail(
                    request.RequestId,
                    BridgeErrorCodes.RequestIdMismatch,
                    "X-Request-ID 与请求体 requestId 不一致");
            }

            return null;
        }

        private static BomPartLookupRequest FirstBomPartLookup(BomPartLookupRequest request)
        {
            return new BomPartLookupRequest
            {
                RequestId = request.RequestId,
                ProfileName = request.ProfileName,
                MaterialCode = request.MaterialCode,
                BomType = request.BomType,
                VersionOrIdentCode = request.VersionOrIdentCode,
                PageNo = 1,
                PageSize = 1
            };
        }

        private static BomActionRequest CreateBomLoadRequest(
            BomPartLookupRequest request,
            IDictionary<string, object> row)
        {
            return new BomActionRequest
            {
                RequestId = request.RequestId,
                ProfileName = request.ProfileName,
                PartId = ReadInt(row, "partId"),
                BomType = ReadInt(row, "bomType"),
                VersionOrIdentCode = Convert.ToString(row["versionOrIdentCode"])
            };
        }

        private static int ReadInt(IDictionary<string, object> row, string key)
        {
            object value;
            if (!row.TryGetValue(key, out value) || value == null)
            {
                throw new InvalidOperationException("U8 BOM 参数缺少 " + key);
            }

            return Convert.ToInt32(value);
        }
    }
}
