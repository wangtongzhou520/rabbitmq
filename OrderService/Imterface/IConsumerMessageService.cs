using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace OrderService
{
    public interface IConsumerMessageService
    {
        void ConsumerMessage();

        void ConsumerMessageAndWriteMessageLog();

        void ConsumerDelayMessage();
    }
}
