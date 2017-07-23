﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Surging.Core.Caching.Configurations;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.DotNetty;
using Surging.Core.ProxyGenerator.Utilitys;
using Surging.Core.System.Ioc;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Surging.Core.EventBusRabbitMQ;
using Surging.Core.EventBusRabbitMQ.Configurations;
using Surging.IModuleServices.Common.Models.Events;
using Surging.Core.CPlatform.EventBus;
using Surging.Modules.Common.IntegrationEvents.EventHandling;

namespace Surging.Services.Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();
            var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory);
            ConfigureEventBus(config);
            ConfigureLogging(services);
            builder.Populate(services);
            ConfigureService(builder);
            ServiceLocator.Current = builder.Build();
            ConfigureCache(config);
            ServiceLocator.GetService<ILoggerFactory>()
                   .AddConsole((c, l) => (int)l >= 3);
            ConfigureRoutes();
            ServiceLocator.GetService<ISubscriptionAdapt>().SubscribeAt();
            var d = ServiceLocator.GetService<UserLoginDateChangeHandler>();
            StartService();
            Console.ReadLine();
        }

        /// <summary>
        /// 配置相关服务
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        private static void ConfigureService(ContainerBuilder builder)
        {
            builder.Initialize();
            builder.RegisterServices();
            builder.RegisterRepositories();
            builder.RegisterModules();
            builder.RegisterServiceBus();
            builder.AddCoreServce()
                 .AddServiceRuntime()
                 .UseSharedFileRouteManager("c:\\routes.txt")//配置本地路由文件路径
                 .UseDotNettyTransport().UseRabbitMQTransport().AddRabbitMqAdapt();//配置Netty
            builder.Register(p => new CPlatformContainer(ServiceLocator.Current));
        }

        /// <summary>
        /// 配置日志服务
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureLogging(IServiceCollection services)
        {
            services.AddLogging();
        }

        public static void ConfigureEventBus(IConfigurationBuilder build)
        {
            build
            .AddEventBusFile("eventBusSettings.json", optional: false);
        }

        /// <summary>
        /// 配置缓存服务
        /// </summary>
        public static void ConfigureCache(IConfigurationBuilder build)
        {
            build
              .AddCacheFile("cacheSettings.json", optional: false);
        }

        /// <summary>
        ///添加路由列表， 有利于测试，
        /// </summary>
        public static void ConfigureRoutes()
        {
            var serviceEntryManager = ServiceLocator.GetService<IServiceEntryManager>();
            var addressDescriptors = serviceEntryManager.GetEntries().Select(i =>
            new ServiceRoute
            {
                Address = new[] { new IpAddressModel { Ip = "127.0.0.1", Port = 98 } },
                ServiceDescriptor = i.Descriptor
            }).ToList();
            var serviceRouteManager = ServiceLocator.GetService<IServiceRouteManager>();
            serviceRouteManager.SetRoutesAsync(addressDescriptors).Wait();
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public static void StartService()
        {
            var serviceHost = ServiceLocator.GetService<IServiceHost>();
            Task.Factory.StartNew(async () =>
            {
                await serviceHost.StartAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 98));
                Console.WriteLine($"服务端启动成功，{DateTime.Now}。");
            }).Wait();
        }
    }
}