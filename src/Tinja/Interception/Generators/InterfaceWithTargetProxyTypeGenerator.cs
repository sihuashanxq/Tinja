using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Extensions;
using Tinja.Interception.Generators.Extensions;
using Tinja.Interception.Generators.Utils;

namespace Tinja.Interception.Generators
{
    public class InterfaceWithTargetProxyTypeGenerator : ProxyTypeGenerator
    {
        protected override Type[] DefaultConstrcutorParameters { get; }

        public InterfaceWithTargetProxyTypeGenerator(Type interaceType, Type implemetionType, IMemberInterceptionProvider provider)
            : base(interaceType, implemetionType, provider)
        {
            DefaultConstrcutorParameters = new[]
            {
                implemetionType,
                typeof(IInterceptorCollector),
                typeof(IMethodInvocationExecutor)
            };
        }

        #region Method

        protected override MethodBuilder DefineTypeMethod(MethodInfo methodInfo)
        {
            var parameterInfos = methodInfo.GetParameters();
            var parameterTypes = parameterInfos.Select(i => i.ParameterType).ToArray();
            var methodAttributes = GetMethodAttributes(methodInfo);
            var methodBudiler = TypeBuilder
                .DefineMethod(methodInfo.Name, methodAttributes, CallingConventions.HasThis, methodInfo.ReturnType, parameterTypes)
                .SetCustomAttributes(methodInfo)
                .DefineParameters(methodInfo)
                .DefineReturnParameter(methodInfo)
                .DefineGenericParameters(methodInfo);

            var ilGen = methodBudiler.GetILGenerator();
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
            ilGen.LoadThisField(GetField("__target"));

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
            var parameterTypes = parameterInfos.Select(i => i.ParameterType).ToArray();
            var methodAttributes = GetMethodAttributes(methodInfo);
            var methodBudiler = TypeBuilder
                .DefineMethod(methodInfo.Name, methodAttributes, CallingConventions.HasThis, methodInfo.ReturnType, parameterTypes)
                .SetCustomAttributes(methodInfo)
                .DefineParameters(methodInfo)
                .DefineReturnParameter(methodInfo)
                .DefineGenericParameters(methodInfo);

            var ilGen = methodBudiler.GetILGenerator();
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
            ilGen.LoadThisField(GetField("__target"));

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

        protected override void DefineTypeFields()
        {
            DefineField("__target", ProxyTargetType, FieldAttributes.Private);
            base.DefineTypeFields();
        }

        protected override void DefineTypeDefaultConstructor()
        {
            var ilGen = TypeBuilder
                .DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, DefaultConstrcutorParameters)
                .GetILGenerator();

            ilGen.SetThisField(GetField("__target"), _ => ilGen.LoadArgument(1));

            ilGen.SetThisField(
                GetField("__interceptors"),
                _ =>
                {
                    ilGen.LoadArgument(2);
                    ilGen.TypeOf(ServiceType);
                    ilGen.TypeOf(ProxyTargetType);
                    ilGen.CallVirt(typeof(IInterceptorCollector).GetMethod("Collect"));
                }
            );

            ilGen.SetThisField(GetField("__executor"), _ => ilGen.LoadArgument(3));
            ilGen.SetThisField(GetField("__executor"), _ => ilGen.New(typeof(MemberInterceptorFilter).GetConstructor(Type.EmptyTypes)));

            ilGen.Return();
        }

        protected override void DefineTypeConstrcutors()
        {
            DefineTypeStaticConstrcutor();
            DefineTypeDefaultConstructor();
        }
    }
}
