using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Extensions;
using Tinja.Interception.Generators.Extensions;
using Tinja.Interception.Generators.Utils;

namespace Tinja.Interception.Generators
{
    public class ClassProxyTypeGenerator : ProxyTypeGenerator
    {
        public ClassProxyTypeGenerator(Type targetToProxy, IMemberInterceptionProvider provider)
        : base(targetToProxy, targetToProxy, provider)
        {

        }

        #region Method

        protected override void DefineTypeMethods()
        {
            foreach (var item in ProxyMembers.Where(i => i.IsMethod).Select(i => i.Member.AsMethod()))
            {
                if (!IsUsedInterception(item) && !item.IsAbstract)
                {
                    continue;
                }

                DefineTypeMethod(item);
            }
        }

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

            if (methodInfo.IsAbstract)
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

        protected override PropertyBuilder DefineTypeProperty(PropertyInfo propertyInfo)
        {
            if (!IsUsedInterception(propertyInfo))
            {
                if (propertyInfo.CanRead && !propertyInfo.GetMethod.IsAbstract)
                {
                    return null;
                }

                if (propertyInfo.CanWrite && !propertyInfo.SetMethod.IsAbstract)
                {
                    return null;
                }
            }

            return base.DefineTypeProperty(propertyInfo);
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

            if (methodInfo.IsAbstract)
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

        protected override void DefineTypeConstrcutors()
        {
            foreach (var item in ProxyTargetType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                CreateTypeConstructor(item);
            }

            DefineTypeStaticConstrcutor();
        }

        protected virtual void CreateTypeConstructor(ConstructorInfo consturctor)
        {
            var parameterInfos = consturctor.GetParameters();
            var parameterTypes = parameterInfos.Select(i => i.ParameterType).ToArray();
            var ilGen = TypeBuilder
                .DefineConstructor(consturctor.Attributes, consturctor.CallingConvention, DefaultConstrcutorParameters.Concat(parameterTypes).ToArray())
                .SetCustomAttributes(consturctor)
                .DefineParameters(parameterInfos, parameterInfos.Length + DefaultConstrcutorParameters.Length)
                .GetILGenerator();

            ilGen.SetThisField(
                GetField("__interceptors"),
                _ =>
                {
                    ilGen.LoadArgument(1);
                    ilGen.TypeOf(ServiceType);
                    ilGen.TypeOf(ProxyTargetType);
                    ilGen.CallVirt(typeof(IInterceptorCollector).GetMethod("Collect"));
                }
            );

            ilGen.SetThisField(GetField("__executor"), _ => ilGen.LoadArgument(2));
            ilGen.SetThisField(GetField("__filter"), _ => ilGen.New(typeof(MemberInterceptorFilter).GetConstructor(Type.EmptyTypes)));

            var baseArgs = new List<Action<ILGenerator>>();
            var argIndex = DefaultConstrcutorParameters.Length;

            if (parameterInfos.Length > 0)
            {
                for (; argIndex < parameterTypes.Length; argIndex++)
                {
                    baseArgs.Add(_ => ilGen.LoadArgument(argIndex + 1));
                }
            }

            ilGen.Base(consturctor, baseArgs.ToArray());
            ilGen.Return();
        }
    }
}
