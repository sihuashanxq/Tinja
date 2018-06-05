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
            var paramterTypes = methodInfo.GetParameters().Select(i => i.ParameterType).ToArray();
            var methodAttributes = GetMethodAttributes(methodInfo);
            var methodBudiler = TypeBuilder
                .DefineMethod(methodInfo.Name, methodAttributes, CallingConventions.HasThis, methodInfo.ReturnType, paramterTypes)
                .SetCustomAttributes(methodInfo)
                .DefineParameters(methodInfo)
                .DefineReturnParameter(methodInfo)
                .DefineGenericParameters(methodInfo);

            var ilGen = methodBudiler.GetILGenerator();

            //this.__executor
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, GetField("__executor"));

            //this.executor.Execute(new MethodInvocation)
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, GetField("__target"));

            ilGen.Emit(OpCodes.Ldtoken, ProxyTargetType);
            ilGen.Emit(OpCodes.Ldsfld, GetField(methodInfo));

            if (methodInfo.IsGenericMethod)
            {
                ilGen.LoadMethodGenericArguments(methodInfo);
            }
            else
            {
                ilGen.Emit(OpCodes.Ldnull);
            }

            //new Parameters[]
            ilGen.Emit(OpCodes.Ldc_I4, paramterTypes.Length);
            ilGen.Emit(OpCodes.Newarr, typeof(object));

            for (var i = 0; i < paramterTypes.Length; i++)
            {
                ilGen.Emit(OpCodes.Dup);
                ilGen.Emit(OpCodes.Ldc_I4, i);
                ilGen.Emit(OpCodes.Ldarg, i + 1);
                ilGen.Box(paramterTypes[i]);
                ilGen.Emit(OpCodes.Stelem_Ref);
            }

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, GetField("__filter"));

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, GetField("__interceptors"));

            ilGen.Emit(OpCodes.Ldsfld, GetField(methodInfo));
            ilGen.Emit(OpCodes.Call, GeneratorUtility.MemberInterceptorFilter);
            ilGen.Emit(OpCodes.Newobj, GeneratorUtility.NewMethodInvocation);

            ilGen.Emit(OpCodes.Callvirt, GeneratorUtility.MethodInvocationExecute);
            ilGen.Emit(methodInfo.IsVoidMethod() ? OpCodes.Pop : OpCodes.Nop);
            ilGen.Emit(OpCodes.Ret);

            return methodBudiler;
        }

        protected override MethodBuilder DefineTypePropertyMethod(MethodInfo methodInfo, PropertyInfo property)
        {
            var paramterTypes = methodInfo.GetParameters().Select(i => i.ParameterType).ToArray();
            var methodAttributes = GetMethodAttributes(methodInfo);
            var methodBudiler = TypeBuilder
                .DefineMethod(methodInfo.Name, methodAttributes, CallingConventions.HasThis, methodInfo.ReturnType, paramterTypes)
                .SetCustomAttributes(methodInfo)
                .DefineParameters(methodInfo)
                .DefineReturnParameter(methodInfo)
                .DefineGenericParameters(methodInfo);

            var ilGen = methodBudiler.GetILGenerator();

            //this.__executor
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, GetField("__executor"));

            //this.executor.Execute(new MethodInvocation)
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, GetField("__target"));

            ilGen.Emit(OpCodes.Ldtoken, ProxyTargetType);
            ilGen.Emit(OpCodes.Ldsfld, GetField(methodInfo));

            if (methodInfo.IsGenericMethod)
            {
                ilGen.LoadMethodGenericArguments(methodInfo);
            }
            else
            {
                ilGen.Emit(OpCodes.Ldnull);
            }

            //new Parameters[]
            ilGen.Emit(OpCodes.Ldc_I4, paramterTypes.Length);
            ilGen.Emit(OpCodes.Newarr, typeof(object));

            for (var i = 0; i < paramterTypes.Length; i++)
            {
                ilGen.Emit(OpCodes.Dup);
                ilGen.Emit(OpCodes.Ldc_I4, i);
                ilGen.Emit(OpCodes.Ldarg, i + 1);
                ilGen.Box(paramterTypes[i]);
                ilGen.Emit(OpCodes.Stelem_Ref);
            }

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, GetField("__filter"));

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, GetField("__interceptors"));

            ilGen.Emit(OpCodes.Ldsfld, GetField(property));
            ilGen.Emit(OpCodes.Call, GeneratorUtility.MemberInterceptorFilter);
            ilGen.Emit(OpCodes.Ldsfld, GetField(property));
            ilGen.Emit(OpCodes.Newobj, GeneratorUtility.NewPropertyMethodInvocation);

            ilGen.Emit(OpCodes.Callvirt, GeneratorUtility.MethodInvocationExecute);
            ilGen.Emit(methodInfo.IsVoidMethod() ? OpCodes.Pop : OpCodes.Nop);
            ilGen.Emit(OpCodes.Ret);

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

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.Emit(OpCodes.Stfld, GetField("__target"));

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_2);
            ilGen.Emit(OpCodes.Ldtoken, ServiceType);
            ilGen.Emit(OpCodes.Ldtoken, ProxyTargetType);
            ilGen.Emit(OpCodes.Call, typeof(IInterceptorCollector).GetMethod("Collect"));
            ilGen.Emit(OpCodes.Stfld, GetField("__interceptors"));

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_3);
            ilGen.Emit(OpCodes.Stfld, GetField("__executor"));

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Newobj, typeof(MemberInterceptorFilter).GetConstructor(Type.EmptyTypes));
            ilGen.Emit(OpCodes.Stfld, GetField("__filter"));

            ilGen.Emit(OpCodes.Ret);
        }

        protected override void DefineTypeConstrcutors()
        {
            DefineTypeStaticConstrcutor();
            DefineTypeDefaultConstructor();
        }
    }
}
