using System;
using Tinja.Abstractions;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Executions;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Core.DynamicProxy.Executions;
using Tinja.Core.DynamicProxy.Metadatas;
using Tinja.Core.Extensions;

namespace Tinja.Core.DynamicProxy
{
    public static class DynamicProxyExtensions
    {
        public static IContainer UseDynamicProxy(this IContainer container)
        {
            if (container == null)
            {
                throw new NullReferenceException(nameof(container));
            }

            container.AddScoped<IInterceptorFactory, InterceptorFactory>();

            container.AddTransient<MethodInvocationInvokerBuilder>();

            container.AddSingleton<IProxyTypeFactory, ProxyTypeFactory>();
            container.AddSingleton<IMemberMetadataProvider, MemberMetadataProvider>();
            container.AddSingleton<IProxyTypeGenerationReferee, ProxyTypeGenerationReferee>();
            container.AddSingleton<IInterceptorSelectorProvider, InterceptorSelectorProvider>();
            container.AddSingleton<IInterceptorMetadataProvider, InterceptorMetadataProvider>();
            container.AddSingleton<IObjectMethodExecutorProvider, ObjectMethodExecutorProvider>();
            container.AddSingleton<IInterceptorMetadataCollector, InterceptorMetadataCollector>();

            return container;
        }
    }
}
