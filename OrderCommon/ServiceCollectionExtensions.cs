using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace OrderCommon
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 通过程序集和结尾加载注入服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly"></param>
        /// <param name="endWith"></param>
        /// <param name="serviceLifetime"></param>
        /// <returns></returns>
        public static IServiceCollection BatchRegisterService(this IServiceCollection services, Assembly assembly, string endWith, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        {
            IEnumerable<Type> typeList = assembly.GetTypes().Where(t => !t.IsInterface && !t.IsSealed && !t.IsAbstract && t.Name.EndsWith(endWith));  // 所有符合注册条件的类集合
            if (typeList.Count() == 0)
                return services;

            var typeDic = new Dictionary<Type, Type[]>(); // 待注册集合
            foreach (var type in typeList)
            {
                var interfaces = type.GetInterfaces();   // 获取接口
                typeDic.Add(type, interfaces);
            }
            if (typeDic.Keys.Count() > 0)
            {
                foreach (var instanceType in typeDic.Keys)
                {
                    foreach (var interfaceType in typeDic[instanceType])
                    {
                        //根据指定的生命周期进行注册
                        switch (serviceLifetime)
                        {
                            case ServiceLifetime.Scoped:
                                services.AddScoped(interfaceType, instanceType);
                                break;
                            case ServiceLifetime.Singleton:
                                services.AddSingleton(interfaceType, instanceType);
                                break;
                            case ServiceLifetime.Transient:
                                services.AddTransient(interfaceType, instanceType);
                                break;
                        }
                    }
                }
            }
            return services;
        }


        /// <summary>
        /// 通过程序集加载
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblys"></param>
        /// <param name="serviceLifetime"></param>
        /// <returns></returns>
        public static IServiceCollection BatchRegisterService(this IServiceCollection services, Assembly[] assemblys, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        {
            List<Type> typeList = new List<Type>();  // 所有符合注册条件的类集合
            foreach (var assembly in assemblys)
            {
                // 筛选当前程序集下符合条件的类
                var types = assembly.GetTypes().Where(t => !t.IsInterface && !t.IsSealed && !t.IsAbstract);
                if (types != null && types.Count() > 0)
                    typeList.AddRange(types);
            }
            if (typeList.Count() == 0)
                return services;

            var typeDic = new Dictionary<Type, Type[]>(); //待注册集合
            foreach (var type in typeList)
            {
                var interfaces = type.GetInterfaces();   //获取接口
                typeDic.Add(type, interfaces);
            }
            if (typeDic.Keys.Count() > 0)
            {
                foreach (var instanceType in typeDic.Keys)
                {
                    foreach (var interfaceType in typeDic[instanceType])
                    {
                        //根据指定的生命周期进行注册
                        switch (serviceLifetime)
                        {
                            case ServiceLifetime.Scoped:
                                services.AddScoped(interfaceType, instanceType);
                                break;
                            case ServiceLifetime.Singleton:
                                services.AddSingleton(interfaceType, instanceType);
                                break;
                            case ServiceLifetime.Transient:
                                services.AddTransient(interfaceType, instanceType);
                                break;
                        }
                    }
                }
            }
            return services;
        }
    }
}
