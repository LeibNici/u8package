using Xinchuan.U8Bridge.Configuration;
using Xinchuan.U8Bridge.Models;

namespace Xinchuan.U8Bridge.U8
{
    public interface IU8ApiClient
    {
        BridgeResponse LoginTest(string requestId, U8ProfileOptions profile);

        BridgeResponse Invoke(string requestId, U8ProfileOptions profile, U8ApiCall call);
    }
}
