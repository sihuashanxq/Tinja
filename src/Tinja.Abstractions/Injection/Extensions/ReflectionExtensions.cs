using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy;

namespace Tinja.Abstractions.Injection.Extensions
{
    public static class ReflectionExtensions
    {
        public static bool Is(this Type type, Type target)
        {
            return target.IsAssignableFrom(type);
        }

        public static bool Is<TType>(this Type type)
        {
            return type.Is(typeof(TType));
        }

        public static bool IsNot<TType>(this Type type)
        {
            return !type.Is<TType>();
        }

        public static bool IsNot(this Type type, Type target)
        {
            return !type.Is(target);
        }

        public static PropertyInfo AsProperty(this MemberInfo m)
        {
            return m as PropertyInfo;
        }

        public static MethodInfo AsMethod(this MemberInfo m)
        {
            return m as MethodInfo;
        }

        public static bool IsVoid(this Type type)
        {
            return type == typeof(void);
        }

        public static bool IsVoidMethod(this MethodInfo method)
        {
            return method != null && method.ReturnType.IsVoid();
        }

        public static bool IsTask(this Type type)
        {
            return type != null && typeof(Task).IsAssignableFrom(type);
        }

        public static bool IsValueTask(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>);
        }

        public static IEnumerable<TElement> Distinct<TElement, TKey>(this IEnumerable<TElement> elements, Func<TElement, TKey> keyProvider)
        {
            if (elements == null)
            {
                return null;
            }

            var pairs = new Dictionary<TKey, TElement>();

            foreach (var element in elements)
            {
                var elementKey = keyProvider(element);

                if (pairs.ContainsKey(elementKey))
                {
                    continue;
                }

                pairs[elementKey] = element;
            }

            return pairs.Values;
        }

        public static InterceptorAttribute[] GetInterceptorAttributes(this MemberInfo memberInfo)
        {
            var attrs = memberInfo.GetCustomAttributes<InterceptorAttribute>(false).ToArray();

            if (memberInfo.DeclaringType != null && memberInfo.DeclaringType.IsInterface)
            {
                return attrs;
            }

            if (memberInfo is Type typeInfo && typeInfo.IsInterface)
            {
                return attrs;
            }

            var inheritedAttrs = memberInfo
                .GetCustomAttributes<InterceptorAttribute>(true)
                .Except(attrs)
                .Where(i => i.Inherited);

            return attrs
                .Concat(inheritedAttrs)
                .Distinct(i => i.InterceptorType)
                .ToArray();
        }

        public static IEnumerable<MemberInfo> GetInterfaceMaps(this MethodInfo methodInfo, Type[] interfaces)
        {
            if (methodInfo == null)
            {
                throw new NullReferenceException(nameof(methodInfo));
            }

            if (interfaces == null)
            {
                throw new NullReferenceException(nameof(interfaces));
            }

            foreach (var @interface in interfaces)
            {
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
                throw new NullReferenceException(nameof(property));
            }

            if (interfaces == null)
            {
                throw new NullReferenceException(nameof(interfaces));
            }

            return interfaces
                .Select(@interface => @interface.GetProperty(property.Name))
                .Where(mapProperty => mapProperty != null);
        }

        public static IEnumerable<MemberInfo> GetInterfaceMaps(this EventInfo eventInfo, Type[] interfaces)
        {
            if (eventInfo == null)
            {
                throw new NullReferenceException(nameof(eventInfo));
            }

            if (interfaces == null)
            {
                throw new NullReferenceException(nameof(interfaces));
            }

            return interfaces
                .Select(@interface => @interface.GetEvent(eventInfo.Name))
                .Where(@event => @event != null);
        }

        public static IEnumerable<MemberInfo> GetInterfaceMaps(this MemberInfo memberInfo, Type[] interfaces)
        {
            if (memberInfo == null)
            {
                throw new NullReferenceException(nameof(memberInfo));
            }

            if (interfaces == null)
            {
                throw new NullReferenceException(nameof(interfaces));
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
                memberInfo.MemberType != MemberTypes.Property)
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

            return false;
        }
    }
}
