using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Extension;
using Tinja.Interception.Generators.Utils;

namespace Tinja.Interception.Generators
{
    public class ClassProxyTypeGenerator : ProxyTypeGenerator
    {
        public ClassProxyTypeGenerator(Type targetToProxy)
        : base(targetToProxy, targetToProxy)
        {

        }

        #region Method

        protected override MethodBuilder CreateTypeMethod(MethodInfo methodInfo)
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

            ilGen.Emit(OpCodes.Ldsfld, GetField(methodInfo));
            ilGen.Emit(OpCodes.Call, GeneratorUtility.MemberInterceptorFilter);
            ilGen.Emit(OpCodes.Newobj, GeneratorUtility.NewMethodInvocation);

            ilGen.Emit(OpCodes.Callvirt, GeneratorUtility.MethodInvocationExecute);
            ilGen.Emit(methodInfo.IsVoidMethod() ? OpCodes.Pop : OpCodes.Nop);
            ilGen.Emit(OpCodes.Ret);

            return methodBudiler;
        }

        protected override MethodBuilder CreateTypePropertyMethod(MethodInfo methodInfo, PropertyInfo property)
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

        protected override void CreateTypeConstrcutors()
        {
            foreach (var item in ProxyTargetType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                CreateTypeConstructor(item);
            }

            CreateTypeStaticConstrcutor();
        }

        protected virtual void CreateTypeConstructor(ConstructorInfo consturctor)
        {
            var parameters = consturctor.GetParameters().Select(i => i.ParameterType).ToArray();
            var constructorBuilder = TypeBuilder.DefineConstructor(consturctor.Attributes, consturctor.CallingConvention, DefaultConstrcutorParameters.Concat(parameters).ToArray());
            var ilGen = constructorBuilder.GetILGenerator();

            CreateTypeConstructorCustomAttributes(constructorBuilder, consturctor);

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.Emit(OpCodes.Ldtoken, ServiceType);
            ilGen.Emit(OpCodes.Ldtoken, ProxyTargetType);
            ilGen.Emit(OpCodes.Call, typeof(IInterceptorCollector).GetMethod("Collect"));
            ilGen.Emit(OpCodes.Stfld, GetField("__interceptors"));

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_2);
            ilGen.Emit(OpCodes.Stfld, GetField("__executor"));

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Newobj, typeof(MemberInterceptorFilter).GetConstructor(Type.EmptyTypes));
            ilGen.Emit(OpCodes.Stfld, GetField("__filter"));

            ilGen.Emit(OpCodes.Ldarg_0);

            for (var i = DefaultConstrcutorParameters.Length; i < parameters.Length; i++)
            {
                ilGen.Emit(OpCodes.Ldarg, i + 1);
            }

            ilGen.Emit(OpCodes.Call, consturctor);
            ilGen.Emit(OpCodes.Ret);
        }
    }
}
