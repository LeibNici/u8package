using System;
using System.Globalization;

namespace Xinchuan.U8Bridge.U8
{
    public static class U8BrokerResultReader
    {
        public static bool IsSuccess(object returnValue, string message, U8BrokerCall call)
        {
            if (call.BooleanReturn)
            {
                bool parsed;
                if (TryReadBoolean(returnValue, out parsed))
                {
                    return parsed && !LooksLikeFailureMessage(message);
                }

                if (LooksLikeFailureMessage(message))
                {
                    return false;
                }

                return true;
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
            string id = ReadNamedId(broker, reflection, call.ReturnIdName);
            if (!string.IsNullOrWhiteSpace(id))
            {
                return id;
            }

            id = ReadNamedId(broker, reflection, "vNewID");
            return !string.IsNullOrWhiteSpace(id)
                ? id
                : ReadNamedId(broker, reflection, "VouchId");
        }

        private static string ReadNamedId(object broker, U8Reflection reflection, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return Convert.ToString(CallGetResult(broker, reflection, name));
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

        private static bool TryReadBoolean(object value, out bool result)
        {
            result = false;
            if (value == null)
            {
                return false;
            }

            if (value is bool)
            {
                result = (bool)value;
                return true;
            }

            string text = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text.Trim();
            if (bool.TryParse(text, out result))
            {
                return true;
            }

            int numeric;
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out numeric))
            {
                result = numeric != 0;
                return true;
            }

            return false;
        }

        private static bool LooksLikeFailureMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            string text = message.Trim();
            return text.IndexOf("失败", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("错误", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("异常", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("不足", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("不成功", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("拒绝", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("不能", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("无效", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("不存在", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("未找到", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("fail", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("denied", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("invalid", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("not found", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
