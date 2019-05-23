using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using OrderDal;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQExtensions;

namespace OrderService
{
    public class ConsumerMessageService: IConsumerMessageService
    {
        private readonly IRabbitMQPersistentConnection rabbitMQPersistentConnection;
        private readonly ILogger<ConsumerMessageService> logger;
        
        private IModel consumerchannel;
        private const int RETRYCOUNT = 6;

        public ConsumerMessageService(IRabbitMQPersistentConnection rabbitMQPersistentConnection,ILogger<ConsumerMessageService> logger)
        {
            this.rabbitMQPersistentConnection = rabbitMQPersistentConnection;
            this.logger = logger;
        }


        public void ConsumerMessage()
        {
            if (!rabbitMQPersistentConnection.IsConnected)
            {
                rabbitMQPersistentConnection.TryConnect();
            }

            consumerchannel = rabbitMQPersistentConnection.CreateModel();

            consumerchannel.ExchangeDeclare("order", "direct", true, false);
            consumerchannel.QueueDeclare("order", true, false, false);
            consumerchannel.QueueBind("order", "order", "order");

            var retryArgs = new Dictionary<string, object>
                    {
                        { "x-dead-letter-exchange", "order"},
                        { "x-dead-letter-routing-key", "order"}
                    };

            consumerchannel.ExchangeDeclare("order" + ".retry", "direct", true, false);
            consumerchannel.QueueDeclare("order" + ".retry", true, false, false, retryArgs);
            consumerchannel.QueueBind("order" + ".retry", "order" + ".retry", "order" + ".retry");


            var consumer = new EventingBasicConsumer(consumerchannel);

            consumer.Received += OnConsumerMessage;
            consumerchannel.BasicConsume("order", false, consumer);
            consumerchannel.CallbackException += OnConsumerMessageException;
        }
        /// <summary>
        /// 消费回调以后的异常函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ea"></param>
        private void OnConsumerMessageException(object sender, CallbackExceptionEventArgs ea)
        {
            consumerchannel.Close();
            consumerchannel = null;
            var mres = new ManualResetEventSlim(false);

            while (!mres.Wait(3000))
            {
                try
                {
                    if (!rabbitMQPersistentConnection.IsConnected)
                    {
                        rabbitMQPersistentConnection.TryConnect();
                    }

                    consumerchannel = rabbitMQPersistentConnection.CreateModel();

                    consumerchannel.ExchangeDeclare("order", "direct", true, false);
                    consumerchannel.QueueDeclare("order", true, false, false);
                    consumerchannel.QueueBind("order", "order", "order");

                    var retryArgs = new Dictionary<string, object>
                    {
                        { "x-dead-letter-exchange", "order"},
                        { "x-dead-letter-routing-key", "order"}
                    };

                    consumerchannel.ExchangeDeclare("order" + ".retry", "direct", true, false);
                    consumerchannel.QueueDeclare("order" + ".retry", true, false, false, retryArgs);
                    consumerchannel.QueueBind("order" + ".retry", "order" + ".retry", "order" + ".retry");


                    var consumer = new EventingBasicConsumer(consumerchannel);

                    consumer.Received += OnConsumerMessage;
                    consumerchannel.BasicConsume("order", false, consumer);
                    consumerchannel.CallbackException += OnConsumerMessageException;

                    mres.Set();
                }
                catch (Exception ex)
                {
                    throw new Exception("rabbitMQ宕机了", ex);
                }
            }
        }


        /// <summary>
        /// 接收到消息以后的回掉函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ea"></param>
        private void OnConsumerMessage(object sender, BasicDeliverEventArgs ea)
        {
            var retryCount = 0;

            //这个地方判断下消息消费的次数
            if (ea.BasicProperties.Headers != null && ea.BasicProperties.Headers.ContainsKey("retryCount"))
            {
                retryCount = (int)ea.BasicProperties.Headers["retryCount"];
                logger.LogWarning($"消息id:{ea.BasicProperties.MessageId}重试第{++retryCount}次开始");
            }
            try
            {
                //这里可以消息都做一些具体判断去分发消费消息
                //剩下怎么去实现自己做一下脑补
                logger.LogDebug($"订单下单成功{ea.BasicProperties.MessageId}");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "消费异常");
                //这个地方消费者处理失败的时候
                //发送消息到重试交换机
                if (IsRetry(retryCount))
                {
                    SetupRetry(retryCount, ea);
                }
                else
                {
                    //这块可以设计一个失败的交换机
                    //如果大于重试次数以后直接流向失败交换机
                    //到时候邮件提醒进行人工处理
                    //可以自行扩展一下
                }

            }
            consumerchannel.BasicAck(ea.DeliveryTag, false);
        }

        /// <summary>
        /// 重发机制
        /// </summary>
        /// <param name="retryCount"></param>
        /// <param name="ea"></param>
        private void SetupRetry(int retryCount,BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var properties = ea.BasicProperties;
            properties.Headers = properties.Headers = properties.Headers ?? new Dictionary<string, object>();
            properties.Headers["retryCount"] = retryCount;
            properties.Expiration = (1000 * 60 * 3).ToString();

            try
            {
                consumerchannel.BasicPublish("order" + ".retry", "order" + ".retry", properties, body);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "rabbitMQ已经关闭!");
                throw new Exception("rabbitMQ已经关闭", ex);
            }
        }

        /// <summary>
        /// 是否还需要重试
        /// </summary>
        /// <param name="retryCount"></param>
        /// <returns></returns>
        private bool IsRetry(int retryCount)
        {
            return retryCount < 3;
        }


        /// <summary>
        /// 订单消费并入库
        /// </summary>
        public void ConsumerMessageAndWriteMessageLog()
        {
            if (!rabbitMQPersistentConnection.IsConnected)
            {
                rabbitMQPersistentConnection.TryConnect();
            }
            consumerchannel = rabbitMQPersistentConnection.CreateModel();

            consumerchannel.ExchangeDeclare("order", "direct", true, false);
            consumerchannel.QueueDeclare("order", true, false, false);
            consumerchannel.QueueBind("order", "order", "order");

            //这里不在做正常消费下死信队列的设计
            //单纯只做发送到MQ中的操作
            var consumer = new EventingBasicConsumer(consumerchannel);
            consumer.Received += OnConsumerMessageAndWriteMessageLog;

            consumerchannel.BasicConsume("order", false, consumer);

            consumerchannel.CallbackException += OnOnConsumerMessageAndWriteMessageLogException;

        }

        private void OnOnConsumerMessageAndWriteMessageLogException(object sender, CallbackExceptionEventArgs ea)
        {
            consumerchannel.Dispose();
            consumerchannel.Close();
            consumerchannel = null;
            var mres = new ManualResetEventSlim(false); 

            while (!mres.Wait(3000)) 
            {
                try
                {
                    if (!rabbitMQPersistentConnection.IsConnected)
                    {
                        rabbitMQPersistentConnection.TryConnect();
                    }
                    consumerchannel = rabbitMQPersistentConnection.CreateModel();

                    consumerchannel.ExchangeDeclare("order", "direct", true, false);
                    consumerchannel.QueueDeclare("order", true, false, false);
                    consumerchannel.QueueBind("order", "order", "order");

                    var consumer = new EventingBasicConsumer(consumerchannel);
                    consumer.Received += OnConsumerMessageAndWriteMessageLog;

                    consumerchannel.BasicConsume("order", false, consumer);

                    consumerchannel.CallbackException += OnOnConsumerMessageAndWriteMessageLogException;

                    mres.Set();
                }
                catch (Exception ex)
                {
                    throw new Exception("rabbitMQ宕机了",ex);
                }
            }
        }

        /// <summary>
        /// 处理完成消息以后回调入库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConsumerMessageAndWriteMessageLog(object sender, BasicDeliverEventArgs ea)
        {
            try
            {
                //下游服务怎么去实现自己做一下脑补
                logger.LogDebug($"订单下单成功{ea.BasicProperties.MessageId}");
                //也可以进行下缓存记录方便延时队列回调检查
                //这里通过调用消息入库服务
            }
            catch (Exception ex)
            {
                //下游消费异常以后坐下记录
                logger.LogError(ex,$"消息{ea.BasicProperties.MessageId}消费异常");
            }

            consumerchannel.BasicAck(ea.DeliveryTag, false);
        }


        /// <summary>
        /// 消费延时发送消息
        /// </summary>
        public void ConsumerDelayMessage()
        {
            if (!rabbitMQPersistentConnection.IsConnected)
            {
                rabbitMQPersistentConnection.TryConnect();
            }

            consumerchannel = rabbitMQPersistentConnection.CreateModel();

            consumerchannel.ExchangeDeclare("order.delay", "direct", true, false);
            consumerchannel.QueueDeclare("order.delay", true, false, false);
            consumerchannel.QueueBind("order.delay", "order.delay", "order.delay");

            //消费延时队列中的消息
            var consumer = new EventingBasicConsumer(consumerchannel);
            consumer.Received += OnReceivedDelayMessage;

            consumerchannel.BasicConsume("order.delay", false, consumer);

            consumerchannel.CallbackException += OnReceivedDelayMessageException;

        }
        private void OnReceivedDelayMessageException(object sender, CallbackExceptionEventArgs ea)
        {
            consumerchannel.Close();
            consumerchannel = null;
            var mres = new ManualResetEventSlim(false);
            while (!mres.Wait(3000))
            {
                try
                {
                    if (!rabbitMQPersistentConnection.IsConnected)
                    {
                        rabbitMQPersistentConnection.TryConnect();
                    }

                    consumerchannel = rabbitMQPersistentConnection.CreateModel();

                    consumerchannel.ExchangeDeclare("order.delay", "direct", true, false);
                    consumerchannel.QueueDeclare("order.delay", true, false, false);
                    consumerchannel.QueueBind("order.delay", "order.delay", "order.delay");

                    //消费延时队列中的消息
                    var consumer = new EventingBasicConsumer(consumerchannel);
                    consumer.Received += OnReceivedDelayMessage;

                    consumerchannel.BasicConsume("order.delay", false, consumer);

                    consumerchannel.CallbackException += OnReceivedDelayMessageException;

                    mres.Set();
                }
                catch (Exception ex)
                {
                    throw new Exception("rabbitMQ宕机了", ex);
                }
            }
        }
        private void OnReceivedDelayMessage(object sender, BasicDeliverEventArgs ea)
        {
            try
            {
                logger.LogDebug($"延时发送的消息{ea.BasicProperties.MessageId}");
                var messageId = ea.BasicProperties.MessageId;
                //判断缓存或者数据库中的消息是否存在
                //如果存在则什么也不做
                //如果不存在就再次调用发送端再次发送消息

            }
            catch (Exception ex)
            {
                //下游消费异常以后坐下记录
                logger.LogError(ex, $"延时发送的消息消费{ea.BasicProperties.MessageId}异常");
            }

            try
            {
                consumerchannel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "rabbitMQ已经关闭!");
                throw new Exception("rabbitMQ已经关闭", ex);
            }
        }
    }
}
