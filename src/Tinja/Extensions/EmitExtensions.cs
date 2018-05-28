using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Tinja.Extension
{
    internal static class EmitExtensions
    {
        static readonly MethodInfo MethodGetMethodFromHandle = typeof(MethodBase).GetMethod("GetMethodFromHandle", new Type[] { typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle) });

        static readonly MethodInfo GetProperty = typeof(Type).GetMethod("GetProperty", new[] { typeof(string), typeof(BindingFlags) });

        internal static ILGenerator Box(this ILGenerator il, Type boxType)
        {
            if (boxType.IsValueType)
            {
                il.Emit(OpCodes.Box, boxType);
            }

            return il;
        }

        public static void LoadMethodInfo(this ILGenerator ilGen, MethodInfo methodInfo)
        {
            ilGen.Emit(OpCodes.Ldtoken, methodInfo);
            ilGen.Emit(OpCodes.Ldtoken, methodInfo.DeclaringType);
            ilGen.Emit(OpCodes.Call, MethodGetMethodFromHandle);
        }

        public static void LoadPropertyInfo(this ILGenerator ilGen, PropertyInfo propertyInfo)
        {
            ilGen.Emit(OpCodes.Ldtoken, propertyInfo.DeclaringType);
            ilGen.Emit(OpCodes.Ldstr, propertyInfo.Name);
            ilGen.Emit(OpCodes.Ldc_I4, (int)(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic));
            ilGen.Emit(OpCodes.Call, GetProperty);
        }
    }
}
