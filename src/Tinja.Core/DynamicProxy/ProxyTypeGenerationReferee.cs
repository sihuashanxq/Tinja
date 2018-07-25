using System;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Definitions;

namespace Tinja.Core.DynamicProxy
{
    public class ProxyTypeGenerationReferee : IProxyTypeGenerationReferee
    {
        private readonly IInterceptorDefinitionProvider _provider;

        public ProxyTypeGenerationReferee(IInterceptorDefinitionProvider provider)
        {
            _provider = provider ?? throw new NullReferenceException(nameof(provider));
        }

        public bool ShouldProxy(MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case MethodInfo methodInfo:
                    return ShouldMethodProxy(methodInfo) || _provider.GetInterceptors(methodInfo).Any();
                case PropertyInfo propertyInfo:
                    return ShouldPropertyProxy(propertyInfo) || _provider.GetInterceptors(propertyInfo).Any();
                default:
                    return false;
            }
        }

        protected virtual bool ShouldMethodProxy(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                return false;
            }

            if (methodInfo.DeclaringType.IsInterface)
            {
                return true;
            }

            if (methodInfo.IsAbstract)
            {
                return true;
            }

            if (methodInfo.IsFinal || methodInfo.DeclaringType.IsSealed)
            {
                return false;
            }

            return methodInfo.IsVirtual;
        }

        protected virtual bool ShouldPropertyProxy(PropertyInfo propertyinfo)
        {
            return ShouldMethodProxy(propertyinfo.GetMethod) || ShouldMethodProxy(propertyinfo.SetMethod);
        }
    }
}
