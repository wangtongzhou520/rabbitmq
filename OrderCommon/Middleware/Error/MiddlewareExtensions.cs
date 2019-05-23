using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;

namespace OrderCommon
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorLog(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorLogMiddleware>();
        }
    }
}
