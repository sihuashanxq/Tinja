using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Interception.Executors;

namespace Tinja.Interception.Generators.Utils
{
    public static class GeneratorUtility
    {
        public static MethodInfo MethodInvocationExecute = typeof(IMethodInvocationExecutor).GetMethod("Execute");

        public static ConstructorInfo NewMethodInvocation = typeof(MethodInvocation).GetConstructor(new[]
        {
            typeof(object),
            typeof(Type),
            typeof(MethodInfo),
            typeof(Type[]),
            typeof(object[]),
            typeof(IInterceptor[])
        });

        public static ConstructorInfo NewPropertyMethodInvocation = typeof(MethodPropertyInvocation).GetConstructor(new[]
        {
            typeof(object),
            typeof(Type),
            typeof(MethodInfo),
            typeof(Type[]),
            typeof(object[]),
            typeof(IInterceptor[]),
            typeof(PropertyInfo)
        });

        public static MethodInfo MemberInterceptorFilter = typeof(MemberInterceptorFilter).GetMethod("Filter");

        const string AssemblyName = "Tinja.Interception.DynamicProxy";

        const string ModuleName = "ProxyModules";

        internal static ModuleBuilder ModuleBuilder { get; }

        internal static AssemblyBuilder AssemblyBuilder { get; }

        internal static Dictionary<Type, int> ProxyIndexs { get; }

        static GeneratorUtility()
        {
            AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(AssemblyName), AssemblyBuilderAccess.Run);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule(ModuleName);
            ProxyIndexs = new Dictionary<Type, int>();
        }

        public static string GetProxyTypeName(Type proxyTargetType)
        {
            lock (ProxyIndexs)
            {
                var order = ProxyIndexs.GetValueOrDefault(proxyTargetType);
                if (order == 0)
                {
                    ProxyIndexs[proxyTargetType] = 1;
                }
                else
                {
                    ProxyIndexs[proxyTargetType] = order + 1;
                }

                return proxyTargetType.FullName + "." + proxyTargetType.Name + "_proxy_" + order;
            }
        }
    }
}
