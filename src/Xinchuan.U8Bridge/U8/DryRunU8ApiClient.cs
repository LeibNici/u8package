using Xinchuan.U8Bridge.Configuration;
using Xinchuan.U8Bridge.Models;

namespace Xinchuan.U8Bridge.U8
{
    public sealed class DryRunU8ApiClient : IU8ApiClient
    {
        public BridgeResponse LoginTest(string requestId, U8ProfileOptions profile)
        {
            return BridgeResponse.Ok(requestId, "DRY_RUN: U8 登录参数格式已接收");
        }

        public BridgeResponse Invoke(string requestId, U8ProfileOptions profile, U8ApiCall call)
        {
            string u8Id = "DRY-" + call.DocumentType + "-" + call.BusinessNo;
            return BridgeResponse.Ok(requestId, "DRY_RUN: 已模拟调用 " + call.ApiAddress, u8Id, call.BusinessNo);
        }
    }
}
