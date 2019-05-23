using System;
using System.Collections.Generic;
using System.Text;

namespace OrderCommon
{
    /// <summary>
    /// 自定义异常
    /// </summary>
    public class CustomException:Exception
    {
        public CustomException(string code)
        {
            ErrorCode = code;
        }
        public CustomException(string code, string message) : base(message)
        {
            ErrorCode = code;
            ErrorMessage = message;
        }

        public CustomException(string code, string message, Exception exception) : base(message, exception)
        {
            ErrorCode = code;
            ErrorMessage = message + exception.Message;
        }
        /// <summary>
        /// 错误码
        /// </summary>
        public string ErrorCode { get; set; }
        /// <summary>
        /// 错误描述
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
