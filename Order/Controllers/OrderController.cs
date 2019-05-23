using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderCommon;
using OrderDal;
using OrderService;
using RabbitMQ.Client;
using RabbitMQExtensions;

namespace Order.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IBaseDal<OrderMessageLogEntity> orderBaseDal;

        private readonly IMessageService<OrderMessageLogEntity> messageService;

        private readonly IConsumerMessageService consumerMessageService;

        private const string EXCHANGENAME = "order";

        private const string QUEUENAME = "order";

        private const string ROUTINGKEY = "order";


        public OrderController(IBaseDal<OrderMessageLogEntity> orderBaseDal, IMessageService<OrderMessageLogEntity> messageService,IConsumerMessageService consumerMessageService)
        {
            this.orderBaseDal = orderBaseDal;
            this.messageService = messageService;
            this.consumerMessageService = consumerMessageService;
        }

        /// <summary>
        /// 创建订单
        /// </summary>
        /// <returns></returns>
        public ActionResult<bool> CreateOrder(long userId)
        {
            //创建订单成功
            OrderEntity orderEntity = new OrderEntity();
            Random random= new Random();
            orderEntity.OrderId = random.Next();
            orderEntity.OrderNo = random.Next();
            orderEntity.UserId = userId;
            orderEntity.OrderInfo = random.Next() + "详情";
            //bool isCreateOrderSuccress = orderService.CreateOrder(orderId);
            //if (!isCreateOrderSuccress)
            //{
            //    throw new Exception("创建订单失败");
            //}
            //创建订单成功以后开始入消息记录库
            //消息建议设计的冗余一些方便以后好查询
            //千万级以后连表太困难
            //建议冗余的信息有用户信息、订单信息、方便以后按照这个核对信息
            //消息表的建议是按照不同的业务进行分表存储
            Random messageRandom = new Random();
            OrderMessageLogEntity orderMessageLog = new OrderMessageLogEntity();
            orderMessageLog.MessageId = messageRandom.Next();
            orderMessageLog.MessageInfo = orderEntity.OrderId+"订单信息";
            orderMessageLog.Status = (int)MessageStatusEnum.SENDING;
            orderMessageLog.OrderId = orderEntity.OrderId;
            orderMessageLog.UserId = orderEntity.UserId;
            orderMessageLog.CreateTime = DateTime.Now;
            orderMessageLog.UpdateTime = DateTime.Now;
            orderMessageLog.TryCount = 0;
            orderMessageLog.NextRetryTime = DateTime.Now.AddMinutes(5);
            //必须保证消息先落库
            bool isCreateOrderMessageLosSuccess = orderBaseDal.Insert(orderMessageLog);
            if (!isCreateOrderMessageLosSuccess)
                throw new Exception("消息入库异常");

            Message message = new Message();
            message.ExchangeName = EXCHANGENAME;
            message.QueueName = QUEUENAME;
            message.MessageId = orderMessageLog.MessageId;
            message.RoutingKey = ROUTINGKEY;
            message.Body = Encoding.UTF8.GetBytes(orderMessageLog.MessageInfo);


            //落库成功以后开始发送消息到MQ
            messageService.SendMessage(message, orderMessageLog);

            //发送延时消息
            //messageService.SendDelayMessage(message, orderMessageLog);


            return true;
        }


        /// <summary>
        /// 消费订单
        /// </summary>
        /// <returns></returns>
        public ActionResult<bool> ConsumerOrder()
        {
            Message message = new Message();
            message.ExchangeName = EXCHANGENAME;
            message.QueueName = QUEUENAME;
            message.RoutingKey = ROUTINGKEY;

            consumerMessageService.ConsumerMessage();

            return true;
        }



        /// <summary>
        /// 通过延时队列发送消息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ActionResult<bool> CreateDelayCreateOrder(long userId)
        {
            //创建订单成功
            OrderEntity orderEntity = new OrderEntity();
            Random random = new Random();
            orderEntity.OrderId = random.Next();
            orderEntity.OrderNo = random.Next();
            orderEntity.UserId = userId;
            orderEntity.OrderInfo = random.Next() + "详情";
            //bool isCreateOrderSuccress = orderService.CreateOrder(orderId);
            //if (!isCreateOrderSuccress)
            //{
            //    throw new Exception("创建订单失败");
            //}
            //创建订单成功以后开始入消息记录库
            //消息建议设计的冗余一些方便以后好查询
            //千万级以后连表太困难
            //建议冗余的信息有用户信息、订单信息、方便以后按照这个核对信息
            //消息表的建议是按照不同的业务进行分表存储
            Random messageRandom = new Random();
            OrderMessageLogEntity orderMessageLog = new OrderMessageLogEntity();
            orderMessageLog.MessageId = messageRandom.Next();
            orderMessageLog.MessageInfo = orderEntity.OrderId + "订单信息";
            orderMessageLog.Status = (int)MessageStatusEnum.SENDING;
            orderMessageLog.OrderId = orderEntity.OrderId;
            orderMessageLog.UserId = orderEntity.UserId;
            orderMessageLog.CreateTime = DateTime.Now;
            orderMessageLog.UpdateTime = DateTime.Now;
            orderMessageLog.TryCount = 0;
            orderMessageLog.NextRetryTime = DateTime.Now.AddMinutes(5);
            ////必须保证消息先落库
            //bool isCreateOrderMessageLosSuccess = orderBaseDal.Insert(orderMessageLog);
            //if (!isCreateOrderMessageLosSuccess)
            //    throw new Exception("消息入库异常");

            Message message = new Message();
            message.ExchangeName = EXCHANGENAME;
            message.QueueName = QUEUENAME;
            message.MessageId = orderMessageLog.MessageId;
            message.RoutingKey = ROUTINGKEY;
            message.Body = Encoding.UTF8.GetBytes(orderMessageLog.MessageInfo);

            //这里的设计是不进行落库
            //假如两条消息都失败必须借助定时任务去对比消息库和订单库的消息id然后进行再补发
            //剩下的只要有一条发送成功其实就能保证下游必然会消费调这条消息,排除下游消费异常的情况 这个地方我不在进行实现自己可脑补一下
            //开始发送消息到MQ
            messageService.SendMessage(message, orderMessageLog);

            //发送延时消息
            messageService.SendDelayMessage(message, orderMessageLog);

            return true;

        }


        public ActionResult<bool> ConsumerOrderAndWirteMessageLog()
        {
            consumerMessageService.ConsumerMessageAndWriteMessageLog();

            return true;
        }


        /// <summary>
        /// 消费延时消息
        /// </summary>
        /// <returns></returns>
        public ActionResult<bool> ConsumerDelayOrder()
        {
            consumerMessageService.ConsumerDelayMessage();

            return true;
        }
    }
}