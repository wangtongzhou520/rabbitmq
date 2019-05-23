using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace OrderDal
{
    /// <summary>
    /// 主要添加多个数据库db
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlSugarDbContext(this IServiceCollection services, IConfiguration configuration,ServiceLifetime serviceLifetime=ServiceLifetime.Scoped)
        {
            if (services == null)
                throw new ArgumentException(nameof(services));

            if (configuration == null)
                throw new ArgumentException(nameof(configuration));

            var dbConfigurations = configuration.GetSection("DbConfig").Get<List<DbConnectionOption>>();

            if (dbConfigurations != null)
            {
                foreach (var dbConfiguration in dbConfigurations)
                {
                    if (serviceLifetime == ServiceLifetime.Scoped)
                    {
                        services.AddScoped(x=> {
                            return new DbSqlSugarClient(new ConnectionConfig
                            {
                                ConnectionString=dbConfiguration.ConnectionString,
                                DbType=dbConfiguration.DbType,
                                IsAutoCloseConnection=dbConfiguration.IsAutoCloseConnection,
                                InitKeyType=InitKeyType.Attribute
                            }, dbConfiguration.Name, dbConfiguration.Default);
                        });
                    }

                    if (serviceLifetime == ServiceLifetime.Singleton)
                    {
                        services.AddSingleton(x => {
                            return new DbSqlSugarClient(new ConnectionConfig
                            {
                                ConnectionString =dbConfiguration.ConnectionString,
                                DbType=dbConfiguration.DbType,
                                IsAutoCloseConnection=dbConfiguration.IsAutoCloseConnection,
                                InitKeyType=InitKeyType.Attribute
                            },dbConfiguration.Name,dbConfiguration.Default);
                        });
                    }


                    if (serviceLifetime == ServiceLifetime.Transient)
                    {
                        services.AddTransient(x => {
                            return new DbSqlSugarClient(new ConnectionConfig
                            {
                                ConnectionString = dbConfiguration.ConnectionString,
                                DbType=dbConfiguration.DbType,
                                IsAutoCloseConnection=dbConfiguration.IsAutoCloseConnection,
                                InitKeyType=InitKeyType.Attribute
                            },dbConfiguration.Name,dbConfiguration.Default);
                        });
                    }
                }
            }

            return services;

        }
    }
}
