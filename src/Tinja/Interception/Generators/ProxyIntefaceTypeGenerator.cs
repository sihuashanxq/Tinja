using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Extension;

namespace Tinja.Interception.Generators
{
    public class ProxyIntefaceTypeGenerator : ProxyTypeGenerator
    {
        public ProxyIntefaceTypeGenerator(Type baseType, Type implementionType)
            : base(baseType, implementionType)
        {

        }

        protected override void CreateTypeFields()
        {
            CreateField("__target", ImplementionType, FieldAttributes.Private);
            base.CreateTypeFields();
        }

        protected override void CreateTypeDefaultConstructor()
        {
            var ilGen = TypeBuilder
                .DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, ExtraConstrcutorParameters)
                .GetILGenerator();

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.Emit(OpCodes.Stfld, GetField("__target"));

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_2);
            ilGen.Emit(OpCodes.Ldtoken, BaseType);
            ilGen.Emit(OpCodes.Ldtoken, ImplementionType);
            ilGen.Emit(OpCodes.Call, typeof(IInterceptorCollector).GetMethod("Collect"));
            ilGen.Emit(OpCodes.Stsfld, GetField("__interceptors"));

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_3);
            ilGen.Emit(OpCodes.Stsfld, GetField("__executor"));

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarga, 4);
            ilGen.Emit(OpCodes.Stsfld, GetField("__filter"));

            ilGen.Emit(OpCodes.Ret);
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
            ilGen.Emit(OpCodes.Ldfld, GetField("__target"));

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

        protected virtual EventBuilder CreateTypeEvent(EventInfo @event)
        {
            var eventBuilder = TypeBuilder.DefineEvent(@event.Name, @event.Attributes, @event.EventHandlerType);

            if (@event.AddMethod != null)
            {
                var addMethod = CreateTypeMethod(@event.AddMethod);
                if (addMethod != null)
                {
                    eventBuilder.SetAddOnMethod(addMethod);
                }
            }

            if (@event.RemoveMethod != null)
            {
                var removeMethod = CreateTypeMethod(@event.RemoveMethod);
                if (removeMethod != null)
                {
                    eventBuilder.SetRemoveOnMethod(removeMethod);
                }
            }

            if (@event.RaiseMethod != null)
            {
                var raiseMethod = CreateTypeMethod(@event.RaiseMethod);
                if (raiseMethod != null)
                {
                    eventBuilder.SetRaiseMethod(raiseMethod);
                }
            }

            return eventBuilder;
        }
    }
}
