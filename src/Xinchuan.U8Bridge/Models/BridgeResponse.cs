using System.Net;
using Newtonsoft.Json;

namespace Xinchuan.U8Bridge.Models
{
    public sealed class BridgeResponse
    {
        public bool Success { get; set; }

        public string RequestId { get; set; }

        public string U8Code { get; set; }

        public string U8Id { get; set; }

        public string Message { get; set; }

        public string ErrorCode { get; set; }

        public string RawMessage { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Data { get; set; }

        public static BridgeResponse Ok(
            string requestId,
            string message,
            string u8Id = null,
            string u8Code = null,
            string rawMessage = null)
        {
            return new BridgeResponse
            {
                Success = true,
                RequestId = requestId,
                U8Code = u8Code,
                U8Id = u8Id,
                Message = message,
                RawMessage = rawMessage
            };
        }

        public static BridgeResponse Fail(string requestId, string errorCode, string message, string rawMessage = null)
        {
            return new BridgeResponse
            {
                Success = false,
                RequestId = requestId,
                ErrorCode = errorCode,
                Message = message,
                RawMessage = rawMessage
            };
        }

        public static BridgeResponse OkData(
            string requestId,
            string message,
            object data,
            string u8Id = null,
            string u8Code = null,
            string rawMessage = null)
        {
            return new BridgeResponse
            {
                Success = true,
                RequestId = requestId,
                U8Code = u8Code,
                U8Id = u8Id,
                Message = message,
                RawMessage = rawMessage,
                Data = data
            };
        }

        public HttpStatusCode ToHttpStatus()
        {
            switch (ErrorCode)
            {
                case null:
                    return HttpStatusCode.OK;
                case BridgeErrorCodes.AuthMissing:
                case BridgeErrorCodes.AuthInvalid:
                    return HttpStatusCode.Unauthorized;
                case BridgeErrorCodes.SourceForbidden:
                    return HttpStatusCode.Forbidden;
                case BridgeErrorCodes.RequestInvalid:
                case BridgeErrorCodes.RequestIdMismatch:
                case BridgeErrorCodes.LoginProfileNotFound:
                case BridgeErrorCodes.U8ApiNotSupported:
                    return HttpStatusCode.BadRequest;
                case BridgeErrorCodes.IdempotencyConflict:
                case BridgeErrorCodes.IdempotencyRecordLocked:
                    return HttpStatusCode.Conflict;
                case BridgeErrorCodes.U8BizError:
                case BridgeErrorCodes.U8ResultFailed:
                    return (HttpStatusCode)422;
                case BridgeErrorCodes.U8LoginFailed:
                case BridgeErrorCodes.U8ComponentUnavailable:
                case BridgeErrorCodes.U8DatabaseError:
                    return HttpStatusCode.ServiceUnavailable;
                default:
                    return HttpStatusCode.InternalServerError;
            }
        }
    }
}
