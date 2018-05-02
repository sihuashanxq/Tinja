using System;
using System.Reflection;

namespace Tinja
{
    internal static class RelectionExtensions
    {
        internal static bool Is(this Type type, Type target)
        {
            return target.IsAssignableFrom(type);
        }

        internal static bool IsVoid(this Type type)
        {
            return type == typeof(void);
        }

        internal static bool IsVoidMethod(this MethodInfo method)
        {
            return method != null && method.ReturnType.IsVoid();
        }
    }
}
