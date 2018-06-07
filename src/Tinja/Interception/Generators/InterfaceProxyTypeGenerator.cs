using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Extensions;
using Tinja.Interception.Generators.Extensions;
using Tinja.Interception.Generators.Utils;

namespace Tinja.Interception.Generators
{
    public class InterfaceProxyTypeGenerator : ProxyTypeGenerator
    {
        public InterfaceProxyTypeGenerator(Type interfaceType, IMemberInterceptionProvider provider)
            : base(interfaceType, interfaceType, provider)
        {

        }

        #region Method

        protected override MethodBuilder DefineTypeMethod(MethodInfo methodInfo)
        {
            var parameterInfos = methodInfo.GetParameters();
            var parameterTypes = methodInfo.GetParameters().Select(i => i.ParameterType).ToArray();
            var methodAttributes = GetMethodAttributes(methodInfo);
            var methodBudiler = TypeBuilder
                .DefineMethod(methodInfo.Name, methodAttributes, CallingConventions.HasThis, methodInfo.ReturnType, parameterTypes)
                .SetCustomAttributes(methodInfo)
                .DefineParameters(methodInfo)
                .DefineReturnParameter(methodInfo)
                .DefineGenericParameters(methodInfo);

            var ilGen = methodBudiler.GetILGenerator();

            if (!IsUsedInterception(methodInfo))
            {
                ilGen.BuildDefaultMethodBody(methodInfo.ReturnType);
                return methodBudiler;
            }

            var arguments = ilGen.DeclareLocal(typeof(object[]));
            var methodReturnValue = ilGen.DeclareLocal(methodInfo.IsVoidMethod() ? typeof(object) : methodInfo.ReturnType);

            ilGen.NewArray(typeof(object), parameterTypes.Length);

            for (var i = 0; i < parameterTypes.Length; i++)
            {
                ilGen.SetArrayElement(
                    _ => ilGen.Emit(OpCodes.Dup),
                    _ => ilGen.Emit(OpCodes.Ldarg, i + 1),
                    i,
                    parameterTypes[i]
                );
            }

            ilGen.SetVariableValue(arguments);

            //this.__executor
            ilGen.LoadThisField(GetField("__executor"));

            //this.executor.Execute(new MethodInvocation)
            ilGen.This();
            ilGen.TypeOf(ProxyTargetType);

            ilGen.LoadStaticField(GetField(methodInfo));

            ilGen.LoadMethodGenericArguments(methodInfo);

            //new Parameters[]
            ilGen.LoadVariable(arguments);

            ilGen.LoadThisField(GetField("__filter"));
            ilGen.LoadThisField(GetField("__interceptors"));
            ilGen.LoadStaticField(GetField(methodInfo));

            ilGen.Call(GeneratorUtility.MemberInterceptorFilter);
            ilGen.New(GeneratorUtility.NewMethodInvocation);

            ilGen.CallVirt(GeneratorUtility.MethodInvocationExecute);
            ilGen.SetVariableValue(methodReturnValue);

            //update ref out
            for (var argIndex = 0; argIndex < parameterInfos.Length; argIndex++)
            {
                var parameterInfo = parameterInfos[argIndex];
                if (!parameterInfo.ParameterType.IsByRef || parameterInfo.IsIn)
                {
                    continue;
                }

                ilGen.LoadArgument(argIndex + 1);
                ilGen.LoadArrayElement(_ => ilGen.Emit(OpCodes.Ldloc, arguments), argIndex, parameterInfo.ParameterType);
                ilGen.Emit(OpCodes.Stind_Ref);
            }

            ilGen.LoadVariable(methodReturnValue);
            ilGen.Emit(methodInfo.IsVoidMethod() ? OpCodes.Pop : OpCodes.Nop);
            ilGen.Return();


            return methodBudiler;
        }

        protected override MethodBuilder DefineTypePropertyMethod(MethodInfo methodInfo, PropertyInfo property)
        {
            var parameterInfos = methodInfo.GetParameters();
            var parameterTypes = methodInfo.GetParameters().Select(i => i.ParameterType).ToArray();
            var methodAttributes = GetMethodAttributes(methodInfo);
            var methodBudiler = TypeBuilder
                .DefineMethod(methodInfo.Name, methodAttributes, CallingConventions.HasThis, methodInfo.ReturnType, parameterTypes)
                .SetCustomAttributes(methodInfo)
                .DefineParameters(methodInfo)
                .DefineReturnParameter(methodInfo)
                .DefineGenericParameters(methodInfo);

            var ilGen = methodBudiler.GetILGenerator();
            if (!IsUsedInterception(property))
            {
                ilGen.BuildDefaultMethodBody(methodInfo.ReturnType);
                return methodBudiler;
            }

            var arguments = ilGen.DeclareLocal(typeof(object[]));
            var methodReturnValue = ilGen.DeclareLocal(methodInfo.IsVoidMethod() ? typeof(object) : methodInfo.ReturnType);

            ilGen.NewArray(typeof(object), parameterTypes.Length);

            for (var i = 0; i < parameterTypes.Length; i++)
            {
                ilGen.SetArrayElement(
                    _ => ilGen.Emit(OpCodes.Dup),
                    _ => ilGen.Emit(OpCodes.Ldarg, i + 1),
                    i,
                    parameterTypes[i]
                );
            }

            ilGen.SetVariableValue(arguments);

            //this.__executor
            ilGen.LoadThisField(GetField("__executor"));

            //this.executor.Execute(new MethodInvocation)
            ilGen.This();
            ilGen.TypeOf(ProxyTargetType);
            ilGen.LoadStaticField(GetField(methodInfo));

            ilGen.LoadMethodGenericArguments(methodInfo);

            //new Parameters[]
            ilGen.LoadVariable(arguments);

            ilGen.LoadThisField(GetField("__filter"));
            ilGen.LoadThisField(GetField("__interceptors"));
            ilGen.LoadStaticField(GetField(property));
            ilGen.Call(GeneratorUtility.MemberInterceptorFilter);

            ilGen.LoadStaticField(GetField(property));
            ilGen.New(GeneratorUtility.NewPropertyMethodInvocation);

            ilGen.CallVirt(GeneratorUtility.MethodInvocationExecute);
            ilGen.SetVariableValue(methodReturnValue);

            //update ref out
            for (var argIndex = 0; argIndex < parameterInfos.Length; argIndex++)
            {
                var parameterInfo = parameterInfos[argIndex];
                if (!parameterInfo.ParameterType.IsByRef || parameterInfo.IsIn)
                {
                    continue;
                }

                ilGen.LoadArgument(argIndex + 1);
                ilGen.LoadArrayElement(_ => ilGen.Emit(OpCodes.Ldloc, arguments), argIndex, parameterInfo.ParameterType);
                ilGen.Emit(OpCodes.Stind_Ref);
            }

            ilGen.LoadVariable(methodReturnValue);
            ilGen.Emit(methodInfo.IsVoidMethod() ? OpCodes.Pop : OpCodes.Nop);
            ilGen.Return();

            return methodBudiler;
        }

        #endregion
    }
}
