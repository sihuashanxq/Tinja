using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

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

        static string GetTypeName(Type serviceType)
        {
            return AssemblyName + serviceType.Namespace + "." + serviceType.Name + Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}
