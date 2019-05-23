using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService
{
    /// <summary>
    /// 消息体的设计
    /// </summary>
    public class Message
    {

        public string ExchangeName { get; set; }

        public string QueueName { get; set; }

        public string RoutingKey { get; set; }

        /// <summary>
        /// 消息id
        /// </summary>
        public long MessageId { get; set; }

        /// <summary>
        /// 消息头的设置
        /// </summary>
        public IDictionary<string, object> Headers { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public byte[] Body { get; set; }



    }
}
