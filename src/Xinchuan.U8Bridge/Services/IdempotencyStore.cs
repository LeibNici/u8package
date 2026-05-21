using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Xinchuan.U8Bridge.Models;

namespace Xinchuan.U8Bridge.Services
{
    public sealed class IdempotencyStore
    {
        private readonly ConcurrentDictionary<string, Record> records =
            new ConcurrentDictionary<string, Record>();

        public BridgeResponse TryGetOrBegin(string key, object payload, string requestId)
        {
            string payloadHash = Hash(payload);
            var newRecord = new Record { PayloadHash = payloadHash, Locked = true };
            if (records.TryAdd(key, newRecord))
            {
                return null;
            }

            var record = records[key];
            if (record.PayloadHash != payloadHash)
            {
                return BridgeResponse.Fail(
                    requestId,
                    BridgeErrorCodes.IdempotencyConflict,
                    "同一业务单号重复请求但内容不一致");
            }

            if (record.Response != null)
            {
                return record.Response;
            }

            if (!record.Locked)
            {
                record.Locked = true;
                return null;
            }

            return BridgeResponse.Fail(
                requestId,
                BridgeErrorCodes.IdempotencyRecordLocked,
                "同一业务单号正在处理中");
        }

        public void Complete(string key, BridgeResponse response)
        {
            if (!records.TryGetValue(key, out var record))
            {
                return;
            }

            record.Response = response;
            record.Locked = false;
        }

        private static string Hash(object payload)
        {
            string json = JsonConvert.SerializeObject(payload);
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
                return System.Convert.ToBase64String(bytes);
            }
        }

        private sealed class Record
        {
            public string PayloadHash { get; set; }
            public bool Locked { get; set; }
            public BridgeResponse Response { get; set; }
        }
    }
}
