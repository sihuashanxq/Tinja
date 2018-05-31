using System;
using System.Reflection;
using Tinja.Interception.Executors;

namespace Tinja.Interception.Generators.Utils
{
    public static class ProxyTypeGeneratorUtility
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
    }
}
