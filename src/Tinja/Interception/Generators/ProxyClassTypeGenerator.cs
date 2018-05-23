using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Extension;

namespace Tinja.Interception.Generators
{
    public class ProxyClassTypeGenerator : ProxyTypeGenerator
    {
        protected override Type[] ExtraConstrcutorParameters => new[]
        {
            typeof(IInterceptorCollector),
            typeof(IMethodInvocationExecutor),
            typeof(IMemberInterceptorFilter)
        };

        public ProxyClassTypeGenerator(Type baseType, Type implemetionType)
            : base(baseType, implemetionType)
        {

        }

        /// <summary>
        /// Create Method
        /// </summary>
        /// <param name="methodInfo"></param>
        protected override MethodBuilder CreateTypeMethod(MethodInfo methodInfo)
        {
            return CreateTypeMethod(methodInfo, null);
        }

        protected virtual MethodBuilder CreateTypeMethod(MethodInfo methodInfo, PropertyInfo property)
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

            var ilGen = methodBudiler.GetILGenerator();

            //this.__executor
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, GetField("__executor"));

            //this.executor.Execute(new MethodInvocation)
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldsfld, GetField(methodInfo));

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
                ilGen.Emit(OpCodes.Call, typeof(IMemberInterceptorFilter).GetMethod("Filter"));
                ilGen.Emit(OpCodes.Newobj, NewMethodInvocation);
            }
            else
            {
                ilGen.Emit(OpCodes.Ldsfld, GetField(property));
                ilGen.Emit(OpCodes.Call, typeof(IMemberInterceptorFilter).GetMethod("Filter"));

                ilGen.Emit(OpCodes.Ldsfld, GetField(property));
                ilGen.Emit(OpCodes.Newobj, NewPropertyMethodInvocation);
            }

            ilGen.Emit(OpCodes.Callvirt, MethodInvocationExecute);
            ilGen.Emit(methodInfo.IsVoidMethod() ? OpCodes.Pop : OpCodes.Nop);
            ilGen.Emit(OpCodes.Ret);

            return methodBudiler;
        }

        protected override PropertyBuilder CreateTypeProperty(PropertyInfo propertyInfo)
        {
            var propertyBuilder = TypeBuilder.DefineProperty(
                propertyInfo.Name,
                propertyInfo.Attributes,
                propertyInfo.PropertyType,
                propertyInfo.GetIndexParameters().Select(i => i.ParameterType).ToArray()
            );

            if (propertyInfo.CanWrite)
            {
                var setter = CreateTypeMethod(propertyInfo.SetMethod, propertyInfo);
                if (setter == null)
                {
                    throw new NullReferenceException(nameof(setter));
                }

                propertyBuilder.SetSetMethod(setter);
            }

            if (propertyInfo.CanRead)
            {
                var getter = CreateTypeMethod(propertyInfo.GetMethod, propertyInfo);
                if (getter == null)
                {
                    throw new NullReferenceException(nameof(getter));
                }

                propertyBuilder.SetGetMethod(getter);
            }

            return propertyBuilder;
        }

        #region Constructor

        protected override void CreateTypeConstructor(ConstructorInfo consturctor)
        {
            var parameters = consturctor.GetParameters().Select(i => i.ParameterType).ToArray();
            var ilGen = TypeBuilder
                .DefineConstructor(consturctor.Attributes, consturctor.CallingConvention, ExtraConstrcutorParameters.Concat(parameters).ToArray())
                .GetILGenerator();

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.Emit(OpCodes.Ldtoken, BaseType);
            ilGen.Emit(OpCodes.Ldtoken, ImplementionType);
            ilGen.Emit(OpCodes.Call, typeof(IInterceptorCollector).GetMethod("Collect"));
            ilGen.Emit(OpCodes.Stfld, GetField("__interceptors"));

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_2);
            ilGen.Emit(OpCodes.Stfld, GetField("__executor"));

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_3);
            ilGen.Emit(OpCodes.Stfld, GetField("__filter"));

            ilGen.Emit(OpCodes.Ldarg_0);

            for (var i = ExtraConstrcutorParameters.Length; i < parameters.Length; i++)
            {
                ilGen.Emit(OpCodes.Ldarg, i + 1);
            }

            ilGen.Emit(OpCodes.Call, consturctor);
            ilGen.Emit(OpCodes.Ret);
        }

        protected override void CreateTypeDefaultConstructor()
        {
            var ilGen = TypeBuilder
                .DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, ExtraConstrcutorParameters)
                .GetILGenerator();

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.Emit(OpCodes.Ldtoken, BaseType);
            ilGen.Emit(OpCodes.Ldtoken, ImplementionType);
            ilGen.Emit(OpCodes.Call, typeof(IInterceptorCollector).GetMethod("Collect"));
            ilGen.Emit(OpCodes.Stsfld, GetField("__interceptors"));

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_2);
            ilGen.Emit(OpCodes.Stsfld, GetField("__executor"));

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_3);
            ilGen.Emit(OpCodes.Stfld, GetField("__filter"));

            ilGen.Emit(OpCodes.Ret);
        }

        protected override ConstructorInfo[] GetBaseConstructorInfos()
        {
            return ImplementionType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        #endregion
    }
}
