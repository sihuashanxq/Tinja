using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Tinja.Interception.Generators.Extensions
{
    internal static class EmitExtensions
    {
        internal static readonly MethodInfo MethodGetMethodFromHandle = typeof(MethodBase).GetMethod("GetMethodFromHandle", new [] { typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle) });

        internal static readonly MethodInfo GetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) });

        internal static readonly MethodInfo GetProperty = typeof(Type).GetMethod("GetProperty", new[] { typeof(string), typeof(BindingFlags) });

        internal static ILGenerator Box(this ILGenerator il, Type boxType)
        {
            if (boxType.IsValueType)
            {
                il.Emit(OpCodes.Box, boxType);
            }

            return il;
        }

        internal static ILGenerator UnBoxAny(this ILGenerator ilGen, Type valueType)
        {
            if (valueType.IsByRef)
            {
                ilGen.Emit(OpCodes.Unbox_Any, valueType.GetElementType());
            }
            else
            {
                ilGen.Emit(OpCodes.Unbox_Any, valueType);
            }

            return ilGen;
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
                ilGen.Emit(OpCodes.Ldnull);
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

        internal static ILGenerator LoadDefaultValue(this ILGenerator ilGen, Type valueType)
        {
            if (valueType == typeof(void))
            {
                return ilGen;
            }

            switch (Type.GetTypeCode(valueType))
            {
                case TypeCode.Decimal:
                    ilGen.Emit(OpCodes.Ldc_I4_0);
                    ilGen.Emit(OpCodes.Newobj, valueType.GetConstructor(new [] { typeof(int) }));
                    break;
                case TypeCode.Double:
                    ilGen.Emit(OpCodes.Ldc_R8, default(Double));
                    break;
                case TypeCode.DBNull:
                case TypeCode.Empty:
                case TypeCode.String:
                    ilGen.Emit(OpCodes.Ldnull);
                    break;
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    ilGen.Emit(OpCodes.Ldc_I4_0);
                    break;
                case TypeCode.Single:
                    ilGen.Emit(OpCodes.Ldc_R4, default(Single));
                    break;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    ilGen.Emit(OpCodes.Ldc_I8);
                    break;
                default:
                    if (valueType.IsValueType)
                    {
                        var localVar = ilGen.DeclareLocal(valueType);
                        ilGen.Emit(OpCodes.Ldloca, localVar);
                        ilGen.Emit(OpCodes.Initobj, valueType);
                        ilGen.Emit(OpCodes.Ldloc, localVar);
                        break;
                    }

                    ilGen.Emit(OpCodes.Ldnull);
                    break;
            }

            return ilGen;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        internal static ILGenerator CastValueToObject(this ILGenerator ilGen, Type valueType)
        {
            if (ilGen == null)
            {
                throw new NullReferenceException(nameof(ilGen));
            }

            if (valueType == null)
            {
                throw new NullReferenceException(nameof(valueType));
            }

            if (!valueType.IsByRef)
            {
                return ilGen.Box(valueType);
            }

            var elementType = valueType.GetElementType();
            if (elementType == null)
            {
                return ilGen;
            }

            switch (Type.GetTypeCode(elementType))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                    ilGen.Emit(OpCodes.Ldind_U1);
                    break;
                case TypeCode.SByte:
                    ilGen.Emit(OpCodes.Ldind_I1);
                    break;
                case TypeCode.Int16:
                    ilGen.Emit(OpCodes.Ldind_I2);
                    break;
                case TypeCode.Char:
                case TypeCode.UInt16:
                    ilGen.Emit(OpCodes.Ldind_U2);
                    break;
                case TypeCode.Int32:
                    ilGen.Emit(OpCodes.Ldind_I4);
                    break;
                case TypeCode.UInt32:
                    ilGen.Emit(OpCodes.Ldind_U4);
                    break;
                case TypeCode.Int64:
                    ilGen.Emit(OpCodes.Ldind_I8);
                    break;
                case TypeCode.UInt64:
                    ilGen.Emit(OpCodes.Ldind_I8);
                    ilGen.Emit(OpCodes.Conv_U8);
                    break;
                case TypeCode.Single:
                    ilGen.Emit(OpCodes.Ldind_R4);
                    break;
                case TypeCode.Double:
                    ilGen.Emit(OpCodes.Ldind_R8);
                    break;
                default:
                    ilGen.Emit(OpCodes.Ldind_Ref);
                    break;
            }

            return ilGen.Box(elementType);
        }

        internal static ILGenerator NewArray(this ILGenerator ilGen, Type arrayElementType, int length)
        {
            ilGen.Emit(OpCodes.Ldc_I4, length);
            ilGen.Emit(OpCodes.Newarr, arrayElementType);

            return ilGen;
        }

        internal static ILGenerator MakeArgumentArray(this ILGenerator ilGen, ParameterInfo[] parameters)
        {
            ilGen.NewArray(typeof(object), parameters.Length);

            for (var i = 0; i < parameters.Length; i++)
            {
                var argIndex = i;

                ilGen.SetArrayElement(
                    _ => ilGen.Emit(OpCodes.Dup),
                    _ => ilGen.Emit(OpCodes.Ldarg, argIndex + 1),
                    i,
                    parameters[i].ParameterType
                );
            }

            return ilGen;
        }

        internal static ILGenerator SetRefArgumentsWithArray(this ILGenerator ilGen, ParameterInfo[] parameters, LocalBuilder array)
        {
            for (var argIndex = 0; argIndex < parameters.Length; argIndex++)
            {
                var parameter = parameters[argIndex];
                if (parameter.IsIn || !parameter.ParameterType.IsByRef)
                {
                    continue;
                }

                ilGen.LoadArgument(argIndex + 1);
                ilGen.LoadArrayElement(_ => ilGen.Emit(OpCodes.Ldloc, array), argIndex, parameter.ParameterType);
                ilGen.Emit(OpCodes.Stind_Ref);
            }

            return ilGen;
        }

        internal static ILGenerator LoadArrayElement(this ILGenerator ilGen, Action<ILGenerator> loadArrayInstance, int arrayIndex, Type elementType)
        {
            if (arrayIndex < 0)
            {
                throw new IndexOutOfRangeException(nameof(arrayIndex));
            }

            loadArrayInstance(ilGen);
            ilGen.Emit(OpCodes.Ldc_I4, arrayIndex);
            ilGen.Emit(OpCodes.Ldelem_Ref);
            ilGen.UnBoxAny(elementType);

            return ilGen;
        }

        internal static ILGenerator SetArrayElement(this ILGenerator ilGen, Action<ILGenerator> loadArrayInstance, Action<ILGenerator> loadElementValue, int arrayIndex, Type valueType)
        {
            if (arrayIndex < 0)
            {
                throw new IndexOutOfRangeException(nameof(arrayIndex));
            }

            loadArrayInstance(ilGen);
            ilGen.Emit(OpCodes.Ldc_I4, arrayIndex);
            loadElementValue(ilGen);
            ilGen.CastValueToObject(valueType);
            ilGen.Emit(OpCodes.Stelem_Ref);

            return ilGen;
        }

        internal static ILGenerator SetThisField(this ILGenerator ilGen, FieldBuilder fieldBuilder, Action<ILGenerator> loadFieldValue)
        {
            ilGen.This();
            loadFieldValue(ilGen);
            ilGen.Emit(OpCodes.Stfld, fieldBuilder);

            return ilGen;
        }

        internal static ILGenerator LoadThisField(this ILGenerator ilGen, FieldBuilder fieldBuilder)
        {
            ilGen.This();
            ilGen.Emit(OpCodes.Ldfld, fieldBuilder);

            return ilGen;
        }

        internal static ILGenerator SetStaticField(this ILGenerator ilGen, FieldBuilder fieldBuilder, Action<ILGenerator> loadFieldValue)
        {
            loadFieldValue(ilGen);
            ilGen.Emit(OpCodes.Stsfld, fieldBuilder);

            return ilGen;
        }

        internal static ILGenerator LoadStaticField(this ILGenerator ilGen, FieldBuilder fieldBuilder)
        {
            ilGen.Emit(OpCodes.Ldsfld, fieldBuilder);

            return ilGen;
        }

        internal static ILGenerator Call(this ILGenerator ilGen, MethodInfo methodInfo, params Action<ILGenerator>[] argumentStuffers)
        {
            if (argumentStuffers != null)
            {
                foreach (var stuffer in argumentStuffers)
                {
                    stuffer(ilGen);
                }
            }

            ilGen.Emit(OpCodes.Call, methodInfo);

            return ilGen;
        }

        internal static ILGenerator Base(this ILGenerator ilGen, ConstructorInfo constructorInfo, params Action<ILGenerator>[] argumentStuffers)
        {
            ilGen.This();

            if (argumentStuffers != null)
            {
                foreach (var stuffer in argumentStuffers)
                {
                    stuffer(ilGen);
                }
            }

            ilGen.Emit(OpCodes.Call, constructorInfo);

            return ilGen;
        }

        internal static ILGenerator CallVirt(this ILGenerator ilGen, MethodInfo methodInfo, params Action<ILGenerator>[] argumentStuffers)
        {
            if (argumentStuffers != null)
            {
                foreach (var stuffer in argumentStuffers)
                {
                    stuffer(ilGen);
                }
            }

            ilGen.Emit(OpCodes.Callvirt, methodInfo);

            return ilGen;
        }

        internal static ILGenerator New(this ILGenerator ilGen, ConstructorInfo constructor, params Action<ILGenerator>[] argumentStuffers)
        {
            if (argumentStuffers != null)
            {
                foreach (var stuffer in argumentStuffers)
                {
                    stuffer(ilGen);
                }
            }

            ilGen.Emit(OpCodes.Newobj, constructor);

            return ilGen;
        }

        internal static ILGenerator LoadArgument(this ILGenerator ilGen, int argumentIndex)
        {
            if (argumentIndex < 0 || argumentIndex > ushort.MaxValue)
            {
                throw new IndexOutOfRangeException(nameof(argumentIndex));
            }

            switch (argumentIndex)
            {
                case 0:
                    ilGen.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    ilGen.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    ilGen.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    ilGen.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (argumentIndex <= byte.MaxValue)
                    {
                        ilGen.Emit(OpCodes.Ldarg_S);
                        break;
                    }

                    ilGen.Emit(OpCodes.Ldarg, argumentIndex);
                    break;
            }

            return ilGen;
        }

        internal static ILGenerator This(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Ldarg_0);

            return ilGen;
        }

        internal static ILGenerator Return(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Ret);

            return ilGen;
        }

        internal static ILGenerator SetVariableValue(this ILGenerator ilGen, LocalBuilder localBuilder)
        {
            ilGen.Emit(OpCodes.Stloc, localBuilder);

            return ilGen;
        }

        internal static ILGenerator LoadVariableRef(this ILGenerator ilGen, LocalBuilder localBuilder)
        {
            ilGen.Emit(OpCodes.Ldloca, localBuilder);

            return ilGen;
        }

        internal static ILGenerator LoadVariable(this ILGenerator ilGen, LocalBuilder localBuilder)
        {
            ilGen.Emit(OpCodes.Ldloc, localBuilder);

            return ilGen;
        }

        internal static ILGenerator LoadVariable(this ILGenerator ilGen, int slot)
        {
            if (slot < 0)
            {
                throw new IndexOutOfRangeException(nameof(slot));
            }

            switch (slot)
            {
                case 0:
                    ilGen.Emit(OpCodes.Ldloc_0);
                    break;
                case 1:
                    ilGen.Emit(OpCodes.Ldloc_1);
                    break;
                case 2:
                    ilGen.Emit(OpCodes.Ldloc_2);
                    break;
                case 3:
                    ilGen.Emit(OpCodes.Ldloc_3);
                    break;
                default:
                    if (slot <= byte.MaxValue)
                    {
                        ilGen.Emit(OpCodes.Ldloc_S, slot);
                        break;
                    }

                    ilGen.Emit(OpCodes.Ldloc, slot);
                    break;
            }

            return ilGen;
        }

        internal static ILGenerator TypeOf(this ILGenerator ilGen, Type type)
        {
            ilGen.Emit(OpCodes.Ldtoken, type);

            return ilGen;
        }
    }
}
