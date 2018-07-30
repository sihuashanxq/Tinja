using System;
using Tinja.Abstractions;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Executors;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Core.DynamicProxy.Executors;
using Tinja.Core.DynamicProxy.Executors.Internal;
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
            container.AddTransient<IInterceptorAccessor, InterceptorAccessor>();

            container.AddSingleton<IProxyTypeFactory, ProxyTypeFactory>();
            container.AddSingleton<IMethodInvokerBuilder, MethodInvokerBuilder>();
            container.AddSingleton<IMemberMetadataProvider, MemberMetadataProvider>();
            container.AddSingleton<IMethodInvocationExecutor, MethodInvocationExecutor>();
            container.AddSingleton<IProxyTypeGenerationReferee, ProxyTypeGenerationReferee>();
            container.AddSingleton<IInterceptorSelectorProvider, InterceptorSelectorProvider>();
            container.AddSingleton<IInterceptorMetadataProvider, InterceptorMetadataProvider>();
            container.AddSingleton<IObjectMethodExecutorProvider, ObjectMethodExecutorProvider>();
            container.AddSingleton<IInterceptorMetadataCollector, InterceptorMetadataCollector>();

            return container;
        }
    }
}
