using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Tinja.Interception;

namespace Tinja
{
    internal static class ReflectionExtensions
    {
        internal static bool Is(this Type type, Type target)
        {
            return target.IsAssignableFrom(type);
        }

        internal static PropertyInfo AsProperty(this MemberInfo m)
        {
            return m as PropertyInfo;
        }

        internal static MethodInfo AsMethod(this MemberInfo m)
        {
            return m as MethodInfo;
        }

        internal static bool IsVoid(this Type type)
        {
            return type == typeof(void);
        }

        internal static bool IsVoidMethod(this MethodInfo method)
        {
            return method != null && method.ReturnType.IsVoid();
        }

        internal static bool IsTask(this Type type)
        {
            return type != null && typeof(Task).IsAssignableFrom(type);
        }

        internal static IEnumerable<TElement> Distinct<TElement, TKey>(this IEnumerable<TElement> elements, Func<TElement, TKey> keyProvider)
        {
            if (elements == null)
            {
                return elements;
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

        internal static IEnumerable<PropertyInfo> GetProperties(this Type typeInfo, IEnumerable<Type> excepts)
        {
            if (typeInfo == null)
            {
                return new PropertyInfo[0];
            }

            if (excepts != null)
            {
                var set = excepts.ToHashSet();
                if (set.Any())
                {
                    return typeInfo
                        .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(i => !set.Contains(i.DeclaringType));
                }
            }

            return typeInfo.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        internal static IEnumerable<MethodInfo> GetMethods(this Type typeInfo, IEnumerable<Type> excepts)
        {
            if (typeInfo == null)
            {
                return new MethodInfo[0];
            }

            if (excepts != null)
            {
                var set = excepts.ToHashSet();
                if (set.Any())
                {
                    return typeInfo
                        .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(i => !set.Contains(i.DeclaringType));
                }
            }

            return typeInfo.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        internal static InterceptorAttribute[] GetInterceptorAttributes(this MemberInfo memberInfo)
        {
            var attrs = memberInfo.GetCustomAttributes<InterceptorAttribute>(false);

            if (memberInfo.DeclaringType != null && memberInfo.DeclaringType.IsInterface)
            {
                return attrs.ToArray();
            }

            if (memberInfo is Type typeInfo && typeInfo.IsInterface)
            {
                return attrs.ToArray();
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

        public static IEnumerable<MemberInfo> GetInterfaceMembers(this MethodInfo methodInfo, Type[] interfaces)
        {
            foreach (var @interface in interfaces)
            {
                if (!@interface.IsAssignableFrom(methodInfo.DeclaringType))
                {
                    continue;
                }

                var mapping = methodInfo.DeclaringType.GetInterfaceMap(@interface);

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

        public static IEnumerable<MemberInfo> GetInterfaceMembers(this PropertyInfo property, Type[] interfaces)
        {
            foreach (var @interface in interfaces)
            {
                var interfaceProperty = @interface.GetProperty(property.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (interfaceProperty != null)
                {
                    yield return interfaceProperty;
                }
            }
        }

        public static IEnumerable<MemberInfo> GetInterfaceMembers(this EventInfo eventInfo, Type[] interfaces)
        {
            foreach (var @interface in interfaces)
            {
                var @event = @interface.GetEvent(eventInfo.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (@event != null)
                {
                    yield return @event;
                }
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
                return !methodInfo.IsPrivate && methodInfo.IsVirtual;
            }

            if (memberInfo is PropertyInfo property)
            {
                return
                    property?.GetMethod.IsOverrideable() ??
                    property?.SetMethod.IsOverrideable() ??
                    false;
            }

            return false;
        }
    }
}
