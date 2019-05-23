using System;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.Autofac;
using HangfireExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using OrderCommon;
using OrderDal;
using OrderService;
using RabbitMQExtensions;

namespace HangFire.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IContainer AutofacContainer { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddMysqlHangfire(Configuration);

            services.AddLogging();

            //DI注入
            services.BatchRegisterService(Assembly.Load("OrderDal"), "Dal", ServiceLifetime.Transient);

            services.BatchRegisterService(Assembly.Load("OrderService"), "Service", ServiceLifetime.Transient);

            services.BatchRegisterService(Assembly.Load("HangfireExtensions"), "Job", ServiceLifetime.Transient);


            // 添加数据ORM
            services.AddSqlSugarDbContext(Configuration);

            //rabbitMQ注入
            services.AddeRabbitMQConnection(Configuration);


            //添加autofac容器替换
            var autofac_builder = new ContainerBuilder();
            autofac_builder.Populate(services);
            autofac_builder.RegisterModule<AutofacModuleRegister>();
            AutofacContainer = autofac_builder.Build();
            return new AutofacServiceProvider(AutofacContainer);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,IServiceProvider serviceProvider)
        {
            GlobalConfiguration.Configuration.UseActivator<HangFireActivator>(new HangFireActivator(serviceProvider));
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //添加nlog
            loggerFactory.AddNLog();
            env.ConfigureNLog("nlog.config");

            app.UseHangfireServer();
            app.UseHangfireDashboard();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }


        /// <summary>
        /// Autofac扩展注册
        /// </summary>
        public class AutofacModuleRegister : Autofac.Module
        {
            /// <summary>
            /// 装载autofac方式注册
            /// </summary>
            /// <param name="builder"></param>
            protected override void Load(ContainerBuilder builder)
            {
                // 数据仓储泛型注册
                builder.RegisterGeneric(typeof(BaseDal<>)).As(typeof(IBaseDal<>))
                    .InstancePerLifetimeScope();
                builder.RegisterGeneric(typeof(MessageService<>)).As(typeof(IMessageService<>))
                    .InstancePerLifetimeScope();
            }
        }
    }
}
