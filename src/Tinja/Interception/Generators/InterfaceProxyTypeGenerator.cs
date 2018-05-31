using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Extension;

namespace Tinja.Interception.Generators
{
    public class InterfaceProxyTypeGenerator : ProxyTypeGenerator
    {
        public InterfaceProxyTypeGenerator(Type interfaceType)
            : base(interfaceType, interfaceType)
        {

        }

        #region Method

        protected override MethodBuilder CreateTypeMethod(MethodInfo methodInfo, PropertyInfo property)
        {
            var paramterTypes = methodInfo.GetParameters().Select(i => i.ParameterType).ToArray();
            var methodAttributes = GetMethodAttributes(methodInfo);
            var methodBudiler = TypeBuilder.DefineMethod(
                methodInfo.Name,
                methodAttributes,
                CallingConventions.HasThis,
                methodInfo.ReturnType,
                paramterTypes
            );

            CreateGenericParameters(methodBudiler, methodInfo);
            CreateTypeMethodCustomAttributes(methodBudiler, methodInfo);

            var ilGen = methodBudiler.GetILGenerator();

            //this.__executor
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, GetField("__executor"));

            //this.executor.Execute(new MethodInvocation)
            ilGen.Emit(OpCodes.Ldarg_0);
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

            if (property == null)
            {
                ilGen.Emit(OpCodes.Ldsfld, GetField(methodInfo));
                ilGen.Emit(OpCodes.Call, typeof(MemberInterceptorFilter).GetMethod("Filter"));
                ilGen.Emit(OpCodes.Newobj, NewMethodInvocation);
            }
            else
            {
                ilGen.Emit(OpCodes.Ldsfld, GetField(property));
                ilGen.Emit(OpCodes.Call, typeof(MemberInterceptorFilter).GetMethod("Filter"));
                ilGen.Emit(OpCodes.Ldsfld, GetField(property));
                ilGen.Emit(OpCodes.Newobj, NewPropertyMethodInvocation);
            }

            ilGen.Emit(OpCodes.Callvirt, MethodInvocationExecute);
            ilGen.Emit(methodInfo.IsVoidMethod() ? OpCodes.Pop : OpCodes.Nop);
            ilGen.Emit(OpCodes.Ret);

            return methodBudiler;
        }

        #endregion
    }
}
