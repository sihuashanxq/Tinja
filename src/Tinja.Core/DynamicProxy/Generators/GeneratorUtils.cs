using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Abstractions.Extensions;
using Tinja.Core.DynamicProxy.Executions;

namespace Tinja.Core.DynamicProxy.Generators
{
    public static class GeneratorUtils
    {
        private const string AssemblyName = "Tinja.Core.DynamicProxy";

        private const string ModuleName = "ProxyModules";

        internal static ModuleBuilder ModuleBuilder { get; }

        internal static AssemblyBuilder AssemblyBuilder { get; }

        internal static Dictionary<Type, int> ProxyIndexs { get; }

        internal static readonly ConstructorInfo NewMethodInvocation = typeof(MethodInvocation).GetConstructor(new[]
        {
            typeof(object),
            typeof(MemberInfo),
            typeof(MethodInfo),
            typeof(Type[]),
            typeof(object[])
        });

        internal static readonly MethodInfo MethodInvocationExecute = typeof(MethodInvocationExecutor)
            .GetMethods()
            .FirstOrDefault(item => item.Name == "Execute" && !item.IsVoidMethod());

        internal static readonly MethodInfo MethodVoidInvocationExecute = typeof(MethodInvocationExecutor)
            .GetMethods()
            .FirstOrDefault(item => item.Name == "Execute" && item.IsVoidMethod());

        internal static readonly MethodInfo MethodInvocationAsyncExecute = typeof(MethodInvocationExecutor)
            .GetMethods()
            .FirstOrDefault(item => item.Name == "ExecuteAsync" && item.IsGenericMethod);

        internal static readonly MethodInfo MethodInvocationVoidAsyncExecute = typeof(MethodInvocationExecutor).
            GetMethods()
            .FirstOrDefault(item => item.Name == "ExecuteAsync" && !item.IsGenericMethod);

        internal static readonly MethodInfo MethodInvocationValueTaskAsyncExecute = typeof(MethodInvocationExecutor)
            .GetMethods()
            .FirstOrDefault(item => item.Name == "ExecuteValueTaskAsync" && item.IsGenericMethod);

        internal static readonly MethodInfo MethodInvocationValueTaskVoidAsyncExecute = typeof(MethodInvocationExecutor)
            .GetMethods()
            .FirstOrDefault(item => item.Name == "ExecuteValueTaskAsync" && !item.IsGenericMethod);

        internal static readonly MethodInfo MethodBuildInvoker = typeof(MethodInvocationInvokerBuilder)
            .GetMethod("BuildInvoker");

        internal static readonly MethodInfo MethodBuildAsyncInvoker = typeof(MethodInvocationInvokerBuilder)
            .GetMethods()
            .FirstOrDefault(item => item.Name == "BuildTaskAsyncInvoker" && item.IsGenericMethod);

        internal static readonly MethodInfo MethodBuildAsyncVoidInvoker = typeof(MethodInvocationInvokerBuilder)
            .GetMethods()
            .FirstOrDefault(item => item.Name == "BuildTaskAsyncInvoker" && !item.IsGenericMethod);

        internal static readonly MethodInfo MethodBuildValueTaskAsyncInvoker = typeof(MethodInvocationInvokerBuilder)
            .GetMethods()
            .FirstOrDefault(item => item.Name == "BuildValueTaskAsyncInvoker" && item.IsGenericMethod);

        internal static readonly MethodInfo MethodBuildVoidValueTaskAsyncInvoker = typeof(MethodInvocationInvokerBuilder)
            .GetMethods()
            .FirstOrDefault(item => item.Name == "BuildValueTaskAsyncInvoker" && !item.IsGenericMethod);

        static GeneratorUtils()
        {
            ProxyIndexs = new Dictionary<Type, int>();
            AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(AssemblyName), AssemblyBuilderAccess.Run);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule(ModuleName);
        }

        internal static string GetProxyTypeName(Type type)
        {
            lock (ProxyIndexs)
            {
                var order = ProxyIndexs.GetValueOrDefault(type);
                if (order == 0)
                {
                    ProxyIndexs[type] = 1;
                }
                else
                {
                    ProxyIndexs[type] = order + 1;
                }

                return type.FullName + "." + type.Name + "_proxy_" + order;
            }
        }

        internal static CustomAttributeBuilder CreateCustomAttribute(CustomAttributeData customAttribute)
        {
            if (customAttribute.NamedArguments == null)
            {
                return new CustomAttributeBuilder(customAttribute.Constructor, customAttribute.ConstructorArguments.Select(c => c.Value).ToArray());
            }

            var args = new object[customAttribute.ConstructorArguments.Count];
            for (var i = 0; i < args.Length; i++)
            {
                if (typeof(IEnumerable).IsAssignableFrom(customAttribute.ConstructorArguments[i].ArgumentType))
                {
                    args[i] = (customAttribute.ConstructorArguments[i].Value as IEnumerable<CustomAttributeTypedArgument>)?.Select(x => x.Value).ToArray();
                    continue;
                }

                args[i] = customAttribute.ConstructorArguments[i].Value;
            }

            var namedProperties = customAttribute
                .NamedArguments
                .Where(n => !n.IsField)
                .Select(n => customAttribute.AttributeType.GetProperty(n.MemberName))
                .ToArray();

            var properties = customAttribute
                .NamedArguments
                .Where(n => !n.IsField)
                .Select(n => n.TypedValue.Value)
                .ToArray();

            var namedFields = customAttribute
                .NamedArguments
                .Where(n => n.IsField)
                .Select(n => customAttribute.AttributeType.GetField(n.MemberName))
                .ToArray();

            var fields = customAttribute
                .NamedArguments
                .Where(n => n.IsField)
                .Select(n => n.TypedValue.Value)
                .ToArray();

            return new CustomAttributeBuilder(customAttribute.Constructor, args
               , namedProperties
               , properties, namedFields, fields);
        }
    }
}
