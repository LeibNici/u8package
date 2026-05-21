namespace Xinchuan.U8Bridge.Models
{
    public static class BridgeErrorCodes
    {
        public const string AuthMissing = "AUTH_MISSING";
        public const string AuthInvalid = "AUTH_INVALID";
        public const string SourceForbidden = "SOURCE_FORBIDDEN";
        public const string RequestInvalid = "REQUEST_INVALID";
        public const string RequestIdMismatch = "REQUEST_ID_MISMATCH";
        public const string LoginProfileNotFound = "LOGIN_PROFILE_NOT_FOUND";
        public const string U8LoginFailed = "U8_LOGIN_FAILED";
        public const string U8ComponentUnavailable = "U8_COMPONENT_UNAVAILABLE";
        public const string U8ApiNotSupported = "U8_API_NOT_SUPPORTED";
        public const string U8SysError = "U8_SYS_ERROR";
        public const string U8BizError = "U8_BIZ_ERROR";
        public const string U8ResultFailed = "U8_RESULT_FAILED";
        public const string IdempotencyConflict = "IDEMPOTENCY_CONFLICT";
        public const string IdempotencyRecordLocked = "IDEMPOTENCY_RECORD_LOCKED";
        public const string BridgeInternalError = "BRIDGE_INTERNAL_ERROR";
    }
}
