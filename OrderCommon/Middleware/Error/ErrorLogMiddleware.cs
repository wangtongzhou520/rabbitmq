using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OrderCommon
{
    public class ErrorLogMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ErrorLogMiddleware> logger;

        public ErrorLogMiddleware(RequestDelegate next,ILogger<ErrorLogMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
           ErrorResult errorResult = null;
            try
            {
                await next(httpContext);
            }
            catch (CustomException ex)
            {
                if (string.IsNullOrWhiteSpace(ex.ErrorMessage))
                    ex.ErrorMessage = "未知异常,请稍后再试";
                errorResult = new ErrorResult(ex.ErrorMessage, ex.ErrorCode);
            }
            catch (Exception ex)
            {
                errorResult = new ErrorResult("-1", "系统开小差了,请稍后再试");
                logger.LogError(ex, $"全局异常,状态码：{ httpContext?.Response?.StatusCode}，Url：{httpContext?.Request?.GetDisplayUrl()}");
            }
            finally
            {
                if (errorResult != null)
                {
                    var Message = JsonConvert.SerializeObject(errorResult);
                    await HandleExceptionAsync(httpContext, Message);
                }

                //这个地方要拦截下404
                if (!httpContext.Response.HasStarted)
                {
                    httpContext.Response.ContentType = "application/json";
                    errorResult = new ErrorResult();
                    errorResult.ErrorCode = httpContext.Response.StatusCode.ToString();
                    switch (httpContext.Response.StatusCode)
                    {
                        case 404:
                            errorResult.Message = "未找到服务";
                            break;
                        default:
                            errorResult.Message = "未知错误";
                            break;
                    }

                    var Message = JsonConvert.SerializeObject(errorResult);
                    await HandleExceptionAsync(httpContext, Message);
                }
            }
        }

        private Task HandleExceptionAsync(HttpContext context, string message)
        {
            context.Response.ContentType = "application/json;charset=utf-8";
            return context.Response.WriteAsync(message);
        }
    }
}
