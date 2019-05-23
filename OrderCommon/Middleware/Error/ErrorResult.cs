using System;
using System.Collections.Generic;
using System.Text;

namespace OrderCommon
{
    public class ErrorResult
    {
        public ErrorResult() { }

        public ErrorResult(string message,string code)
        {
            Message = message;
            ErrorCode = code;
        }

        public string Message { get; set; }
        public string ErrorCode { get; set; }
    }
}
