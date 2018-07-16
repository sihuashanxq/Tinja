using System;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Extensions;
using Tinja.Interception.Executors;
using Tinja.Interception.Generators.Extensions;

namespace Tinja.Interception.Generators
{
    public class InterfaceTargetProxyTypeGenerator : ProxyTypeGenerator
    {
        protected override Type[] DefaultConstrcutorParameterTypes { get; }

        public InterfaceTargetProxyTypeGenerator(Type interaceType, Type implementionType, IInterceptorDescriptorCollector collector)
            : base(interaceType, implementionType, collector)
        {
            DefaultConstrcutorParameterTypes = new[]
            {
                implementionType,
                typeof(IInterceptorCollector),
                typeof(IMethodInvocationExecutor),
                typeof(InterceptorFilter)
            };
        }

        protected override void DefineTypeBuilder()
        {
            if (ProxyTargetType.IsValueType)
            {
                throw new NotSupportedException($"implemention type:{ProxyTargetType.FullName} must not be value type");
            }

            TypeBuilder = GeneratorUtility
                .ModuleBuilder
                .DefineType(
                    GeneratorUtility.GetProxyTypeName(ProxyTargetType),
                    TypeAttributes.Class | TypeAttributes.Public,
                    typeof(object),
                    new[] { ServiceType }
                )
                .DefineGenericParameters(ProxyTargetType)
                .SetCustomAttributes(ProxyTargetType);
        }

        #region Method

        protected override MethodBuilder DefineTypeMethod(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            var methodBuilder = TypeBuilder.DefineMethod(methodInfo);
            var ilGen = methodBuilder.GetILGenerator();

            var arguments = ilGen.DeclareLocal(typeof(object[]));
            var methodReturnValue = ilGen.DeclareLocal(methodInfo.IsVoidMethod() ? typeof(object) : methodInfo.ReturnType);

            ilGen.MakeArgumentArray(parameters);
            ilGen.SetVariableValue(arguments);

            //this.__executor
            ilGen.LoadThisField(GetField("__executor"));

            //this.executor.Execute(new MethodInvocation)
            ilGen.LoadThisField(GetField("__target"));

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

            return methodBuilder;
        }

        protected override MethodBuilder DefineTypePropertyMethod(MethodInfo methodInfo, PropertyInfo property)
        {
            var parameters = methodInfo.GetParameters();
            var methodBuilder = TypeBuilder.DefineMethod(methodInfo);
            var ilGen = methodBuilder.GetILGenerator();
            var arguments = ilGen.DeclareLocal(typeof(object[]));
            var methodReturnValue = ilGen.DeclareLocal(methodInfo.IsVoidMethod() ? typeof(object) : methodInfo.ReturnType);

            ilGen.MakeArgumentArray(parameters);
            ilGen.SetVariableValue(arguments);

            //this.__executor
            ilGen.LoadThisField(GetField("__executor"));

            //this.executor.Execute(new MethodInvocation)
            ilGen.LoadThisField(GetField("__target"));

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

        protected override void DefineTypeFields()
        {
            DefineField("__target", ProxyTargetType, FieldAttributes.Private);
            base.DefineTypeFields();
        }

        protected override void DefineTypeDefaultConstructor()
        {
            var ilGen = TypeBuilder
                .DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, DefaultConstrcutorParameterTypes)
                .GetILGenerator();

            ilGen.SetThisField(GetField("__target"), () => ilGen.LoadArgument(1));

            ilGen.SetThisField(
                GetField("__interceptors"),
                () =>
                {
                    ilGen.LoadArgument(2);
                    ilGen.TypeOf(ServiceType);
                    ilGen.TypeOf(ProxyTargetType);
                    ilGen.CallVirt(typeof(IInterceptorCollector).GetMethod("Collect"));
                }
            );

            ilGen.SetThisField(GetField("__executor"), () => ilGen.LoadArgument(3));
            ilGen.SetThisField(GetField("__filter"), () => ilGen.LoadArgument(4));

            ilGen.Return();
        }
    }
}
