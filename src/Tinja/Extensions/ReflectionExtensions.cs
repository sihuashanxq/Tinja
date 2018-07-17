using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Tinja.Interception;

namespace Tinja.Extensions
{
    internal static class ReflectionExtensions
    {
        internal static bool Is(this Type type, Type target)
        {
            return target.IsAssignableFrom(type);
        }

        internal static bool Is<TType>(this Type type)
        {
            return Is(type, typeof(TType));
        }

        internal static bool IsNot<TType>(this Type type)
        {
            return !Is<TType>(type);
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

        internal static bool IsValueTask(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>);
        }

        internal static IEnumerable<TElement> Distinct<TElement, TKey>(this IEnumerable<TElement> elements, Func<TElement, TKey> keyProvider)
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
        internal static InterceptorAttribute[] GetInterceptorAttributes(this MemberInfo memberInfo)
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

        /// <summary>
        /// 获取给定方法在定接口中的映射成员
        /// </summary>
        /// <param name="interfaces"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        internal static IEnumerable<MemberInfo> GetInterfaceMapMembers(this MethodInfo methodInfo, Type[] interfaces)
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

        /// <summary>
        /// 获取给定属性在定接口中的映射成员
        /// </summary>
        /// <param name="interfaces"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        internal static IEnumerable<MemberInfo> GetInterfaceMapMembers(this PropertyInfo property, Type[] interfaces)
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

        /// <summary>
        /// 获取给定事件在定接口中的映射成员
        /// </summary>
        /// <param name="eventInfo"></param>
        /// <param name="interfaces"></param>
        /// <returns></returns>
        internal static IEnumerable<MemberInfo> GetInterfaceMapMembers(this EventInfo eventInfo, Type[] interfaces)
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

        /// <summary>
        /// 是否可override
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        internal static bool IsOverrideable(this MemberInfo memberInfo)
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
