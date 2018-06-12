using System;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Extensions;
using Tinja.Interception.Executors;
using Tinja.Interception.Generators.Extensions;

namespace Tinja.Interception.Generators
{
    public class InterfaceWithTargetProxyTypeGenerator : ProxyTypeGenerator
    {
        protected override Type[] DefaultConstrcutorParameters { get; }

        public InterfaceWithTargetProxyTypeGenerator(Type interaceType, Type implemetionType, IMemberInterceptionCollector provider)
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

            ilGen.TypeOf(ProxyTargetType);
            ilGen.LoadStaticField(GetField(methodInfo));

            ilGen.LoadMethodGenericArguments(methodInfo);

            //new Parameters[]
            ilGen.LoadVariable(arguments);

            ilGen.LoadThisField(GetField("__filter"));
            ilGen.LoadThisField(GetField("__interceptors"));
            ilGen.LoadStaticField(GetField(methodInfo));
            ilGen.Call(MemberInterceptorFilter);

            ilGen.New(NewMethodInvocation);
            ilGen.CallVirt(MethodInvocationExecute);

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

            ilGen.CallVirt(MethodInvocationExecute);
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
