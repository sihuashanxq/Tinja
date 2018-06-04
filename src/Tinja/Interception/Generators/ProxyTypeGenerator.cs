using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Extension;
using Tinja.Interception.Generators.Utils;
using Tinja.Interception.Members;

namespace Tinja.Interception.Generators
{
    public class ProxyTypeGenerator : IProxyTypeGenerator
    {
        protected Type ServiceType { get; }

        protected Type ProxyTargetType { get; }

        protected TypeBuilder TypeBuilder { get; set; }

        protected IEnumerable<ProxyMember> ProxyMembers { get; }

        protected Dictionary<string, FieldBuilder> Fields { get; }

        protected IMemberInterceptionProvider InterceptionProvider { get; }

        protected IEnumerable<MemberInterception> MemberInterceptions { get; }

        protected virtual Type[] DefaultConstrcutorParameters => new[]
        {
            typeof(IInterceptorCollector),
            typeof(IMethodInvocationExecutor)
        };

        public ProxyTypeGenerator(Type serviceType, Type implemetionType, IMemberInterceptionProvider provider)
        {
            ServiceType = serviceType;
            ProxyTargetType = implemetionType;
            InterceptionProvider = provider;
            ProxyMembers = MemberCollectorFactory
                .Default
                .Create(serviceType, implemetionType)
                .Collect();

            MemberInterceptions = InterceptionProvider.GetInterceptions(serviceType, implemetionType);
            Fields = new Dictionary<string, FieldBuilder>();
        }

        public virtual Type CreateProxyType()
        {
            CreateTypeBuilder();

            CreateTypeFields();

            CreateTypeMethods();

            CreateTypeProperties();

            CreateTypeConstrcutors();

            CreateGenericParameters(TypeBuilder, ProxyTargetType);

            CreateTypeCustomAttribute(TypeBuilder, ProxyTargetType);

            return TypeBuilder.CreateType();
        }

        protected virtual void CreateTypeBuilder()
        {
            if (ProxyTargetType.IsValueType)
            {
                throw new NotSupportedException($"implemention type:{ProxyTargetType.FullName} must not be value type");
            }

            TypeBuilder = GeneratorUtility.ModuleBuilder.DefineType(
                  GeneratorUtility.GetProxyTypeName(ProxyTargetType),
                  TypeAttributes.Class | TypeAttributes.Public,
                  ProxyTargetType.IsInterface ? typeof(object) : ProxyTargetType,
                  ProxyTargetType.IsInterface ? new[] { ProxyTargetType } : ProxyTargetType.GetInterfaces()
              );
        }

        #region Field

        protected virtual void CreateTypeFields()
        {
            CreateField("__executor", typeof(IMethodInvocationExecutor), FieldAttributes.Private);
            CreateField("__interceptors", typeof(IEnumerable<MemberInterceptionBinding>), FieldAttributes.Private);
            CreateField("__filter", typeof(MemberInterceptorFilter), FieldAttributes.Private);

            foreach (var item in ProxyMembers.Where(i => i.IsProperty).Select(i => i.Member.AsProperty()))
            {
                CreateField(GetMemberIdentifier(item), typeof(PropertyInfo), FieldAttributes.Private | FieldAttributes.Static);
            }

            foreach (var item in ProxyMembers.Where(i => i.IsMethod).Select(i => i.Member.AsMethod()))
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
            foreach (var item in ProxyMembers.Where(i => i.IsMethod))
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
            return CreateTypeMethod(methodInfo);
        }

        protected virtual MethodBuilder CreateTypePropertyMethod(MethodInfo methodInfo, PropertyInfo property)
        {
            return null;
        }

        #endregion

        #region Property

        protected virtual void CreateTypeProperties()
        {
            foreach (var item in ProxyMembers.Where(i => i.IsProperty))
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
                var setter = CreateTypePropertyMethod(propertyInfo.SetMethod, propertyInfo);
                if (setter == null)
                {
                    throw new NullReferenceException(nameof(setter));
                }

                propertyBuilder.SetSetMethod(setter);
            }

            if (propertyInfo.CanRead)
            {
                var getter = CreateTypePropertyMethod(propertyInfo.GetMethod, propertyInfo);
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
            CreateTypeDefaultConstructor();
            CreateTypeStaticConstrcutor();
        }

        protected virtual void CreateTypeDefaultConstructor()
        {
            var ilGen = TypeBuilder
                .DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, DefaultConstrcutorParameters)
                .GetILGenerator();

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

            ilGen.Emit(OpCodes.Ret);
        }

        protected virtual void CreateTypeStaticConstrcutor()
        {
            var ilGen = TypeBuilder
                .DefineConstructor(MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, Type.EmptyTypes)
                .GetILGenerator();

            foreach (var item in ProxyMembers.Where(i => i.IsProperty).Select(i => i.Member.AsProperty()))
            {
                ilGen.LoadPropertyInfo(item);
                ilGen.Emit(OpCodes.Stsfld, GetField(item));
            }

            foreach (var item in ProxyMembers.Where(i => i.IsMethod).Select(i => i.Member.AsMethod()))
            {
                ilGen.LoadMethodInfo(item);
                ilGen.Emit(OpCodes.Stsfld, GetField(item));
            }

            ilGen.Emit(OpCodes.Ret);
        }

        #endregion

        protected virtual bool ContainsInterception(MemberInfo memberInfo)
        {
            return MemberInterceptions.Any(i => i.Prioritys.Any(n => n.Key == memberInfo || n.Key == memberInfo.DeclaringType));
        }

        protected virtual MethodAttributes GetMethodAttributes(MethodInfo methodInfo)
        {
            if (ServiceType.IsInterface)
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

        protected static void CreateTypeCustomAttribute(TypeBuilder typeBuilder, Type target)
        {
            foreach (var customAttriute in target.CustomAttributes)
            {
                typeBuilder.SetCustomAttribute(CreateCustomAttribute(customAttriute));
            }
        }

        protected static void CreateTypeMethodCustomAttributes(MethodBuilder methodBuilder, MethodInfo methodInfo)
        {
            foreach (var customAttriute in methodInfo.CustomAttributes)
            {
                methodBuilder.SetCustomAttribute(CreateCustomAttribute(customAttriute));
            }
        }

        protected static void CreateTypePropertyCustomAttributes(PropertyBuilder propertyBuilder, PropertyInfo propertyInfo)
        {
            foreach (var customAttriute in propertyInfo.CustomAttributes)
            {
                propertyBuilder.SetCustomAttribute(CreateCustomAttribute(customAttriute));
            }
        }

        protected static void CreateTypeConstructorCustomAttributes(ConstructorBuilder constructorBuilder, ConstructorInfo constructorInfo)
        {
            foreach (var customAttriute in constructorInfo.CustomAttributes)
            {
                constructorBuilder.SetCustomAttribute(CreateCustomAttribute(customAttriute));
            }
        }

        protected static void CreateTypeParameterCustomAttributes(ParameterBuilder parameterBuilder, ConstructorInfo constructorInfo)
        {
            foreach (var customAttriute in constructorInfo.CustomAttributes)
            {
                parameterBuilder.SetCustomAttribute(CreateCustomAttribute(customAttriute));
            }
        }

        protected static CustomAttributeBuilder CreateCustomAttribute(CustomAttributeData customAttribute)
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
                    args[i] = (customAttribute.ConstructorArguments[i].Value as IEnumerable<CustomAttributeTypedArgument>)?.Select(x => x.Value).ToArray();
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

        protected static void CreateGenericParameters(TypeBuilder typeBuilder, Type target)
        {
            if (!target.IsGenericType)
            {
                return;
            }

            var genericArguments = target.GetGenericArguments();
            var genericArgumentBuilders = typeBuilder.DefineGenericParameters(genericArguments.Select(i => i.Name).ToArray());

            SetGenericParameterConstraints(genericArgumentBuilders, genericArguments);
        }

        protected static void CreateGenericParameters(MethodBuilder methodBuilder, MethodInfo target)
        {
            if (!target.IsGenericMethod)
            {
                return;
            }

            var genericArguments = target.GetGenericArguments();
            var genericArgumentBuilders = methodBuilder.DefineGenericParameters(genericArguments.Select(i => i.Name).ToArray());

            SetGenericParameterConstraints(genericArgumentBuilders, genericArguments);
        }

        protected static void SetGenericParameterConstraints(GenericTypeParameterBuilder[] genericArgumentBuilders, Type[] genericArguments)
        {
            for (var i = 0; i < genericArguments.Length; i++)
            {
                foreach (var constraint in genericArguments[i].GetGenericParameterConstraints())
                {
                    if (constraint.IsInterface)
                    {
                        genericArgumentBuilders[i].SetInterfaceConstraints(constraint);
                        genericArgumentBuilders[i].SetGenericParameterAttributes(genericArguments[i].GenericParameterAttributes);
                        continue;
                    }

                    genericArgumentBuilders[i].SetBaseTypeConstraint(constraint);
                    genericArgumentBuilders[i].SetGenericParameterAttributes(genericArguments[i].GenericParameterAttributes);
                }
            }
        }

        #endregion
    }
}
