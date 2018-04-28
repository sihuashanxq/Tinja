using System;
using System.Reflection.Emit;

namespace Tinja.Extension
{
    internal static class EmitExtensions
    {
        internal static ILGenerator Box(this ILGenerator il, Type boxType)
        {
            if (boxType.IsValueType)
            {
                il.Emit(OpCodes.Box, boxType);
            }

            return il;
        }
    }
}
