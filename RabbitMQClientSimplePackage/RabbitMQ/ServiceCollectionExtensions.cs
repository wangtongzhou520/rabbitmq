using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQExtensions;

namespace RabbitMQExtensions
{
    /// <summary>
    /// 通过IOC创建单例的RabbitMQ的客户端
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddeRabbitMQConnection(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
                throw new ArgumentException(nameof(services));

            if (configuration == null)
                throw new ArgumentException(nameof(configuration));

            var rabbitOption = new RabbitOption(configuration);

            var factory = new ConnectionFactory{ Uri = new Uri(rabbitOption.Uri),AutomaticRecoveryEnabled=true,NetworkRecoveryInterval=TimeSpan.FromSeconds(60)};

            services.AddSingleton<IRabbitMQPersistentConnection>(x =>
            {
                var logger = x.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
                return new DefaultRabbitMQPersistentConnection(factory, logger);
            });

            return services;
        }
    }
}
