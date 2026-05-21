using System;
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

        public BridgeOperationService(
            BridgeOptions options,
            IU8ApiClient apiClient,
            IdempotencyStore idempotencyStore)
        {
            this.options = options;
            this.apiClient = apiClient;
            this.idempotencyStore = idempotencyStore;
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
    }
}
