using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy.Executions;
using Tinja.Abstractions.Extensions;
using Tinja.Core.DynamicProxy.Generators.Extensions;

namespace Tinja.Core.DynamicProxy.Executions
{
    internal class ObjectMethodExecutor : IObjectMethodExecutor
    {
        protected Func<object, object[], Task> MethodExecutor { get; }

        public MethodInfo MethodInfo { get; }

        public ObjectMethodExecutor(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo ?? throw new NullReferenceException(nameof(methodInfo));

            if (MethodInfo.ReturnType.IsTask() || MethodInfo.ReturnType.IsValueTask())
            {
                MethodExecutor = CreateTaskAsyncExecutor(MethodInfo);
            }
            else
            {
                MethodExecutor = CreateAsyncExecutorWrapper(MethodInfo);
            }
        }

        public Task ExecuteAsync(object instance, object[] paramterValues)
        {
            return MethodExecutor(instance, paramterValues);
        }

        private static Func<object, object[], Task> CreateAsyncExecutorWrapper(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new NullReferenceException(nameof(methodInfo));
            }

            var invokeDelegate = CreateMethodInvokDelegate(methodInfo);
            if (invokeDelegate == null)
            {
                throw new NullReferenceException(nameof(invokeDelegate));
            }

            var instance = Expression.Parameter(typeof(object), "instance");
            var parameter = Expression.Parameter(typeof(object[]), "parameters");
            var invocation = Expression.Invoke(Expression.Constant(invokeDelegate), instance, parameter);
            var executor = Expression.Lambda(invocation, instance, parameter).Compile();

            return (obj, args) => Task.FromResult(((Func<object, object[], object>)executor)(obj, args));
        }

        private static Func<object, object[], Task> CreateTaskAsyncExecutor(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new NullReferenceException(nameof(methodInfo));
            }

            var invokeDelegate = CreateMethodInvokDelegate(methodInfo);
            if (invokeDelegate == null)
            {
                throw new NullReferenceException(nameof(invokeDelegate));
            }

            var instance = Expression.Parameter(typeof(object), "instance");
            var parameters = Expression.Parameter(typeof(object[]), "parameters");
            var invocation = Expression.Invoke(Expression.Constant(invokeDelegate), instance, parameters);
            var asResult = Expression.Convert(invocation, typeof(Task));

            return (Func<object, object[], Task>)Expression.Lambda(asResult, instance, parameters).Compile();
        }

        private static Func<object, object[], object> CreateMethodInvokDelegate(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new NullReferenceException(nameof(methodInfo));
            }

            var dynamicMethod = new DynamicMethod(methodInfo.Name, typeof(object), new[] { typeof(object), typeof(object[]) });
            var parameters = methodInfo.GetParameters();
            var varBuilders = new Dictionary<int, LocalBuilder>();
            var ilGenerator = dynamicMethod.GetILGenerator();
            var resultValue = CreateResultValueVariable(ilGenerator, methodInfo);

            ilGenerator.LoadArgument(0);
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (parameter.ParameterType.IsByRef)
                {
                    varBuilders[i] = ilGenerator.DeclareLocal(parameter.ParameterType.GetElementType());

                    ilGenerator.LoadArrayElement(_ => ilGenerator.LoadArgument(1), i, parameter.ParameterType);
                    ilGenerator.SetVariableValue(varBuilders[i]);
                    ilGenerator.LoadVariableRef(varBuilders[i]);
                    continue;
                }

                ilGenerator.LoadArrayElement(_ => ilGenerator.LoadArgument(1), i, parameter.ParameterType);
            }

            ilGenerator.Call(methodInfo);
            ilGenerator.Emit(methodInfo.ReturnType.IsVoid() ? OpCodes.Ldnull : OpCodes.Nop);

            ilGenerator.SetVariableValue(resultValue);

            foreach (var kv in varBuilders)
            {
                ilGenerator.SetArrayElement(
                    _ => ilGenerator.LoadArgument(1),
                    _ => ilGenerator.Emit(OpCodes.Ldloc, kv.Value),
                    kv.Key,
                    kv.Value.LocalType
                );
            }

            if (methodInfo.ReturnType.IsValueTask())
            {
                ilGenerator.LoadVariableRef(resultValue);
                ilGenerator.Call(methodInfo.ReturnType.GetMethod("AsTask"));
            }
            else
            {
                ilGenerator.LoadVariable(resultValue);
                ilGenerator.Box(methodInfo.ReturnType);
            }

            ilGenerator.Return();

            return (Func<object, object[], object>)dynamicMethod.CreateDelegate(typeof(Func<object, object[], object>));
        }

        private static LocalBuilder CreateResultValueVariable(ILGenerator ilGen, MethodInfo methodInfo)
        {
            if (methodInfo.IsVoidMethod())
            {
                return ilGen.DeclareLocal(typeof(object));
            }

            return ilGen.DeclareLocal(methodInfo.ReturnType);
        }
    }
}
