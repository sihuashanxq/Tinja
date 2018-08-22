using System;
using Tinja.Abstractions;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Configurations;
using Tinja.Abstractions.DynamicProxy.Executions;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Core.DynamicProxy.Configurations;
using Tinja.Core.DynamicProxy.Executions;
using Tinja.Core.DynamicProxy.Metadatas;
using Tinja.Core.Extensions;

namespace Tinja.Core.DynamicProxy
{
    public static class DynamicProxyExtensions
    {
        public static IContainer UseDynamicProxy(this IContainer container, Action<IDynamicProxyConfiguration> configurator = null)
        {
            if (container == null)
            {
                throw new NullReferenceException(nameof(container));
            }

            var configuration = new DynamicProxyConfiguration();
            configurator?.Invoke(configuration);

            container.AddTransient<MethodInvocationInvokerBuilder>();
            container.AddScoped<IInterceptorFactory, InterceptorFactory>();
            container.AddSingleton<IProxyTypeFactory, ProxyTypeFactory>();
            container.AddSingleton<IProxyTypeGenerationReferee, ProxyTypeGenerationReferee>();
            container.AddSingleton<IInterceptorSelectorProvider, InterceptorSelectorProvider>();
            container.AddSingleton<IInterceptorMetadataProvider, InterceptorMetadataProvider>();
            container.AddSingleton<IDynamicProxyConfiguration>(configuration);
            container.AddSingleton<IMemberMetadataProvider>(new MemberMetadataProvider());
            container.AddSingleton<IObjectMethodExecutorProvider>(new ObjectMethodExecutorProvider());
            container.AddSingleton<IInterceptorMetadataCollector>(new DataAnnotationsInterceptorMetadataCollector());

            return container.ConfiureInterceptors(configuration);
        }

        private static IContainer ConfiureInterceptors(this IContainer container, DynamicProxyConfiguration configuration)
        {
            if (configuration.InterceptorRegistrations.Count != 0)
            {
                container.AddSingleton<IInterceptorMetadataCollector>(new ConfiguredInterceptorMetadataCollector(configuration.InterceptorRegistrations));
            }

            return container;
        }
    }
}
