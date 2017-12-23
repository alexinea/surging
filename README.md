# surging 　　　　　　　　　　　　　　　　　　　　[English](https://github.com/dotnetcore/surging/blob/master/README.EN.md)
[![Member project of .NET China Foundation](https://github.com/dotnetcore/Home/blob/master/icons/member-project-of-netchina.png)](https://github.com/dotnetcore)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://mit-license.org/)
# surging 是一个分布式微服务框架,提供高性能RPC远程服务调用，采用Zookeeper、Consul作为surging服务的注册中心，集成了哈希，随机，轮询作为负载均衡的算法，RPC集成采用的是netty框架，采用异步传输。

<br />

启动配置：

 <br/>
 
 ```c#
var host = new ServiceHostBuilder()
                .RegisterServices(builder =>
                {
                    builder.AddMicroService(option =>
                    {
                        option.AddServiceRuntime();//
                        // option.UseZooKeeperManager(new ConfigInfo("127.0.0.1:2181")); //使用Zookeeper管理
                        option.UseConsulManager(new ConfigInfo("127.0.0.1:8500"));//使用Consul管理
                        option.UseDotNettyTransport();//使用Netty传输
                        option.UseRabbitMQTransport();//使用rabbitmq 传输
                        option.AddRabbitMQAdapt();//基于rabbitmq的消费的服务适配
                      //  option.UseProtoBufferCodec();//基于protobuf序列化
                        option.UseMessagePackCodec();//基于MessagePack序列化
                        builder.Register(p => new CPlatformContainer(ServiceLocator.Current));//初始化注入容器
                    });
                })
                .SubscribeAt()     //消息订阅
                .UseServer("127.0.0.1", 98)
              //.UseServer("127.0.0.1", 98，“true”) //自动生成Token
              //.UseServer("127.0.0.1", 98，“123456789”) //固定密码Token
                .UseLog4net("Configs/log4net.config") //使用log4net记录日志
                .UseLog4net()  //使用log4net记录日志
                .UseStartup<Startup>()
                .Build();
                
            using (host.Run())
            {
                Console.WriteLine($"服务端启动成功，{DateTime.Now}。");
            }
 ```    
<br/>

服务路由访问配置：
<br/>

```c#
[ServiceBundle("api/{Service}")]
 ```    
<br/>

JWT验证，接口方法添加以下特性：
<br/>

```c#
   [Authorization(AuthType = AuthorizationType.JWT)];
 ```    
<br/>

AppSecret验证，接口方法添加以下特性：
<br/>

```c#
 [Authorization(AuthType = AuthorizationType.AppSecret)];
 ```    
<br/>

订阅功能：
<br/>

```c#
 ServiceLocator.GetService< ISubscriptionAdapt >().SubscribeAt();
 ```    
 
 <br/>
增加服务容错、服务容错降级、服务强制降级


* 增加容错策略Injection，脚本注入：

<br/>

```c#
[Command(Strategy= StrategyType.Injection ,Injection = @"return null;")]
```    

 <br/>
 
```C#  
[Command(Strategy= StrategyType.Injection ,Injection = @"return 
Task.FromResult(new Surging.IModuleServices.Common.Models.UserModel
         {
            Name=""fanly"",
            Age=18
         });",InjectionNamespaces =new string[] { "Surging.IModuleServices.Common"})] 
```


* 增加容错策略Injection，本地模块注入：   

<br/>

```C#  
[Command(Strategy= StrategyType.Injection ,Injection = @"return true;")] 
```

<br/>

增加缓存降级，怎么使用？
<br/>
在业务接口方法上添加如下特性
<br/>

```C#  
   [Command(Strategy= StrategyType.Failover,FailoverCluster =3,RequestCacheEnabled =true)]  //RequestCacheEnabled =true 就是启用缓存
```

<br/>
怎么拦截获取缓存
 <br/>
在业务接口方法上添加如下特性
 <br/>
 
```C#  
 [InterceptMethod(CachingMethod.Get, Key = "GetUser_id_{0}", Mode = CacheTargetType.Redis, Time = 480)]
```
    
<br/>
怎么拦截删除缓存
 <br/>
在业务接口方法上添加如下特性
 <br/>
 
```C#  
  [InterceptMethod(CachingMethod.Remove, "GetUser_id_{0}", "GetUserName_name_{0}", Mode = CacheTargetType.Redis)]
```
      
<br/>
怎么添加缓存KEY
   <br/>
在业务模型属性上添加，如下特性，可以支持多个
   <br/>
   
```C# 
[CacheKey(1)]
```
        
<br/>
配置拦截器
<br/>
   
```C# 
 .AddClientIntercepted(typeof(CacheProviderInterceptor))
```

<br/>

[简单示例](https://github.com/dotnetcore/surging/blob/master/docs/docs.en/INDEX.md)


<br/>


IDE:Visual Studio 2017 15.3 Preview ,vscode
<br/>
框架：.NET core 2.0
<br/>
如有任何问题可以加入QQ群：615562965
<br/>
[博客园]:https://www.cnblogs.com/fanliang11
