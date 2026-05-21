using System;

namespace Xinchuan.U8Bridge.U8
{
    public static class U8BrokerResultReader
    {
        public static bool IsSuccess(object returnValue, string message, U8BrokerCall call)
        {
            if (call.BooleanReturn)
            {
                return Convert.ToBoolean(returnValue);
            }

            return string.IsNullOrWhiteSpace(message);
        }

        public static string ReadMessage(object returnValue, object broker, U8Reflection reflection, U8BrokerCall call)
        {
            if (!call.BooleanReturn)
            {
                return Convert.ToString(returnValue);
            }

            string errMsg = Convert.ToString(CallGetResult(broker, reflection, "errMsg"));
            if (!string.IsNullOrWhiteSpace(errMsg))
            {
                return errMsg;
            }

            return Convert.ToString(returnValue);
        }

        public static string ReadId(object broker, U8Reflection reflection, U8BrokerCall call)
        {
            if (string.IsNullOrWhiteSpace(call.ReturnIdName))
            {
                return null;
            }

            return Convert.ToString(CallGetResult(broker, reflection, call.ReturnIdName));
        }

        private static object CallGetResult(object broker, U8Reflection reflection, string name)
        {
            try
            {
                return reflection.Call(broker, "GetResult", name);
            }
            catch
            {
                return null;
            }
        }
    }
}
