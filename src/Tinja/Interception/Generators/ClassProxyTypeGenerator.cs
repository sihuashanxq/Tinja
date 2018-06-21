using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Extensions;
using Tinja.Interception.Generators.Extensions;

namespace Tinja.Interception.Generators
{
    public class ClassProxyTypeGenerator : ProxyTypeGenerator
    {
        public ClassProxyTypeGenerator(Type targetToProxy, IMemberInterceptionCollector provider)
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
            var methodBuilder = TypeBuilder.DefineMethod(methodInfo);
            if (methodInfo.IsAbstract)
            {
                return methodBuilder.MakeDefaultMethodBody(methodInfo);
            }

            var ilGen = methodBuilder.GetILGenerator();
            var parameters = methodInfo.GetParameters();

            var arguments = ilGen.DeclareLocal(typeof(object[]));
            var methodReturnValue = ilGen.DeclareLocal(methodInfo.IsVoidMethod() ? typeof(object) : methodInfo.ReturnType);

            ilGen.MakeArgumentArray(parameters);
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

            ilGen.Call(MemberInterceptorFilter);
            ilGen.New(NewMethodInvocation);

            ilGen.InvokeMethodInvocation(methodInfo);

            ilGen.SetVariableValue(methodReturnValue);

            //update ref out
            ilGen.SetRefArgumentsWithArray(parameters, arguments);

            ilGen.LoadVariable(methodReturnValue);
            ilGen.Emit(methodInfo.IsVoidMethod() ? OpCodes.Pop : OpCodes.Nop);
            ilGen.Return();

            return methodBuilder;
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
            var methodBuilder = TypeBuilder.DefineMethod(methodInfo);
            if (methodInfo.IsAbstract)
            {
                return methodBuilder.MakeDefaultMethodBody(methodInfo);
            }

            var ilGen = methodBuilder.GetILGenerator();
            var parameters = methodInfo.GetParameters();

            var arguments = ilGen.DeclareLocal(typeof(object[]));
            var methodReturnValue = ilGen.DeclareLocal(methodInfo.IsVoidMethod() ? typeof(object) : methodInfo.ReturnType);

            ilGen.MakeArgumentArray(parameters);
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

            ilGen.Call(MemberInterceptorFilter);
            ilGen.LoadStaticField(GetField(property));

            ilGen.New(NewPropertyMethodInvocation);
            ilGen.InvokeMethodInvocation(methodInfo);

            ilGen.SetVariableValue(methodReturnValue);

            //update ref out
            ilGen.SetRefArgumentsWithArray(parameters, arguments);

            ilGen.LoadVariable(methodReturnValue);
            ilGen.Emit(methodInfo.IsVoidMethod() ? OpCodes.Pop : OpCodes.Nop);
            ilGen.Return();

            return methodBuilder;
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
            var startIndex = DefaultConstrcutorParameters.Length;

            if (parameterInfos.Length > 0)
            {
                for (; startIndex < parameterTypes.Length; startIndex++)
                {
                    var argIndex = startIndex;
                    baseArgs.Add(_ => ilGen.LoadArgument(argIndex + 1));
                }
            }

            ilGen.Base(consturctor, baseArgs.ToArray());
            ilGen.Return();
        }
    }
}
