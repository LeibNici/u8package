using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UFIDA.U8.U8APIFramework;
using UFIDA.U8.U8APIFramework.Parameter;
using Xinchuan.U8Bridge.Configuration;
using Xinchuan.U8Bridge.Models;
using Xinchuan.U8Bridge.Services;

namespace Xinchuan.U8Bridge.U8
{
    public sealed class U8BrokerRuntime
    {
        private readonly object login;
        private readonly U8Reflection reflection;

        public U8BrokerRuntime(BridgeOptions options, object login)
        {
            this.login = login;
            string frameworkDirectory = ResolveEmbeddedFrameworkDirectory();
            BridgeLogger.Info("Using embedded U8 API Framework: " + frameworkDirectory);
            reflection = new U8Reflection(frameworkDirectory);
        }

        public BridgeResponse Invoke(string requestId, U8ApiCall call)
        {
            U8ApiBroker broker = null;
            try
            {
                U8BrokerCall brokerCall = U8BrokerDocumentMapper.Map(call);
                broker = CreateBroker(call.ApiAddress, brokerCall);
                ApplyTo(broker, brokerCall);
                return InvokeBroker(requestId, call, broker, brokerCall);
            }
            finally
            {
                ReleaseBroker(broker);
            }
        }

        private U8ApiBroker CreateBroker(string apiAddress, U8BrokerCall brokerCall)
        {
            var env = new U8EnvContext();
            env.GetType().InvokeMember("U8Login", BindingFlags.SetProperty, null, env, new[] { login });
            if (brokerCall.VoucherType.HasValue)
            {
                brokerCall.ContextValues["VoucherType"] = brokerCall.VoucherType.Value;
            }

            foreach (var item in brokerCall.ContextValues)
            {
                env.SetApiContext(item.Key, U8BrokerCall.ResolveValue(item.Value, reflection));
            }

            return new U8ApiBroker(new U8ApiAddress(apiAddress), env);
        }

        private void ApplyTo(U8ApiBroker broker, U8BrokerCall brokerCall)
        {
            foreach (KeyValuePair<string, object> item in brokerCall.NormalValues)
            {
                broker.AssignNormalValue(item.Key, U8BrokerCall.ResolveValue(item.Value, reflection));
            }

            foreach (U8BoObject item in brokerCall.BusinessObjects)
            {
                ApplyBusinessObject(broker.GetBoParam(item.Name), item);
            }

            foreach (U8ExtBoObject item in brokerCall.ExtensionObjects)
            {
                ApplyExtensionObject(broker.GetExtBoEntity(item.Name), item);
            }
        }

        private void ApplyBusinessObject(BusinessObject bo, U8BoObject item)
        {
            bo.RowCount = item.Rows.Count;
            for (int row = 0; row < item.Rows.Count; row++)
            {
                foreach (KeyValuePair<string, object> field in item.Rows[row])
                {
                    bo[row][field.Key] = U8BrokerCall.ResolveValue(field.Value, reflection);
                }
            }
        }

        private void ApplyExtensionObject(ExtensionBusinessEntity entity, U8ExtBoObject item)
        {
            entity.ItemCount = item.Rows.Count;
            for (int row = 0; row < item.Rows.Count; row++)
            {
                foreach (KeyValuePair<string, object> field in item.Rows[row].Fields)
                {
                    entity[row][field.Key] = U8BrokerCall.ResolveValue(field.Value, reflection);
                }

                foreach (KeyValuePair<string, U8ExtBoObject> child in item.Rows[row].Children)
                {
                    ApplyExtensionObject(entity[row].SubEntity[child.Key], child.Value);
                }
            }
        }

        private BridgeResponse InvokeBroker(
            string requestId,
            U8ApiCall call,
            U8ApiBroker broker,
            U8BrokerCall brokerCall)
        {
            bool invokeOk = broker.Invoke();
            if (!invokeOk)
            {
                return BuildInvokeFailure(requestId, broker);
            }

            object returnValue = broker.GetReturnValue();
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

        private BridgeResponse BuildInvokeFailure(string requestId, U8ApiBroker broker)
        {
            object exception = broker.GetException();
            string raw = broker.GetExceptionString();
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

        private static void ReleaseBroker(U8ApiBroker broker)
        {
            if (broker != null)
            {
                try
                {
                    broker.Release();
                }
                catch
                {
                }
            }
        }

        private static string ResolveEmbeddedFrameworkDirectory()
        {
            string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "U8APIFramework");
            if (!Directory.Exists(directory))
            {
                throw new InvalidOperationException(
                    "内置 U8APIFramework 目录不存在: " + directory);
            }

            return directory;
        }
    }
}
