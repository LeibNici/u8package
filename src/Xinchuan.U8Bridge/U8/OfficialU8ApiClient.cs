using System;
using System.Runtime.InteropServices;
using Xinchuan.U8Bridge.Configuration;
using Xinchuan.U8Bridge.Models;
using Xinchuan.U8Bridge.Services;

namespace Xinchuan.U8Bridge.U8
{
    public sealed class OfficialU8ApiClient : IU8ApiClient
    {
        private readonly BridgeOptions options;

        public OfficialU8ApiClient(BridgeOptions options)
        {
            this.options = options;
        }

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
            BridgeResponse invalid = ValidateInvoke(requestId, profile, call);
            if (invalid != null)
            {
                return invalid;
            }

            object login = null;
            try
            {
                string shareString;
                login = CreateLogin(profile, out shareString);
                if (login == null)
                {
                    return BridgeResponse.Fail(
                        requestId,
                        BridgeErrorCodes.U8LoginFailed,
                        "U8 登录失败",
                        shareString);
                }

                BridgeLogger.Info("Invoking U8 API. RequestId=" + requestId + ", ApiAddress=" + call.ApiAddress);
                var runtime = new U8BrokerRuntime(options, login);
                return runtime.Invoke(requestId, call);
            }
            catch (COMException ex)
            {
                return BridgeResponse.Fail(
                    requestId,
                    BridgeErrorCodes.U8ComponentUnavailable,
                    "U8 COM 组件不可用",
                    ex.Message);
            }
            catch (BridgeRuntimeEnvironmentException ex)
            {
                return BridgeResponse.Fail(
                    requestId,
                    BridgeErrorCodes.BridgeInternalError,
                    "Bridge 运行环境异常",
                    DescribeException(ex));
            }
            catch (InvalidOperationException ex)
            {
                return BridgeResponse.Fail(
                    requestId,
                    BridgeErrorCodes.U8ComponentUnavailable,
                    "U8 API Framework 组件不可用",
                    DescribeException(ex));
            }
            catch (Exception ex)
            {
                BridgeLogger.Error("U8 API invoke failed. RequestId=" + requestId, ex);
                return BridgeResponse.Fail(requestId, BridgeErrorCodes.U8SysError, "U8 API 调用异常", DescribeException(ex));
            }
            finally
            {
                ReleaseCom(login);
            }
        }

        private static BridgeResponse TryLogin(string requestId, U8ProfileOptions profile)
        {
            object login = null;
            try
            {
                string shareString;
                login = CreateLogin(profile, out shareString);
                return login != null
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
                ReleaseCom(login);
            }
        }

        private static object CreateLogin(U8ProfileOptions profile, out string shareString)
        {
            shareString = null;
            Type loginType = Type.GetTypeFromProgID("U8Login.clsLogin");
            if (loginType == null)
            {
                throw new COMException("当前 Windows 环境未注册 U8Login.clsLogin");
            }

            object login = Activator.CreateInstance(loginType);
            bool ok = InvokeLogin(login, profile);
            shareString = Convert.ToString(loginType.InvokeMember(
                "ShareString",
                System.Reflection.BindingFlags.GetProperty,
                null,
                login,
                null));
            if (ok)
            {
                return login;
            }

            ReleaseCom(login);
            return null;
        }

        private static BridgeResponse ValidateInvoke(string requestId, U8ProfileOptions profile, U8ApiCall call)
        {
            if (HasMissingLoginField(profile))
            {
                return BridgeResponse.Fail(requestId, BridgeErrorCodes.RequestInvalid, "U8 登录 Profile 缺少必要字段");
            }

            if (call == null || string.IsNullOrWhiteSpace(call.ApiAddress))
            {
                return BridgeResponse.Fail(requestId, BridgeErrorCodes.U8ApiNotSupported, "U8 API 地址未配置");
            }

            return null;
        }

        private static string DescribeException(Exception ex)
        {
            Exception root = ex.GetBaseException();
            if (root == null || root == ex)
            {
                return ex.Message;
            }

            return root.GetType().FullName + ": " + root.Message;
        }

        private static void ReleaseCom(object value)
        {
            if (value != null && Marshal.IsComObject(value))
            {
                Marshal.FinalReleaseComObject(value);
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
