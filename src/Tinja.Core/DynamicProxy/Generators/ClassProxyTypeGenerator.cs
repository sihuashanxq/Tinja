using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Abstractions.DynamicProxy.Definitions;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Abstractions.Injection.Extensions;
using Tinja.Core.DynamicProxy.Generators.Extensions;

namespace Tinja.Core.DynamicProxy.Generators
{
    public class ClassProxyTypeGenerator : ProxyTypeGenerator
    {
        public ClassProxyTypeGenerator(Type classType, IEnumerable<MemberMetadata> members) 
            : base(classType, members)
        {

        }

        #region Method

        protected override void DefineTypeMethods()
        {
            foreach (var item in Members.Where(i => i.IsMethod).Select(i => i.Member.AsMethod()))
            {
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

            ilGen.LoadStaticField(GetField(methodInfo));
            ilGen.LoadMethodGenericArguments(methodInfo);

            ilGen.LoadVariable(arguments);

            ilGen.LoadThisField(GetField("__filter"));
            ilGen.LoadThisField(GetField("__interceptors"));
            ilGen.LoadStaticField(GetField(methodInfo));

            ilGen.Call(GeneratorUtils.GetOrCreateInterceptors);
            ilGen.New(GeneratorUtils.NewMethodInvocation);

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

            ilGen.LoadStaticField(GetField(methodInfo));

            ilGen.LoadMethodGenericArguments(methodInfo);

            //new Parameters[]
            ilGen.LoadVariable(arguments);

            ilGen.LoadThisField(GetField("__filter"));
            ilGen.LoadThisField(GetField("__interceptors"));
            ilGen.LoadStaticField(GetField(property));

            ilGen.Call(GeneratorUtils.GetOrCreateInterceptors);
            ilGen.LoadStaticField(GetField(property));

            ilGen.New(GeneratorUtils.NewPropertyMethodInvocation);
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
            foreach (var item in TargetType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
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
                .DefineConstructor(consturctor.Attributes, consturctor.CallingConvention, DefaultConstrcutorParameterTypes.Concat(parameterTypes).ToArray())
                .SetCustomAttributes(consturctor)
                .DefineParameters(parameterInfos, parameterInfos.Length + DefaultConstrcutorParameterTypes.Length)
                .GetILGenerator();

            ilGen.SetThisField(
                GetField("__interceptors"),
                () =>
                {
                    ilGen.LoadArgument(1);
                    //ilGen.TypeOf(ServiceType);
                    ilGen.TypeOf(TargetType);
                    ilGen.CallVirt(typeof(IInterceptorDefinitionCollector).GetMethod("Collect"));
                }
            );

            ilGen.SetThisField(GetField("__executor"), () => ilGen.LoadArgument(2));
            ilGen.SetThisField(GetField("__filter"), () => ilGen.LoadArgument(3));

            var baseArgs = new List<Action>();
            var startIndex = DefaultConstrcutorParameterTypes.Length;

            if (parameterInfos.Length > 0)
            {
                for (; startIndex < parameterTypes.Length; startIndex++)
                {
                    var argIndex = startIndex;
                    baseArgs.Add(() => ilGen.LoadArgument(argIndex + 1));
                }
            }

            ilGen.Base(consturctor, baseArgs.ToArray());
            ilGen.Return();
        }
    }
}
