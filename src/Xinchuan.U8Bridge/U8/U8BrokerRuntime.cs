using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xinchuan.U8Bridge.Configuration;
using Xinchuan.U8Bridge.Models;

namespace Xinchuan.U8Bridge.U8
{
    public sealed class U8BrokerRuntime
    {
        private readonly BridgeOptions options;
        private readonly object login;
        private readonly U8Reflection reflection;

        public U8BrokerRuntime(BridgeOptions options, object login)
        {
            this.options = options;
            this.login = login;
            reflection = new U8Reflection(GetProbeDirectories(options));
        }

        public BridgeResponse Invoke(string requestId, U8ApiCall call)
        {
            object broker = null;
            try
            {
                U8BrokerCall brokerCall = U8BrokerDocumentMapper.Map(call);
                broker = CreateBroker(call.ApiAddress, brokerCall);
                brokerCall.ApplyTo(broker, reflection);
                return InvokeBroker(requestId, call, broker, brokerCall);
            }
            finally
            {
                ReleaseBroker(broker);
            }
        }

        private object CreateBroker(string apiAddress, U8BrokerCall brokerCall)
        {
            Type envType = reflection.ResolveType("UFIDA.U8.U8APIFramework.U8EnvContext");
            Type addressType = reflection.ResolveType("UFIDA.U8.U8APIFramework.U8ApiAddress");
            Type brokerType = reflection.ResolveType("UFIDA.U8.U8APIFramework.U8ApiBroker");
            object env = Activator.CreateInstance(envType);
            envType.InvokeMember("U8Login", BindingFlags.SetProperty, null, env, new[] { login });
            if (brokerCall.VoucherType.HasValue)
            {
                brokerCall.ContextValues["VoucherType"] = brokerCall.VoucherType.Value;
            }

            foreach (var item in brokerCall.ContextValues)
            {
                envType.InvokeMember(
                   "SetApiContext",
                   BindingFlags.InvokeMethod,
                   null,
                   env,
                   new[] { item.Key, U8BrokerCall.ResolveValue(item.Value, reflection) });
            }

            object address = Activator.CreateInstance(addressType, apiAddress);
            return Activator.CreateInstance(brokerType, address, env);
        }

        private BridgeResponse InvokeBroker(
            string requestId,
            U8ApiCall call,
            object broker,
            U8BrokerCall brokerCall)
        {
            bool invokeOk = Convert.ToBoolean(reflection.Call(broker, "Invoke"));
            if (!invokeOk)
            {
                return BuildInvokeFailure(requestId, broker);
            }

            object returnValue = reflection.Call(broker, "GetReturnValue");
            string message = U8BrokerResultReader.ReadMessage(returnValue, broker, reflection, brokerCall);
            if (!U8BrokerResultReader.IsSuccess(returnValue, message, brokerCall))
            {
                return BridgeResponse.Fail(requestId, BridgeErrorCodes.U8ResultFailed, "U8 API 返回失败", message);
            }

            string u8Id = U8BrokerResultReader.ReadId(broker, reflection, brokerCall);
            if (brokerCall.DataReader != null)
            {
                return BridgeResponse.OkData(requestId, "U8 API 调用成功", brokerCall.DataReader(broker, reflection));
            }

            return BridgeResponse.Ok(requestId, "U8 API 调用成功", u8Id ?? call.BusinessNo);
        }

        private BridgeResponse BuildInvokeFailure(string requestId, object broker)
        {
            object exception = reflection.Call(broker, "GetException");
            string raw = Convert.ToString(reflection.Call(broker, "GetExceptionString"));
            string message = exception == null ? raw : Convert.ToString(GetProperty(exception, "Message"));
            string typeName = exception == null ? string.Empty : exception.GetType().FullName;
            if (typeName.IndexOf("MomBizException", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return BridgeResponse.Fail(requestId, BridgeErrorCodes.U8BizError, "U8 业务校验失败", raw ?? message);
            }

            if (typeName.IndexOf("MomSysException", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return BridgeResponse.Fail(requestId, BridgeErrorCodes.U8SysError, "U8 系统异常", raw ?? message);
            }

            return BridgeResponse.Fail(requestId, BridgeErrorCodes.BridgeInternalError, message, raw);
        }

        private static object GetProperty(object target, string name)
        {
            return target.GetType().InvokeMember(name, BindingFlags.GetProperty, null, target, null);
        }

        private void ReleaseBroker(object broker)
        {
            if (broker != null)
            {
                try
                {
                    reflection.Call(broker, "Release");
                }
                catch
                {
                }
            }
        }

        private static IEnumerable<string> GetProbeDirectories(BridgeOptions options)
        {
            yield return AppDomain.CurrentDomain.BaseDirectory;
            yield return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "U8APIFramework");
            yield return Environment.GetEnvironmentVariable("U8_API_DLL_DIR");
            yield return @"C:\U8SOFT\UFMOM\U8APIFramework";
            yield return @"C:\U8SOFT\Interop";
            yield return @"C:\U8SOFT\ufcomsql";
        }
    }
}
