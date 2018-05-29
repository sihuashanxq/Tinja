using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using Tinja.Extension;
using Tinja.Interception.Executors;
using Tinja.Interception.TypeMembers;

namespace Tinja.Interception.Generators
{
    public class ProxyTypeGenerator : IProxyTypeGenerator
    {
        protected static MethodInfo MethodInvocationExecute { get; }

        protected static ConstructorInfo NewMethodInvocation { get; }

        protected static ConstructorInfo NewPropertyMethodInvocation { get; }

        protected Type BaseType { get; }

        protected Type ImplementionType { get; }

        protected TypeBuilder TypeBuilder { get; set; }

        protected IEnumerable<TypeMember> TypeMembers { get; }

        protected Dictionary<string, FieldBuilder> Fields { get; }

        protected virtual Type[] ExtraConstrcutorParameters => new[]
        {
            typeof(IInterceptorCollector),
            typeof(IMethodInvocationExecutor)
        };

        static ProxyTypeGenerator()
        {
            MethodInvocationExecute = typeof(IMethodInvocationExecutor).GetMethod("Execute");

            NewMethodInvocation = typeof(MethodInvocation).GetConstructor(new[]
            {
                typeof(object),
                typeof(MethodInfo),
                typeof(Type[]),
                typeof(object[]),
                typeof(IInterceptor[])
            });

            NewPropertyMethodInvocation = typeof(MethodPropertyInvocation).GetConstructor(new[]
            {
                typeof(object),
                typeof(MethodInfo),
                typeof(Type[]),
                typeof(object[]),
                typeof(IInterceptor[]),
                typeof(PropertyInfo)
            });
        }

        public ProxyTypeGenerator(Type baseType, Type implemetionType)
        {
            BaseType = baseType;
            ImplementionType = implemetionType;
            TypeMembers = TypeMemberCollector.Collect(BaseType, implemetionType);
            TypeBuilder = TypeGeneratorUtil.DefineType(ImplementionType, BaseType);

            Fields = new Dictionary<string, FieldBuilder>();
        }

        public virtual Type CreateProxyType()
        {
            CreateTypeFields();

            CreateTypeMethods();

            CreateTypeProperties();

            CreateTypeConstrcutors();

            CreateGenericParameters(TypeBuilder, ImplementionType);

            CreateTypeCustomAttribute(TypeBuilder, ImplementionType);

            return TypeBuilder.CreateType();
        }

        #region Field

        protected virtual void CreateTypeFields()
        {
            CreateField("__executor", typeof(IMethodInvocationExecutor), FieldAttributes.Private);
            CreateField("__interceptors", typeof(IEnumerable<InterceptionTargetBinding>), FieldAttributes.Private);
            CreateField("__filter", typeof(MemberInterceptorFilter), FieldAttributes.Private);

            foreach (var item in TypeMembers.Where(i => i.IsProperty).Select(i => i.Member.AsProperty()))
            {
                CreateField(GetMemberIdentifier(item), typeof(PropertyInfo), FieldAttributes.Private | FieldAttributes.Static);
            }

            foreach (var item in TypeMembers.Where(i => i.IsMethod).Select(i => i.Member.AsMethod()))
            {
                CreateField(GetMemberIdentifier(item), typeof(MethodInfo), FieldAttributes.Private | FieldAttributes.Static);
            }
        }

        public FieldBuilder GetField(string field)
        {
            return Fields.GetValueOrDefault(field);
        }

        public FieldBuilder GetField(MemberInfo memberInfo)
        {
            return GetField(GetMemberIdentifier(memberInfo));
        }

        public FieldBuilder CreateField(string field, Type fieldType, FieldAttributes attributes)
        {
            if (!Fields.ContainsKey(field))
            {
                return Fields[field] = TypeBuilder.DefineField(field, fieldType, attributes);
            }

            return Fields[field];
        }

        #endregion

        #region Method

        protected virtual void CreateTypeMethods()
        {
            foreach (var item in TypeMembers.Where(i => i.IsMethod))
            {
                CreateTypeMethod(item.Member.AsMethod());
            }
        }

        /// <summary>
        /// Create Method
        /// </summary>
        /// <param name="methodInfo"></param>
        protected virtual MethodBuilder CreateTypeMethod(MethodInfo methodInfo)
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

            CreateGenericParameters(methodBudiler, methodInfo);
            CreateTypeMethodCustomAttributes(methodBudiler, methodInfo);

            var ilGen = methodBudiler.GetILGenerator();

            //this.__executor
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, GetField("__executor"));

            //this.executor.Execute(new MethodInvocation)
            ilGen.Emit(OpCodes.Ldarg_0);
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

        #region Property

        protected virtual void CreateTypeProperties()
        {
            foreach (var item in TypeMembers.Where(i => i.IsProperty))
            {
                CreateTypeProperty(item.Member.AsProperty());
            }
        }

        protected virtual PropertyBuilder CreateTypeProperty(PropertyInfo propertyInfo)
        {
            var propertyBuilder = TypeBuilder.DefineProperty(
                propertyInfo.Name,
                propertyInfo.Attributes,
                propertyInfo.PropertyType,
                propertyInfo.GetIndexParameters().Select(i => i.ParameterType).ToArray()
            );

            CreateTypePropertyCustomAttributes(propertyBuilder, propertyInfo);

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

        #endregion  

        #region Constructors

        protected virtual void CreateTypeConstrcutors()
        {
            CreateTypeDefaultStaticConstrcutor();

            var bases = GetBaseConstructorInfos();
            if (!bases.Any())
            {
                CreateTypeDefaultConstructor();
                return;
            }

            foreach (var item in bases)
            {
                CreateTypeConstructor(item);
            }
        }

        protected virtual void CreateTypeConstructor(ConstructorInfo consturctor)
        {
            var parameters = consturctor.GetParameters().Select(i => i.ParameterType).ToArray();
            var constructorBuilder = TypeBuilder.DefineConstructor(consturctor.Attributes, consturctor.CallingConvention, ExtraConstrcutorParameters.Concat(parameters).ToArray());
            var ilGen = constructorBuilder.GetILGenerator();

            CreateTypeConstructorCustomAttributes(constructorBuilder, consturctor);

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
            ilGen.Emit(OpCodes.Newobj, typeof(MemberInterceptorFilter).GetConstructor(Type.EmptyTypes));
            ilGen.Emit(OpCodes.Stfld, GetField("__filter"));

            ilGen.Emit(OpCodes.Ldarg_0);

            for (var i = ExtraConstrcutorParameters.Length; i < parameters.Length; i++)
            {
                ilGen.Emit(OpCodes.Ldarg, i + 1);
            }

            ilGen.Emit(OpCodes.Call, consturctor);
            ilGen.Emit(OpCodes.Ret);
        }

        protected virtual void CreateTypeDefaultConstructor()
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
            ilGen.Emit(OpCodes.Newobj, typeof(MemberInterceptorFilter).GetConstructor(Type.EmptyTypes));
            ilGen.Emit(OpCodes.Stfld, GetField("__filter"));

            ilGen.Emit(OpCodes.Ret);
        }

        protected virtual void CreateTypeDefaultStaticConstrcutor()
        {
            var ilGen = TypeBuilder
                .DefineConstructor(MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, Type.EmptyTypes)
                .GetILGenerator();

            foreach (var item in TypeMembers.Where(i => i.IsProperty).Select(i => i.Member.AsProperty()))
            {
                ilGen.LoadPropertyInfo(item);
                ilGen.Emit(OpCodes.Stsfld, GetField(item));
            }

            foreach (var item in TypeMembers.Where(i => i.IsMethod).Select(i => i.Member.AsMethod()))
            {
                ilGen.LoadMethodInfo(item);
                ilGen.Emit(OpCodes.Stsfld, GetField(item));
            }

            ilGen.Emit(OpCodes.Ret);
        }

        protected virtual ConstructorInfo[] GetBaseConstructorInfos()
        {
            return ImplementionType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        #endregion

        protected virtual MethodAttributes GetMethodAttributes(MethodInfo methodInfo)
        {
            if (BaseType.IsInterface)
            {
                return MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
            }

            var attributes = MethodAttributes.HideBySig | MethodAttributes.Virtual;
            if (methodInfo.IsPublic)
            {
                return MethodAttributes.Public | attributes;
            }

            if (methodInfo.IsFamily)
            {
                return MethodAttributes.Family | attributes;
            }

            if (methodInfo.IsFamilyAndAssembly)
            {
                return MethodAttributes.FamANDAssem | attributes;
            }

            if (methodInfo.IsFamilyOrAssembly)
            {
                return MethodAttributes.FamORAssem | attributes;
            }

            if (methodInfo.IsPrivate)
            {
                return MethodAttributes.Private | attributes;
            }

            return attributes;
        }

        /// <summary>
        /// 获取MemberInfo 标识符
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        protected static string GetMemberIdentifier(MemberInfo memberInfo)
        {
            return "__proxy__member__" + memberInfo.Name + "_" + memberInfo.GetHashCode();
        }

        #region Attribute

        private static void CreateTypeCustomAttribute(TypeBuilder typeBuilder, Type target)
        {
            foreach (var customAttriute in target.CustomAttributes)
            {
                typeBuilder.SetCustomAttribute(CreateCustomAttribute(customAttriute));
            }
        }

        private static void CreateTypeMethodCustomAttributes(MethodBuilder methodBuilder, MethodInfo methodInfo)
        {
            foreach (var customAttriute in methodInfo.CustomAttributes)
            {
                methodBuilder.SetCustomAttribute(CreateCustomAttribute(customAttriute));
            }
        }

        private static void CreateTypePropertyCustomAttributes(PropertyBuilder propertyBuilder, PropertyInfo propertyInfo)
        {
            foreach (var customAttriute in propertyInfo.CustomAttributes)
            {
                propertyBuilder.SetCustomAttribute(CreateCustomAttribute(customAttriute));
            }
        }

        private static void CreateTypeConstructorCustomAttributes(ConstructorBuilder constructorBuilder, ConstructorInfo constructorInfo)
        {
            foreach (var customAttriute in constructorInfo.CustomAttributes)
            {
                constructorBuilder.SetCustomAttribute(CreateCustomAttribute(customAttriute));
            }
        }

        private static void CreateTypeConstructorCustomAttributes(ParameterBuilder parameterBuilder, ConstructorInfo constructorInfo)
        {
            foreach (var customAttriute in constructorInfo.CustomAttributes)
            {
                parameterBuilder.SetCustomAttribute(CreateCustomAttribute(customAttriute));
            }
        }

        private static CustomAttributeBuilder CreateCustomAttribute(CustomAttributeData customAttribute)
        {
            if (customAttribute.NamedArguments == null)
            {
                return new CustomAttributeBuilder(customAttribute.Constructor, customAttribute.ConstructorArguments.Select(c => c.Value).ToArray());
            }

            var args = new object[customAttribute.ConstructorArguments.Count];
            for (var i = 0; i < args.Length; i++)
            {
                if (typeof(IEnumerable).IsAssignableFrom(customAttribute.ConstructorArguments[i].ArgumentType))
                {
                    args[i] = (customAttribute.ConstructorArguments[i].Value as IEnumerable<CustomAttributeTypedArgument>).Select(x => x.Value).ToArray();
                    continue;
                }

                args[i] = customAttribute.ConstructorArguments[i].Value;
            }

            var namedProperties = customAttribute
                .NamedArguments
                .Where(n => !n.IsField)
                .Select(n => customAttribute.AttributeType.GetProperty(n.MemberName))
                .ToArray();

            var properties = customAttribute
                .NamedArguments
                .Where(n => !n.IsField)
                .Select(n => n.TypedValue.Value)
                .ToArray();

            var namedFields = customAttribute
                .NamedArguments
                .Where(n => n.IsField)
                .Select(n => customAttribute.AttributeType.GetField(n.MemberName))
                .ToArray();

            var fields = customAttribute
                .NamedArguments
                .Where(n => n.IsField)
                .Select(n => n.TypedValue.Value)
                .ToArray();

            return new CustomAttributeBuilder(customAttribute.Constructor, args
               , namedProperties
               , properties, namedFields, fields);
        }

        #endregion

        #region Generic

        private static void CreateGenericParameters(TypeBuilder typeBuilder, Type target)
        {
            if (!target.IsGenericType)
            {
                return;
            }

            var genericArguments = target.GetGenericArguments();
            var genericArgumentBuilders = typeBuilder.DefineGenericParameters(genericArguments.Select(i => i.Name).ToArray());

            SetGenericParameterConstraints(genericArgumentBuilders, genericArguments);
        }

        private static void CreateGenericParameters(MethodBuilder methodBuilder, MethodInfo target)
        {
            if (!target.IsGenericMethod)
            {
                return;
            }

            var genericArguments = target.GetGenericArguments();
            var genericArgumentBuilders = methodBuilder.DefineGenericParameters(genericArguments.Select(i => i.Name).ToArray());

            SetGenericParameterConstraints(genericArgumentBuilders, genericArguments);
        }

        private static void SetGenericParameterConstraints(GenericTypeParameterBuilder[] genericArgumentBuilders, Type[] genericArguments)
        {
            for (var i = 0; i < genericArguments.Length; i++)
            {
                foreach (var constraint in genericArguments[i].GetGenericParameterConstraints())
                {
                    if (constraint.IsInterface)
                    {
                        genericArgumentBuilders[i].SetInterfaceConstraints(constraint);
                        continue;
                    }

                    genericArgumentBuilders[i].SetBaseTypeConstraint(constraint);
                }
            }
        }

        #endregion
    }
}
