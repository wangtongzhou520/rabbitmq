using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace HangfireExtensions
{
    public class HangfireOption
    {
        public HangfireOption(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentException(nameof(config));

            var section = config.GetSection("hangfire");
            section.Bind(this);
        }

        public string MySql { get; set; }
    }
}
