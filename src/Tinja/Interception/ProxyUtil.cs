using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace Tinja.Interception
{
    public static class ProxyUtil
    {
        const string AssemblyName = "Tinja.Interception.DynamicProxy";

        const string ModuleName = "ProxyModules";

        internal static ModuleBuilder ModuleBuilder { get; }

        internal static AssemblyBuilder AssemblyBuilder { get; }

        internal static ConcurrentDictionary<Type, Type> ProxyTypeCaches { get; }

        static ProxyUtil()
        {
            AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(AssemblyName), AssemblyBuilderAccess.Run);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule(ModuleName);
            ProxyTypeCaches = new ConcurrentDictionary<Type, Type>();
        }

        public static Type GenerateProxyType(Type implType, Type baseType)
        {
            var methodIntereceptors = GetMethodDefinedIntereceptors(baseType);

            var typeBuilder = DefineType(implType, baseType);
            var fileds = DefineFields(typeBuilder, methodIntereceptors.Values.SelectMany(i => i).Distinct().ToArray());

            DefineConstructors(typeBuilder, baseType, methodIntereceptors.SelectMany(i => i.Value).Distinct().ToArray(), fileds);
            //DefineMethods(typeBuilder, baseType,)

            return typeBuilder.CreateType();
        }

        internal static void DefineConstructors(
            TypeBuilder typeBuilder,
            Type baseType,
            Type[] parameterTypes,
            Dictionary<Type, FieldBuilder> fileds
        )
        {
            var constructors = baseType.GetConstructors();
            if (constructors.Length == 0)
            {
                DefineConstructor(typeBuilder, null, parameterTypes, fileds);
                return;
            }

            foreach (var item in constructors)
            {
                DefineConstructor(typeBuilder, item, parameterTypes, fileds);
            }
        }

        internal static void DefineConstructor(
            TypeBuilder typeBuilder,
            ConstructorInfo constrcutor,
            Type[] parameterTypes,
            Dictionary<Type, FieldBuilder> parameterFileds
        )
        {
            var methodAttributes = constrcutor?.Attributes ?? MethodAttributes.Public;
            var callingConvention = constrcutor?.CallingConvention ?? CallingConventions.HasThis;
            var parameters = parameterTypes
                .Concat(
                    (constrcutor?.GetParameters() ?? new ParameterInfo[0])
                    .Select(i => i.ParameterType)
                )
                .ToArray();

            var constructorBuilder = typeBuilder.DefineConstructor(
                methodAttributes,
                callingConvention,
                parameters
            );

            var il = constructorBuilder.GetILGenerator();

            for (var i = 0; i < parameterTypes.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg, i + 1);
                il.Emit(OpCodes.Stfld, parameterFileds[parameterTypes[i]]);
            }

            if (constrcutor != null)
            {
                il.Emit(OpCodes.Ldarg_0);

                for (var i = parameterTypes.Length; i < parameters.Length; i++)
                {
                    il.Emit(OpCodes.Ldarg, i + 1);
                }

                il.Emit(OpCodes.Call, constrcutor);
            }

            il.Emit(OpCodes.Ret);
        }

        internal static void DefineMethods(
            TypeBuilder typeBuilder,
            Type baseType,
            Dictionary<Type, FieldBuilder> fileds,
            Dictionary<MethodInfo, List<Type>> methodInterecetpors
        )
        {

        }

        internal static TypeBuilder DefineType(Type implType, Type baseType)
        {
            if (implType.IsInterface || implType.IsAbstract)
            {
                throw new NotSupportedException($"implemetion type:{implType.FullName} must not be interface and abstract!");
            }

            return
                ModuleBuilder.DefineType(
                   GetTypeName(baseType),
                   TypeAttributes.Class | TypeAttributes.Public,
                   baseType.IsInterface ? null : baseType,
                   baseType.IsInterface ? new[] { baseType } : null
               );
        }

        internal static Dictionary<MethodInfo, List<Type>> GetMethodDefinedIntereceptors(Type baseType)
        {
            var intereceptors = baseType.GetCustomAttributes<InterceptorAttribute>();
            var map = new Dictionary<MethodInfo, List<Type>>();

            foreach (var item in baseType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                map[item] = item
                    .GetCustomAttributes<InterceptorAttribute>()
                    .Concat(intereceptors)
                    .Select(i => i.InterceptorType)
                    .Distinct()
                    .ToList();
            }

            return map;
        }

        internal static Dictionary<Type, FieldBuilder> DefineFields(TypeBuilder typeBuilder, Type[] fieldTypes)
        {
            var map = new Dictionary<Type, FieldBuilder>();

            if (fieldTypes == null)
            {
                return map;
            }

            for (var i = 0; i < fieldTypes.Length; i++)
            {
                var fieldType = fieldTypes[i];
                if (fieldType == null)
                {
                    continue;
                }

                map[fieldType] = typeBuilder.DefineField(
                    "__" + fieldType.Name + i,
                    fieldTypes[i],
                    FieldAttributes.Private
                );
            }

            return map;
        }

        static string GetTypeName(Type serviceType)
        {
            return AssemblyName + serviceType.Namespace + "." + serviceType.Name + Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}
