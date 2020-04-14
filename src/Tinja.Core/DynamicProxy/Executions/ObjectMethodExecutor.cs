using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Abstractions.DynamicProxy.Executions;
using Tinja.Abstractions.Extensions;
using Tinja.Core.DynamicProxy.Generators.Extensions;

namespace Tinja.Core.DynamicProxy.Executions
{
    /// <summary>
    /// the default implementation of <see cref="IObjectMethodExecutor"/>
    /// </summary>
    internal class ObjectMethodExecutor : IObjectMethodExecutor
    {
        /// <summary>
        /// the target method to be wrapped
        /// </summary>
        internal MethodInfo MethodInfo { get; }

        /// <summary>
        /// a <see cref="Delegate"/> used to invoke the wrapped method.
        /// </summary>
        internal Delegate MethodExecutor { get; }

        internal ObjectMethodExecutor(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            MethodExecutor = CreateMethodExecutor(MethodInfo);
        }

        /// <summary>
        /// invoke method by the given instance and arguments
        /// </summary>
        /// <typeparam name="TResult">the type of return value</typeparam>
        /// <param name="instance"> an instance of type defined the method</param>
        /// <param name="arguments">call arguments</param>
        /// <returns></returns>
        public TResult Execute<TResult>(object instance, object[] arguments)
        {
            return ((Func<object, object[], TResult>)MethodExecutor)(instance, arguments);
        }

        internal static Delegate CreateMethodExecutor(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            var valueType = methodInfo.IsVoidMethod() ? typeof(object) : methodInfo.ReturnType;
            var refLocalBuilders = new Dictionary<int, LocalBuilder>();

            var dyMethod = new DynamicMethod(methodInfo.Name, valueType, new[] { typeof(object), typeof(object[]) });
            var ilGenerator = dyMethod.GetILGenerator();
            var returnValue = ilGenerator.DeclareLocal(valueType);

            ilGenerator.LoadArgument(0);

            LoadMethodArguments(ilGenerator, methodInfo, refLocalBuilders);

            ilGenerator.Call(methodInfo);
            ilGenerator.Emit(methodInfo.ReturnType.IsVoidType() ? OpCodes.Ldnull : OpCodes.Nop);

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
