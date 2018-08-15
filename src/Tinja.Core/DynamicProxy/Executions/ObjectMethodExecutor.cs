using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Abstractions.DynamicProxy.Executions;
using Tinja.Abstractions.Extensions;
using Tinja.Core.DynamicProxy.Generators.Extensions;

namespace Tinja.Core.DynamicProxy.Executions
{
    internal class ObjectMethodExecutor : IObjectMethodExecutor
    {
        protected Delegate MethodExecutor { get; }

        public MethodInfo MethodInfo { get; }

        public ObjectMethodExecutor(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo ?? throw new NullReferenceException(nameof(methodInfo));
            MethodExecutor = CreateMethodExecutor(MethodInfo);
        }

        public TResult Execute<TResult>(object instance, object[] parameterValues)
        {
            return ((Func<object, object[], TResult>)MethodExecutor)(instance, parameterValues);
        }

        internal static Delegate CreateMethodExecutor(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new NullReferenceException(nameof(methodInfo));
            }

            var valueType = methodInfo.IsVoidMethod() ? typeof(object) : methodInfo.ReturnType;
            var parameters = methodInfo.GetParameters();
            var localBuilders = new Dictionary<int, LocalBuilder>();

            var dyMethod = new DynamicMethod(methodInfo.Name, valueType, new[] { typeof(object), typeof(object[]) });
            var ilGenerator = dyMethod.GetILGenerator();
            var returnValue = ilGenerator.DeclareLocal(valueType);

            ilGenerator.LoadArgument(0);

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (!parameter.ParameterType.IsByRef)
                {
                    ilGenerator.LoadArrayElement(_ => ilGenerator.LoadArgument(1), i, parameter.ParameterType);
                    continue;
                }

                localBuilders[i] = ilGenerator.DeclareLocal(parameter.ParameterType.GetElementType());

                ilGenerator.LoadArrayElement(_ => ilGenerator.LoadArgument(1), i, parameter.ParameterType);
                ilGenerator.SetVariableValue(localBuilders[i]);
                ilGenerator.LoadVariableRef(localBuilders[i]);
            }

            ilGenerator.Call(methodInfo);
            ilGenerator.Emit(methodInfo.ReturnType.IsVoid() ? OpCodes.Ldnull : OpCodes.Nop);

            ilGenerator.SetVariableValue(returnValue);

            foreach (var kv in localBuilders)
            {
                ilGenerator.SetArrayElement(
                    _ => ilGenerator.LoadArgument(1),
                    _ => ilGenerator.Emit(OpCodes.Ldloc, kv.Value),
                    kv.Key,
                    kv.Value.LocalType
                );
            }

            ilGenerator.LoadVariable(returnValue);
            ilGenerator.Return();

            return dyMethod.CreateDelegate(typeof(Func<,,>).MakeGenericType(typeof(object), typeof(object[]), valueType));
        }
    }
}
