using System;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Metadatas;

namespace Tinja.Core.DynamicProxy
{
    [DisableProxy]
    public class MemberProxyableDetector : IMemberProxyableDetector
    {
        private readonly IInterceptorMetadataProvider _provider;

        public MemberProxyableDetector(IInterceptorMetadataProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public bool IsProxyable(MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case EventInfo eventInfo:
                    return IsEventProxyable(eventInfo);
                case MethodInfo methodInfo:
                    return IsMethodProxyable(methodInfo);
                case PropertyInfo propertyInfo:
                    return IsPropertyProxyable(propertyInfo);
            }

            return false;
        }

        protected virtual bool IsEventProxyable(EventInfo eventInfo)
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

        protected virtual bool IsMethodProxyable(MethodInfo methodInfo)
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

        protected virtual bool IsPropertyProxyable(PropertyInfo propertyInfo)
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
