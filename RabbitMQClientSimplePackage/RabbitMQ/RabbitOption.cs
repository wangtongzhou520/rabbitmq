using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace RabbitMQExtensions
{
    /// <summary>
    /// 连接字符串类
    /// </summary>
    public class RabbitOption
    {
        public RabbitOption(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentException(nameof(config));

            var section = config.GetSection("rabbit");
            section.Bind(this);
        }

        public string Uri { get; set; }
    }
}
