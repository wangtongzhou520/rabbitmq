using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace RabbitMQExtensions
{
    public class DefaultRabbitMQPersistentConnection : IRabbitMQPersistentConnection
    {
        private readonly IConnectionFactory connectionFactory;
        private readonly ILogger<DefaultRabbitMQPersistentConnection> logger;

        private IConnection connection;

        private const int RETTRYCOUNT = 6;

        private static readonly object lockObj = new object();
        public DefaultRabbitMQPersistentConnection(IConnectionFactory connectionFactory, ILogger<DefaultRabbitMQPersistentConnection> logger)
        {
            this.connectionFactory = connectionFactory;
            this.logger = logger;
        }

        public bool IsConnected
        {
            get
            {
                return connection != null && connection.IsOpen;
            }
        }

        public void Cleanup()
        {
            try
            {
                connection.Dispose();
                connection.Close();
                connection = null;

            }
            catch (IOException ex)
            {
                logger.LogCritical(ex.ToString());
            }
        }

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                connection.Close();
                throw new InvalidOperationException("连接不到rabbitmq");
            }
            return connection.CreateModel();
        }

        public bool TryConnect()
        {
            logger.LogInformation("RabbitMQ客户端尝试连接");

            lock (lockObj)
            {
                if (connection == null)
                {
                    var policy = RetryPolicy.Handle<SocketException>()
                        .Or<BrokerUnreachableException>()
                        .WaitAndRetry(RETTRYCOUNT, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>    
                        {
                            logger.LogWarning(ex.ToString());
                        });

                    policy.Execute(() =>
                    {
                        connection = connectionFactory.CreateConnection();
                    });
                }



                if (IsConnected)
                {
                    connection.ConnectionShutdown += OnConnectionShutdown;
                    connection.CallbackException += OnCallbackException;
                    connection.ConnectionBlocked += OnConnectionBlocked;

                    logger.LogInformation($"RabbitMQ{connection.Endpoint.HostName}获取了连接");

                    return true;
                }
                else
                {
                    logger.LogCritical("无法创建和打开RabbitMQ连接");

                    return false;
                }
            }
        }


        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {

            logger.LogWarning("RabbitMQ连接异常,尝试重连...");

            Cleanup();
            TryConnect();
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {

            logger.LogWarning("RabbitMQ连接异常,尝试重连...");

            Cleanup();
            TryConnect();
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {

            logger.LogWarning("RabbitMQ连接异常,尝试重连...");

            Cleanup();
            TryConnect();
        }
    }
}
