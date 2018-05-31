using System;
using System.Collections.Concurrent;
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
            if (implementionType.IsValueType)
            {
                throw new NotSupportedException($"implemention type:{implementionType.FullName} must not be value type");
            }

            return
                ModuleBuilder.DefineType(
                   GetTypeName(baseType),
                   TypeAttributes.Class | TypeAttributes.Public,
                   implementionType.IsInterface ? typeof(object) : implementionType,
                   implementionType.GetInterfaces()
               );
        }

        static string GetTypeName(Type serviceType)
        {
            return AssemblyName + serviceType.Namespace + "." + serviceType.Name + Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}
