using System;

namespace Tinja
{
    internal static class RelectionExtensions
    {
        internal static bool Is(this Type type, Type target)
        {
            return target.IsAssignableFrom(type);
        }
    }
}
