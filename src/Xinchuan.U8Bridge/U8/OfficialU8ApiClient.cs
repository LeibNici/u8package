using System;
using System.Runtime.InteropServices;
using Xinchuan.U8Bridge.Configuration;
using Xinchuan.U8Bridge.Models;

namespace Xinchuan.U8Bridge.U8
{
    public sealed class OfficialU8ApiClient : IU8ApiClient
    {
        public BridgeResponse LoginTest(string requestId, U8ProfileOptions profile)
        {
            if (HasMissingLoginField(profile))
            {
                return BridgeResponse.Fail(
                    requestId,
                    BridgeErrorCodes.RequestInvalid,
                    "U8 登录 Profile 缺少必要字段");
            }

            return TryLogin(requestId, profile);
        }

        public BridgeResponse Invoke(string requestId, U8ProfileOptions profile, U8ApiCall call)
        {
            if (string.IsNullOrWhiteSpace(call.ApiAddress))
            {
                return BridgeResponse.Fail(requestId, BridgeErrorCodes.U8ApiNotSupported, "U8 API 地址未配置");
            }

            return BridgeResponse.Fail(
                requestId,
                BridgeErrorCodes.U8ApiNotSupported,
                "U8 API Broker 写单据调用层尚未完成字段映射绑定",
                call.ApiAddress);
        }

        private static BridgeResponse TryLogin(string requestId, U8ProfileOptions profile)
        {
            object login = null;
            try
            {
                Type loginType = Type.GetTypeFromProgID("U8Login.clsLogin");
                if (loginType == null)
                {
                    return BridgeResponse.Fail(
                        requestId,
                        BridgeErrorCodes.U8ComponentUnavailable,
                        "当前 Windows 环境未注册 U8Login.clsLogin");
                }

                login = Activator.CreateInstance(loginType);
                bool ok = InvokeLogin(login, profile);
                string shareString = Convert.ToString(loginType.InvokeMember(
                    "ShareString",
                    System.Reflection.BindingFlags.GetProperty,
                    null,
                    login,
                    null));
                return ok
                    ? BridgeResponse.Ok(requestId, "U8 登录成功")
                    : BridgeResponse.Fail(requestId, BridgeErrorCodes.U8LoginFailed, "U8 登录失败", shareString);
            }
            catch (COMException ex)
            {
                return BridgeResponse.Fail(
                    requestId,
                    BridgeErrorCodes.U8ComponentUnavailable,
                    "U8 COM 组件不可用",
                    ex.Message);
            }
            catch (Exception ex)
            {
                return BridgeResponse.Fail(requestId, BridgeErrorCodes.U8SysError, "U8 登录调用异常", ex.Message);
            }
            finally
            {
                if (login != null && Marshal.IsComObject(login))
                {
                    Marshal.FinalReleaseComObject(login);
                }
            }
        }

        private static bool InvokeLogin(object login, U8ProfileOptions profile)
        {
            string subId = profile.SubId;
            string accountId = profile.AccountId;
            string year = profile.Year;
            string userId = profile.UserId;
            string password = profile.Password ?? string.Empty;
            string loginDate = profile.LoginDate;
            string server = profile.Server;
            string serial = profile.Serial ?? string.Empty;
            object[] args = { subId, accountId, year, userId, password, loginDate, server, serial };
            object result = login.GetType().InvokeMember(
                "Login",
                System.Reflection.BindingFlags.InvokeMethod,
                null,
                login,
                args);
            return result is bool ok && ok;
        }

        private static bool HasMissingLoginField(U8ProfileOptions profile)
        {
            return profile == null
                || string.IsNullOrWhiteSpace(profile.SubId)
                || string.IsNullOrWhiteSpace(profile.AccountId)
                || string.IsNullOrWhiteSpace(profile.Year)
                || string.IsNullOrWhiteSpace(profile.UserId)
                || string.IsNullOrWhiteSpace(profile.LoginDate)
                || string.IsNullOrWhiteSpace(profile.Server);
        }
    }
}
