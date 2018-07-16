using System;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Extensions;
using Tinja.Interception.Generators.Extensions;

namespace Tinja.Interception.Generators
{
    public class InterfaceProxyTypeGenerator : ProxyTypeGenerator
    {
        public InterfaceProxyTypeGenerator(Type interfaceType, IInterceptorDescriptorCollector collector)
            : base(interfaceType, interfaceType, collector)
        {

        }

        #region Method

        protected override MethodBuilder DefineTypeMethod(MethodInfo methodInfo)
        {
            var methodBudiler = TypeBuilder.DefineMethod(methodInfo);
            if (!IsUsedInterception(methodInfo))
            {
                return methodBudiler.MakeDefaultMethodBody(methodInfo);
            }

            var ilGen = methodBudiler.GetILGenerator();
            var parameters = methodInfo.GetParameters();

            var arguments = ilGen.DeclareLocal(typeof(object[]));
            var methodReturnValue = ilGen.DeclareLocal(methodInfo.IsVoidMethod() ? typeof(object) : methodInfo.ReturnType);

            ilGen.MakeArgumentArray(parameters);
            ilGen.SetVariableValue(arguments);

            //this.__executor
            ilGen.LoadThisField(GetField("__executor"));

            //this.executor.Execute(new MethodInvocation)
            ilGen.This();

            ilGen.LoadStaticField(GetField(methodInfo));

            ilGen.LoadMethodGenericArguments(methodInfo);

            //new Parameters[]
            ilGen.LoadVariable(arguments);

            ilGen.LoadThisField(GetField("__filter"));
            ilGen.LoadThisField(GetField("__interceptors"));
            ilGen.LoadStaticField(GetField(methodInfo));

            ilGen.Call(GeneratorUtility.FilterInterceptor);
            ilGen.New(GeneratorUtility.NewMethodInvocation);

            ilGen.InvokeMethodInvocation(methodInfo);

            ilGen.SetVariableValue(methodReturnValue);

            //update ref out
            ilGen.SetRefArgumentsWithArray(parameters, arguments);

            ilGen.LoadVariable(methodReturnValue);
            ilGen.Emit(methodInfo.IsVoidMethod() ? OpCodes.Pop : OpCodes.Nop);
            ilGen.Return();


            return methodBudiler;
        }

        protected override MethodBuilder DefineTypePropertyMethod(MethodInfo methodInfo, PropertyInfo property)
        {
            var methodBuilder = TypeBuilder.DefineMethod(methodInfo);
            if (!IsUsedInterception(property))
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
            ilGen.LoadStaticField(GetField(methodInfo));

            ilGen.LoadMethodGenericArguments(methodInfo);

            //new Parameters[]
            ilGen.LoadVariable(arguments);

            ilGen.LoadThisField(GetField("__filter"));
            ilGen.LoadThisField(GetField("__interceptors"));
            ilGen.LoadStaticField(GetField(property));
            ilGen.Call(GeneratorUtility.FilterInterceptor);

            ilGen.LoadStaticField(GetField(property));
            ilGen.New(GeneratorUtility.NewPropertyMethodInvocation);

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
    }
}
