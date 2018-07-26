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
                    return ShoudMethodProxy(methodInfo);
                case PropertyInfo propertyInfo:
                    return ShouldPropertyProxy(propertyInfo);
                default:
                    return false;
            }
        }

        protected virtual bool ShoudMethodProxy(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                return false;
            }

            if (IsInterfaceOrAbstractMethod(methodInfo))
            {
                return true;
            }

            return MethodOverridable(methodInfo) && _provider.GetInterceptors(methodInfo).Any();
        }

        protected virtual bool ShouldPropertyProxy(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                return false;
            }

            if (IsInterfaceOrAbstractMethod(propertyInfo.GetMethod) ||
                IsInterfaceOrAbstractMethod(propertyInfo.GetMethod))
            {
                return true;
            }

            if (MethodOverridable(propertyInfo.GetMethod))
            {
                return _provider.GetInterceptors(propertyInfo).Any();
            }

            if (MethodOverridable(propertyInfo.SetMethod))
            {
                return _provider.GetInterceptors(propertyInfo).Any();
            }

            return false;
        }

        protected virtual bool MethodOverridable(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                return false;
            }

            if (methodInfo.IsFinal || methodInfo.DeclaringType.IsSealed)
            {
                return false;
            }

            return methodInfo.IsVirtual;
        }

        protected virtual bool IsInterfaceOrAbstractMethod(MethodInfo methodInfo)
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

            return false;
        }
    }
}
