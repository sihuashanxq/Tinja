using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace Tinja.Interception
{
    public static class TypeGeneratorUtil
    {
        const string AssemblyName = "Tinja.Interception.DynamicProxy";

        const string ModuleName = "ProxyModules";

        internal static ModuleBuilder ModuleBuilder { get; }

        internal static AssemblyBuilder AssemblyBuilder { get; }

        internal static ConcurrentDictionary<Type, Type> ProxyTypeCaches { get; }

        static TypeGeneratorUtil()
        {
            AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(AssemblyName), AssemblyBuilderAccess.Run);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule(ModuleName);
            ProxyTypeCaches = new ConcurrentDictionary<Type, Type>();
        }

        public static TypeBuilder DefineType(Type implementionType, Type baseType)
        {
            if (implementionType.IsInterface || implementionType.IsAbstract)
            {
                throw new NotSupportedException($"implemention type:{implementionType.FullName} must not be interface and abstract!");
            }

            if (implementionType.IsValueType)
            {
                throw new NotSupportedException($"implemention type:{implementionType.FullName} must not be value type");
            }

            return
                ModuleBuilder.DefineType(
                   GetTypeName(baseType),
                   TypeAttributes.Class | TypeAttributes.Public,
                   baseType.IsInterface ? typeof(object) : baseType,
                   baseType.IsInterface ? new[] { baseType } : null
               );
        }

        public static void AssignFieldWithMethodInfo(ILGenerator il, FieldBuilder field, MethodInfo method)
        {
            var getMethod = typeof(Type).GetMethod("GetMethod", new[] { typeof(string), typeof(BindingFlags), typeof(Binder), typeof(Type[]), typeof(ParameterModifier[]) });
            var parameterTypes = method.GetParameters().Select(info => info.ParameterType).ToArray();
            var GetTypeFromRuntimeHandleMethod = typeof(Type).GetMethod("GetTypeFromHandle");

            il.Emit(OpCodes.Ldtoken, method.DeclaringType);

            il.Emit(OpCodes.Ldstr, method.Name);
            il.Emit(OpCodes.Ldc_I4, (int)(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ldc_I4, parameterTypes.Length);
            il.Emit(OpCodes.Newarr, typeof(Type));

            for (var i = 0; i < parameterTypes.Length; i++)
            {
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldtoken, parameterTypes[i]);
                il.Emit(OpCodes.Stelem, typeof(Type));
            }

            il.Emit(OpCodes.Ldnull);
            il.EmitCall(OpCodes.Call, getMethod, null);
            il.Emit(OpCodes.Stsfld, field);
        }

        public static IEnumerable<MethodInfo> GetOverrideableMethods(Type type)
        {
            var properties = type.GetProperties();
            var typeOfObject = typeof(object);

            foreach (var item in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (properties.Any(i => i.GetMethod == item || i.SetMethod == item))
                {
                    continue;
                }

                if (item.DeclaringType == typeOfObject)
                {
                    continue;
                }

                if (type.IsInterface || item.IsVirtual)
                {
                    yield return item;
                }
            }
        }

        public static Type GenerateProxyType(Type implType, Type baseType)
        {
            var methodIntereceptors = GetMethodDefinedIntereceptors(baseType);

            var typeBuilder = DefineType(implType, baseType);
            var fileds = DefineFields(typeBuilder, methodIntereceptors.Values.SelectMany(i => i).Concat(new[] { implType }).Distinct().ToArray());

            DefineConstructors(typeBuilder, baseType, methodIntereceptors.SelectMany(i => i.Value).Concat(new[] { implType }).Distinct().ToArray(), fileds);

            DefineMethods(typeBuilder, implType, baseType, fileds, methodIntereceptors);

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
            Type implType,
            Type baseType,
            Dictionary<Type, FieldBuilder> fileds,
            Dictionary<MethodInfo, List<Type>> methodInterecetpors
        )
        {
            var wrappedObject = fileds[implType];

            foreach (var item in baseType.GetMethods())
            {
                var paramterInfos = item.GetParameters();
                var paramterTypes = paramterInfos.Select(i => i.ParameterType).ToArray();
                var methodBudiler = typeBuilder.DefineMethod(
                       item.Name,
                       MethodAttributes.Virtual | MethodAttributes.Public,
                       item.ReturnType,
                       paramterTypes);

                var il = methodBudiler.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, wrappedObject);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Ldc_I4, paramterTypes.Length);
                il.Emit(OpCodes.Newarr, typeof(object));

                for (var i = 0; i < paramterInfos.Length; i++)
                {
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldarg, i + 1);

                    if (paramterInfos[i].ParameterType.IsValueType)
                    {
                        il.Emit(OpCodes.Box, paramterInfos[i].ParameterType);
                    }

                    il.Emit(OpCodes.Stelem_Ref);
                }

                il.Emit(OpCodes.Ldc_I4, methodInterecetpors[item].Count);
                il.Emit(OpCodes.Newarr, typeof(IIntereceptor));

                for (var i = 0; i < methodInterecetpors[item].Count; i++)
                {
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, fileds[methodInterecetpors[item][i]]);
                    il.Emit(OpCodes.Stelem_Ref);
                }

                il.Emit(OpCodes.Call, typeof(TypeGeneratorUtil).GetMethod(nameof(Invoke), BindingFlags.Static | BindingFlags.Public));
                il.Emit(OpCodes.Ret);
            }
        }


        public static object Invoke(
            object target,
            MethodInfo methodInfo,
            object[] parameters,
            IIntereceptor[] intereceptors)
        {
            Console.WriteLine("222");
            return null;
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
