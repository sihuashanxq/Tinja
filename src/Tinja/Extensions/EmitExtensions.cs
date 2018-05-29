using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Tinja.Extension
{
    internal static class EmitExtensions
    {
        internal static readonly MethodInfo MethodGetMethodFromHandle = typeof(MethodBase).GetMethod("GetMethodFromHandle", new Type[] { typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle) });

        internal static readonly MethodInfo GetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) });

        internal static readonly MethodInfo GetProperty = typeof(Type).GetMethod("GetProperty", new[] { typeof(string), typeof(BindingFlags) });

        internal static readonly MethodInfo MakeGenericMethod = typeof(MethodInfo).GetMethod("MakeGenericMethod", new[] { typeof(Type[]) });

        internal static ILGenerator Box(this ILGenerator il, Type boxType)
        {
            if (boxType.IsValueType)
            {
                il.Emit(OpCodes.Box, boxType);
            }

            return il;
        }

        internal static void LoadMethodInfo(this ILGenerator ilGen, MethodInfo methodInfo)
        {
            ilGen.Emit(OpCodes.Ldtoken, methodInfo);
            ilGen.Emit(OpCodes.Ldtoken, methodInfo.DeclaringType);
            ilGen.Emit(OpCodes.Call, MethodGetMethodFromHandle);
        }

        internal static void LoadPropertyInfo(this ILGenerator ilGen, PropertyInfo propertyInfo)
        {
            ilGen.Emit(OpCodes.Ldtoken, propertyInfo.DeclaringType);
            ilGen.Emit(OpCodes.Ldstr, propertyInfo.Name);
            ilGen.Emit(OpCodes.Ldc_I4, (int)(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic));
            ilGen.Emit(OpCodes.Call, GetProperty);
        }

        internal static ILGenerator LoadMethodGenericArguments(this ILGenerator ilGen, MethodInfo methodInfo)
        {
            if (!methodInfo.IsGenericMethod)
            {
                return ilGen;
            }

            var genericArguments = methodInfo.GetGenericArguments();
            if (genericArguments.Length == 0)
            {
                return ilGen;
            }

            ilGen.Emit(OpCodes.Ldc_I4, genericArguments.Length);
            ilGen.Emit(OpCodes.Newarr, typeof(Type));

            for (var i = 0; i < genericArguments.Length; i++)
            {
                ilGen.Emit(OpCodes.Dup);
                ilGen.Emit(OpCodes.Ldc_I4, i);
                ilGen.Emit(OpCodes.Ldtoken, genericArguments[i]);
                ilGen.Emit(OpCodes.Call, GetTypeFromHandle);     //泛型实参
                ilGen.Emit(OpCodes.Stelem_Ref);
            }

            return ilGen;
        }
    }
}
