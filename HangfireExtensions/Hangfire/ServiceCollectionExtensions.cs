using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.MySql.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HangfireExtensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMysqlHangfire(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
                throw new ArgumentException(nameof(services));

            if (configuration == null)
                throw new ArgumentException(nameof(configuration));

            var hangfireMysql =new HangfireOption(configuration);



            services.AddHangfire(x => {
                x.UseStorage(new MySqlStorage(hangfireMysql.MySql,
                    new MySqlStorageOptions
                    {
                        TablePrefix = "log",
                        TransactionIsolationLevel = IsolationLevel.ReadCommitted,//实物隔离级别，默认为读取已提交
                        QueuePollInterval = TimeSpan.FromSeconds(1),//队列检测频率，秒级任务需要配置短点，一般任务可以配置默认时间
                        JobExpirationCheckInterval = TimeSpan.FromHours(1),//作业到期检查间隔（管理过期记录）。默认值为1小时
                        CountersAggregateInterval = TimeSpan.FromMinutes(5),//聚合计数器的间隔。默认为5分钟
                        PrepareSchemaIfNecessary = true,//设置true，则会自动创建表
                        DashboardJobListLimit = 50000,//仪表盘作业列表展示条数限制
                        TransactionTimeout = TimeSpan.FromMinutes(1),//事务超时时间，默认一分钟
                    }));
            });
            return services;
        }
    }
}
