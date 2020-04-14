using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy.Registrations;

namespace Tinja.Abstractions.Extensions
{
    /// <summary>
    /// extensions for reflection
    /// </summary>
    public static class ReflectionExtensions
    {
        public static bool IsType(this Type type, Type target)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            return target.IsAssignableFrom(type);
        }

        public static bool IsType<TType>(this Type type)
        {
            return type.IsType(typeof(TType));
        }

        public static bool IsNotType<TType>(this Type type)
        {
            return !type.IsType<TType>();
        }

        public static bool IsNotType(this Type type, Type target)
        {
            return !type.IsType(target);
        }

        public static EventInfo AsEvent(this MemberInfo member)
        {
            return member as EventInfo;
        }

        public static MethodInfo AsMethod(this MemberInfo member)
        {
            return member as MethodInfo;
        }

        public static PropertyInfo AsProperty(this MemberInfo member)
        {
            return member as PropertyInfo;
        }

        public static bool IsVoidType(this Type type)
        {
            return type == typeof(void);
        }

        public static bool IsVoidMethod(this MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            return method.ReturnType.IsVoidType();
        }

        public static bool IsTask(this Type type)
        {
            return type != null && type.IsType<Task>();
        }

        public static bool IsValueTask(this Type type)
        {
            return type == typeof(ValueTask) || type.IsGenericType && type.GetGenericTypeDefinition().IsType(typeof(ValueTask<>));
        }

        public static IEnumerable<TElement> Distinct<TElement, TKey>(this IEnumerable<TElement> elements, Func<TElement, TKey> keyProvider)
        {
            if (elements == null)
            {
                return null;
            }

            var map = new Dictionary<TKey, TElement>();

            foreach (var element in elements)
            {
                map.TryAdd(keyProvider(element), element);
            }

            return map.Values;
        }

        public static InterceptorAttribute[] GetInterceptorAttributes(this MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                throw new ArgumentNullException(nameof(memberInfo));
            }

            var attributes = memberInfo.GetCustomAttributes<InterceptorAttribute>(false).ToArray();

            if (memberInfo.DeclaringType != null &&
                memberInfo.DeclaringType.IsInterface)
            {
                return attributes;
            }

            if (memberInfo is Type typeInfo && typeInfo.IsInterface)
            {
                return attributes;
            }

            var inherits = memberInfo
                .GetCustomAttributes<InterceptorAttribute>(true)
                .Except(attributes)
                .Where(i => i.Inherited);

            return attributes
                .Concat(inherits)
                .Distinct(i => i.InterceptorType)
                .ToArray();
        }

        public static IEnumerable<MemberInfo> GetInterfaceMaps(this MethodInfo methodInfo, Type[] interfaces)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            if (interfaces == null)
            {
                throw new ArgumentNullException(nameof(interfaces));
            }

            foreach (var @interface in interfaces)
            {
                if (methodInfo.DeclaringType.IsNotType(@interface))
                {
                    continue;
                }

                var mapping = methodInfo.DeclaringType.GetInterfaceMap(@interface);
                if (mapping.TargetMethods.Length == 0)
                {
                    continue;
                }

                for (var i = 0; i < mapping.TargetMethods.Length; i++)
                {
                    if (mapping.TargetMethods[i] == methodInfo)
                    {
                        yield return mapping.InterfaceMethods[i];
                        break;
                    }
                }
            }
        }

        public static IEnumerable<MemberInfo> GetInterfaceMaps(this PropertyInfo property, Type[] interfaces)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (interfaces == null)
            {
                throw new ArgumentNullException(nameof(interfaces));
            }

            return interfaces
                .Select(@interface => @interface.GetProperty(property.Name))
                .Where(item => item != null);
        }

        public static IEnumerable<MemberInfo> GetInterfaceMaps(this EventInfo eventInfo, Type[] interfaces)
        {
            if (eventInfo == null)
            {
                throw new ArgumentNullException(nameof(eventInfo));
            }

            if (interfaces == null)
            {
                throw new ArgumentNullException(nameof(interfaces));
            }

            return interfaces
                .Select(@interface => @interface.GetEvent(eventInfo.Name))
                .Where(item => item != null);
        }

        public static IEnumerable<MemberInfo> GetInterfaceMaps(this MemberInfo memberInfo, Type[] interfaces)
        {
            if (memberInfo == null)
            {
                throw new ArgumentNullException(nameof(memberInfo));
            }

            if (interfaces == null)
            {
                throw new ArgumentNullException(nameof(interfaces));
            }

            switch (memberInfo)
            {
                case MethodInfo methodInfo:
                    return methodInfo.GetInterfaceMaps(interfaces);

                case PropertyInfo propertyInfo:
                    return propertyInfo.GetInterfaceMaps(interfaces);

                case EventInfo eventInfo:
                    return eventInfo.GetInterfaceMaps(interfaces);

                default:
                    return new MemberInfo[0];
            }
        }

        public static bool IsOverrideable(this MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                return false;
            }

            if (memberInfo.MemberType != MemberTypes.Method &&
                memberInfo.MemberType != MemberTypes.Property &&
                memberInfo.MemberType != MemberTypes.Event)
            {
                return false;
            }

            if (memberInfo is MethodInfo methodInfo)
            {
                if (methodInfo.IsPrivate)
                {
                    return false;
                }

                if (methodInfo.IsFinal)
                {
                    return false;
                }

                return methodInfo.IsVirtual || methodInfo.IsAbstract;
            }

            if (memberInfo is PropertyInfo property)
            {
                return
                    property.GetMethod?.IsOverrideable() ??
                    property.SetMethod?.IsOverrideable() ??
                    false;
            }

            if (memberInfo is EventInfo eventInfo)
            {
                return
                    eventInfo.AddMethod?.IsOverrideable() ??
                    eventInfo.RemoveMethod?.IsOverrideable() ??
                    eventInfo.RaiseMethod?.IsOverrideable() ??
                    false;
            }

            return false;
        }
    }
}
