using System;
using System.Collections.Generic;
using System.Text;

namespace HangfireExtensions
{
    public class HangFireActivator: Hangfire.JobActivator
    {

        private readonly IServiceProvider _serviceProvider;
        public HangFireActivator(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public override object ActivateJob(Type jobType)
        {
            return _serviceProvider.GetService(jobType);
        }
    }
}
