using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using OrderCommon;
using OrderDal;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQExtensions;

namespace OrderService
{
    public class MessageService<T>:IMessageService<T> where T : BaseMessage, new()
    {
        private readonly IRabbitMQPersistentConnection rabbitMQPersistentConnection;
        private readonly ILogger<MessageService<T>> logger;
        private readonly IBaseDal<T> baseDal;
        private const int RETRYCOUNT = 3;


        public MessageService(IRabbitMQPersistentConnection rabbitMQPersistentConnection, ILogger<MessageService<T>> logger, IBaseDal<T> baseDal)
        {
            this.rabbitMQPersistentConnection = rabbitMQPersistentConnection;
            this.logger = logger;
            this.baseDal = baseDal;
        }

        /// <summary>
        /// 发送单条消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="tMessageLog"></param>
        public void SendMessage(Message message, T tMessageLog)
        {

            if (!rabbitMQPersistentConnection.IsConnected)
            {
                rabbitMQPersistentConnection.TryConnect();
            }

            var policy = RetryPolicy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(RETRYCOUNT, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    logger.LogWarning(ex.ToString());
                });

            using (var channel = rabbitMQPersistentConnection.CreateModel())
            {
                channel.ExchangeDeclare(message.ExchangeName, "direct", true, false);
                channel.QueueDeclare(message.QueueName, true, false, false);
                channel.QueueBind(message.ExchangeName, message.QueueName, message.RoutingKey);

                policy.Execute(() =>
                {
                    channel.ConfirmSelect();
                    var properties = channel.CreateBasicProperties();
                    properties.MessageId = message.MessageId.ToString();
                    properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                    properties.Persistent = true;

                    channel.BasicPublish(message.ExchangeName, message.RoutingKey, properties, message.Body);

                    //如果返回成功,就改变状态表示发送成功
                    //返回失败也不需要处理,定时任务会再次推送
                    if (channel.WaitForConfirms())
                    {
                        //这里Update失败
                        //我认为也不需要处理，重新发一次消息即可
                        //失败一般是数据库挂掉、连接池满、或者网络问题
                        //重试也是无用的,还不如重新发送一次消息
                        tMessageLog.UpdateTime = DateTime.Now;
                        tMessageLog.NextRetryTime = DateTime.Now.AddMinutes(5);
                        tMessageLog.Status = (int)MessageStatusEnum.SEND_SUCCESS;
                        baseDal.Update(tMessageLog);

                    }

                });
            }

        }


        /// <summary>
        /// 批量发送消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ts"></param>
        public void BatchSendMessage(Message message, List<T> ts)
        {

            if (!ts.Any())
            {
                return;
            }

            if (!rabbitMQPersistentConnection.IsConnected)
            {
                rabbitMQPersistentConnection.TryConnect();
            }

            var policy = RetryPolicy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(RETRYCOUNT, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    logger.LogWarning(ex.ToString());
                });

            using (var channel = rabbitMQPersistentConnection.CreateModel())
            {
                channel.ExchangeDeclare(message.ExchangeName, "direct", true, false);
                channel.QueueDeclare(message.QueueName, true, false, false);
                channel.QueueBind(message.ExchangeName, message.QueueName, message.RoutingKey);

                policy.Execute(() =>
                {
                    channel.ConfirmSelect();


                    foreach (var item in ts)
                    {
                        var properties = channel.CreateBasicProperties();
                        properties.MessageId = item.MessageId.ToString();
                        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                        properties.Persistent = true;
                        channel.BasicPublish(message.ExchangeName, message.RoutingKey, properties, Encoding.UTF8.GetBytes(item.MessageInfo));
                    }
                    //如果返回成功,就改变状态表示发送成功
                    //返回失败也不需要处理,定时任务会再次推送
                    if (channel.WaitForConfirms())
                    {
                        //这里Update失败
                        //我认为也不需要处理，重新发一次消息即可
                        //失败一般是数据库挂掉、连接池满、或者网络问题
                        //重试也是无用的,还不如重新发送一次消息
                        foreach (var item in ts)
                        {
                            item.UpdateTime = DateTime.Now;
                            item.NextRetryTime = DateTime.Now.AddMinutes(5);
                            item.Status = (int)MessageStatusEnum.SEND_SUCCESS;
                        }

                        baseDal.UpdateRange(ts.ToArray());

                    }

                });
            }
        }


        /// <summary>
        /// 发送延时消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="tMessageLog"></param>
        public void SendDelayMessage(Message message, T tMessageLog)
        {
            if (!rabbitMQPersistentConnection.IsConnected)
            {
                rabbitMQPersistentConnection.TryConnect();
            }
            var policy = RetryPolicy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(RETRYCOUNT, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    logger.LogWarning(ex.ToString());
                });

            using (var channel = rabbitMQPersistentConnection.CreateModel())
            {

                //普通交换机
                channel.ExchangeDeclare(message.ExchangeName + ".ttl", "direct", true, false);
                //DLX
                channel.ExchangeDeclare(message.ExchangeName + ".delay", "direct", true, false);

                var dlxArgs = new Dictionary<string, object>()
                {
                    { "x-dead-letter-exchange", message.ExchangeName + ".delay"},
                    { "x-dead-letter-routing-key", message.RoutingKey + ".delay"}
                };


                channel.QueueDeclare(message.QueueName + ".ttl", true, false, false, dlxArgs);
                channel.QueueBind(message.ExchangeName + ".ttl", message.QueueName + ".ttl", message.RoutingKey + ".ttl");


                channel.QueueDeclare(message.QueueName + ".delay", true, false, false);
                channel.QueueBind(message.QueueName + ".delay", message.ExchangeName + ".delay", message.RoutingKey + ".delay");

                policy.Execute(() =>
                {
                    channel.ConfirmSelect();
                    var properties = channel.CreateBasicProperties();
                    properties.MessageId = message.MessageId.ToString();
                    properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                    properties.Persistent = true;
                    properties.Expiration = "60000";

                    channel.BasicPublish(message.ExchangeName + ".ttl", message.RoutingKey + ".ttl", properties, message.Body);

                });

            }
        }

    }
}
