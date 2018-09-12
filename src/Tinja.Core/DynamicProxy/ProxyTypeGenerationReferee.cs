using System;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Metadatas;

namespace Tinja.Core.DynamicProxy
{
    [DisableProxy]
    public class ProxyTypeGenerationReferee : IProxyTypeGenerationReferee
    {
        private readonly IInterceptorMetadataProvider _provider;

        public ProxyTypeGenerationReferee(IInterceptorMetadataProvider provider)
        {
            _provider = provider ?? throw new NullReferenceException(nameof(provider));
        }

        public bool ShouldProxy(MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case EventInfo eventInfo:
                    return ShowEventProxy(eventInfo);
                case MethodInfo methodInfo:
                    return ShoudMethodProxy(methodInfo);
                case PropertyInfo propertyInfo:
                    return ShouldPropertyProxy(propertyInfo);
                default:
                    return false;
            }
        }

        protected virtual bool ShowEventProxy(EventInfo eventInfo)
        {
            if (eventInfo == null)
            {
                return false;
            }

            if (IsInterfaceOrAbstractMethod(eventInfo.RaiseMethod) ||
                IsInterfaceOrAbstractMethod(eventInfo.AddMethod) ||
                IsInterfaceOrAbstractMethod(eventInfo.RemoveMethod))
            {
                return true;
            }

            if (MethodOverridable(eventInfo.RaiseMethod) ||
                MethodOverridable(eventInfo.AddMethod) ||
                MethodOverridable(eventInfo.RemoveMethod))
            {
                return _provider.GetInterceptors(eventInfo).Any();
            }

            return false;
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

            if (MethodOverridable(propertyInfo.GetMethod) ||
                MethodOverridable(propertyInfo.SetMethod))
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
