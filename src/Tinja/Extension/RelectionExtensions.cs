using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Tinja
{
    internal static class RelectionExtensions
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
    }
}
