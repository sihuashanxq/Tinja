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
            var refLocalBuilders = new Dictionary<int, LocalBuilder>();

            var dyMethod = new DynamicMethod(methodInfo.Name, valueType, new[] { typeof(object), typeof(object[]) });
            var ilGenerator = dyMethod.GetILGenerator();
            var returnValue = ilGenerator.DeclareLocal(valueType);

            ilGenerator.LoadArgument(0);

            LoadMethodArguments(ilGenerator, methodInfo, refLocalBuilders);

            ilGenerator.Call(methodInfo);
            ilGenerator.Emit(methodInfo.ReturnType.IsVoid() ? OpCodes.Ldnull : OpCodes.Nop);

            ilGenerator.SetVariableValue(returnValue);

            SetMethodRefArguments(ilGenerator, refLocalBuilders);

            ilGenerator.LoadVariable(returnValue);
            ilGenerator.Return();

            return dyMethod.CreateDelegate(typeof(Func<,,>).MakeGenericType(typeof(object), typeof(object[]), valueType));
        }

        internal static void SetMethodRefArguments(ILGenerator ilGen, Dictionary<int, LocalBuilder> refLocalBuilders)
        {
            foreach (var item in refLocalBuilders)
            {
                ilGen.SetArrayElement(
                    _ => ilGen.LoadArgument(1),
                    _ => ilGen.Emit(OpCodes.Ldloc, item.Value),
                    item.Key,
                    item.Value.LocalType
                );
            }
        }

        internal static void LoadMethodArguments(ILGenerator ilGen, MethodInfo methodInfo, Dictionary<int, LocalBuilder> refLocalBuilders)
        {
            var parameterInfos = methodInfo.GetParameters();
            if (parameterInfos.Length == 0)
            {
                return;
            }

            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var parameterInfo = parameterInfos[i];
                var parameterType = parameterInfo.ParameterType;
                if (parameterType.IsByRef)
                {
                    var elementType = parameterType.GetElementType();
                    refLocalBuilders[i] = ilGen.DeclareLocal(elementType);

                    ilGen.LoadArrayElement(_ => ilGen.LoadArgument(1), i, parameterType);
                    ilGen.SetVariableValue(refLocalBuilders[i]);
                    ilGen.LoadVariableRef(refLocalBuilders[i]);
                    continue;
                }

                ilGen.LoadArrayElement(_ => ilGen.LoadArgument(1), i, parameterType);
            }
        }
    }
}
