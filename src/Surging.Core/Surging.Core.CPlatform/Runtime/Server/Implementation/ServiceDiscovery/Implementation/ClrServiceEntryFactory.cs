using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.DependencyResolution;
using Surging.Core.CPlatform.Filters.Implementation;
using Surging.Core.CPlatform.Ids;
using Surging.Core.CPlatform.Routing.Template;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Surging.Core.CPlatform.Utilities.FastInvoke;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Implementation
{
    /// <summary>
    /// Clr������Ŀ������
    /// </summary>
    public class ClrServiceEntryFactory : IClrServiceEntryFactory
    {
        #region �ֶ�

        /// <summary>
        /// Defines the _serviceIdGenerator
        /// </summary>
        private readonly IServiceIdGenerator _serviceIdGenerator;

        /// <summary>
        /// Defines the _serviceProvider
        /// </summary>
        private readonly CPlatformContainer _serviceProvider;

        /// <summary>
        /// Defines the _typeConvertibleService
        /// </summary>
        private readonly ITypeConvertibleService _typeConvertibleService;

        #endregion �ֶ�

        #region ���캯��

        /// <summary>
        /// Initializes a new instance of the <see cref="ClrServiceEntryFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">The serviceProvider<see cref="CPlatformContainer"/></param>
        /// <param name="serviceIdGenerator">The serviceIdGenerator<see cref="IServiceIdGenerator"/></param>
        /// <param name="typeConvertibleService">The typeConvertibleService<see cref="ITypeConvertibleService"/></param>
        public ClrServiceEntryFactory(CPlatformContainer serviceProvider, IServiceIdGenerator serviceIdGenerator, ITypeConvertibleService typeConvertibleService)
        {
            _serviceProvider = serviceProvider;
            _serviceIdGenerator = serviceIdGenerator;
            _typeConvertibleService = typeConvertibleService;
        }

        #endregion ���캯��

        #region ����

        /// <summary>
        /// ����������Ŀ��
        /// </summary>
        /// <param name="service">�������͡�</param>
        /// <returns>������Ŀ���ϡ�</returns>
        public IEnumerable<ServiceEntry> CreateServiceEntry(Type service)
        {
            var routeTemplate = service.GetCustomAttribute<ServiceBundleAttribute>();
            foreach (var methodInfo in service.GetTypeInfo().GetMethods())
            {
                var serviceRoute = methodInfo.GetCustomAttribute<ServiceRouteAttribute>();
                var routeTemplateVal = routeTemplate.RouteTemplate;
                if (!routeTemplate.IsPrefix && serviceRoute != null)
                    routeTemplateVal = serviceRoute.Template;
                else if (routeTemplate.IsPrefix && serviceRoute != null)
                    routeTemplateVal = $"{ routeTemplate.RouteTemplate}/{ serviceRoute.Template}";
                yield return Create(methodInfo, service.Name, routeTemplateVal);
            }
        }

        /// <summary>
        /// The Create
        /// </summary>
        /// <param name="method">The method<see cref="MethodInfo"/></param>
        /// <param name="serviceName">The serviceName<see cref="string"/></param>
        /// <param name="routeTemplate">The routeTemplate<see cref="string"/></param>
        /// <returns>The <see cref="ServiceEntry"/></returns>
        private ServiceEntry Create(MethodInfo method, string serviceName, string routeTemplate)
        {
            var serviceId = _serviceIdGenerator.GenerateServiceId(method);
            var attributes = method.GetCustomAttributes().ToList();
            var serviceDescriptor = new ServiceDescriptor
            {
                Id = serviceId,
                RoutePath = RoutePatternParser.Parse(routeTemplate, serviceName, method.Name)
            };
            var descriptorAttributes = method.GetCustomAttributes<ServiceDescriptorAttribute>();
            foreach (var descriptorAttribute in descriptorAttributes)
            {
                descriptorAttribute.Apply(serviceDescriptor);
            }
            var authorization = attributes.Where(p => p is AuthorizationFilterAttribute).FirstOrDefault();
            if (authorization != null)
                serviceDescriptor.EnableAuthorization(true);
            if (authorization != null)
            {
                serviceDescriptor.AuthType(((authorization as AuthorizationAttribute)?.AuthType)
                    ?? AuthorizationType.AppSecret);
            }
            var fastInvoker = GetHandler(serviceId, method);
            return new ServiceEntry
            {
                Descriptor = serviceDescriptor,
                RoutePath = serviceDescriptor.RoutePath,
                MethodName = method.Name,
                Type = method.DeclaringType,
                Attributes = attributes,
                Func = (key, parameters) =>
             {
                 object instance = null;
                 if (AppConfig.ServerOptions.IsModulePerLifetimeScope)
                     instance = _serviceProvider.GetInstancePerLifetimeScope(key, method.DeclaringType);
                 else
                     instance = _serviceProvider.GetInstances(key, method.DeclaringType);
                 var list = new List<object>();

                 foreach (var parameterInfo in method.GetParameters())
                 {
                     //�����Ƿ���Ĭ��ֵ���жϣ���Ĭ��ֵ�������û�û����ȡĬ��ֵ
                     if (parameterInfo.HasDefaultValue && !parameters.ContainsKey(parameterInfo.Name))
                     {
                         list.Add(parameterInfo.DefaultValue);
                         continue;
                     }
                     var value = parameters[parameterInfo.Name];
                     var parameterType = parameterInfo.ParameterType;
                     var parameter = _typeConvertibleService.Convert(value, parameterType);
                     list.Add(parameter);
                 }
                 var result = fastInvoker(instance, list.ToArray());
                 return Task.FromResult(result);
             }
            };
        }

        /// <summary>
        /// The GetHandler
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="method">The method<see cref="MethodInfo"/></param>
        /// <returns>The <see cref="FastInvokeHandler"/></returns>
        private FastInvokeHandler GetHandler(string key, MethodInfo method)
        {
            var objInstance = ServiceResolver.Current.GetService(null, key);
            if (objInstance == null)
            {
                objInstance = FastInvoke.GetMethodInvoker(method);
                ServiceResolver.Current.Register(key, objInstance, null);
            }
            return objInstance as FastInvokeHandler;
        }

        #endregion ����
    }
}