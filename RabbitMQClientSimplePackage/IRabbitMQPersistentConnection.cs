using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace RabbitMQExtensions
{
    /// <summary>
    /// rabbitmq持久化连接
    /// </summary>
    public interface IRabbitMQPersistentConnection
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}
