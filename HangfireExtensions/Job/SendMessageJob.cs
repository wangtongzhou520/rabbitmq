using System;
using System.Collections.Generic;
using System.Text;
using Hangfire;
using Microsoft.Extensions.Logging;
using OrderCommon;
using OrderDal;
using OrderService;

namespace HangfireExtensions
{
    public class SendMessageJob : ISendMessageJob
    {
        private readonly IBaseDal<OrderMessageLogEntity> orderService;
        private readonly ILogger<SendMessageJob> logger;
        private readonly IMessageService<OrderMessageLogEntity> messageService;
        private static bool IsRunning;
        private static object obj = new object();

        private const string EXCHANGENAME = "order";

        private const string QUEUENAME = "order";

        private const string ROUTINGKEY = "order";


        public SendMessageJob(IBaseDal<OrderMessageLogEntity> orderService,ILogger<SendMessageJob> logger,IMessageService<OrderMessageLogEntity> messageService)
        {
            this.orderService = orderService;
            this.logger = logger;
            this.messageService = messageService;
        }
        public void SendMessageService()
        {
            if (IsRunning == false)
            {
                lock (obj)
                {
                    if (IsRunning == false)
                    {
                        IsRunning = true;
                        try
                        {
                            var retryOrderMessages = orderService.GetList(x => x.Status == 0 && x.NextRetryTime < DateTime.Now && x.TryCount < 6);
                            logger.LogDebug($"查询到的条数为{retryOrderMessages.Count}");
                            Message message = new Message();
                            message.ExchangeName = EXCHANGENAME;
                            message.RoutingKey = ROUTINGKEY;
                            message.QueueName = QUEUENAME;

                            messageService.BatchSendMessage(message, retryOrderMessages);
                        }
                        catch (Exception ex)
                        {
                            IsRunning = false;
                            logger.LogError("定时任务执行异常", ex);
                        }
                        IsRunning = false;
                    }
                }
            }
        }
    }
}
