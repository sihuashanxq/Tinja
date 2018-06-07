using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Extensions;
using Tinja.Interception.Generators.Extensions;
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
            DefineTypeBuilder();

            DefineTypeFields();

            DefineTypeMethods();

            DefineTypeProperties();

            DefineTypeConstrcutors();

            return TypeBuilder.CreateType();
        }

        protected virtual void DefineTypeBuilder()
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
                    ProxyTargetType.IsInterface ? typeof(object) : ProxyTargetType,
                    ProxyTargetType.IsInterface ? new[] { ProxyTargetType } : ProxyTargetType.GetInterfaces()
                )
                .DefineGenericParameters(ProxyTargetType)
                .SetCustomAttributes(ProxyTargetType);
        }

        #region Field

        protected virtual void DefineTypeFields()
        {
            DefineField("__executor", typeof(IMethodInvocationExecutor), FieldAttributes.Private);
            DefineField("__interceptors", typeof(IEnumerable<MemberInterceptionBinding>), FieldAttributes.Private);
            DefineField("__filter", typeof(MemberInterceptorFilter), FieldAttributes.Private);

            foreach (var item in ProxyMembers.Where(i => i.IsProperty).Select(i => i.Member.AsProperty()))
            {
                DefineField(GetMemberIdentifier(item), typeof(PropertyInfo), FieldAttributes.Private | FieldAttributes.Static);
            }

            foreach (var item in ProxyMembers.Where(i => i.IsMethod).Select(i => i.Member.AsMethod()))
            {
                DefineField(GetMemberIdentifier(item), typeof(MethodInfo), FieldAttributes.Private | FieldAttributes.Static);
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

        public FieldBuilder DefineField(string field, Type fieldType, FieldAttributes attributes)
        {
            if (!Fields.ContainsKey(field))
            {
                return Fields[field] = TypeBuilder.DefineField(field, fieldType, attributes);
            }

            return Fields[field];
        }

        #endregion

        #region Method

        protected virtual void DefineTypeMethods()
        {
            foreach (var item in ProxyMembers.Where(i => i.IsMethod))
            {
                DefineTypeMethod(item.Member.AsMethod());
            }
        }

        /// <summary>
        /// Create Method
        /// </summary>
        /// <param name="methodInfo"></param>
        protected virtual MethodBuilder DefineTypeMethod(MethodInfo methodInfo)
        {
            return DefineTypeMethod(methodInfo);
        }

        protected virtual MethodBuilder DefineTypePropertyMethod(MethodInfo methodInfo, PropertyInfo property)
        {
            return null;
        }

        #endregion

        #region Property

        protected virtual void DefineTypeProperties()
        {
            foreach (var item in ProxyMembers.Where(i => i.IsProperty))
            {
                DefineTypeProperty(item.Member.AsProperty());
            }
        }

        protected virtual PropertyBuilder DefineTypeProperty(PropertyInfo propertyInfo)
        {
            var propertyBuilder = TypeBuilder
                .DefineProperty(
                    propertyInfo.Name,
                    propertyInfo.Attributes,
                    propertyInfo.PropertyType,
                    propertyInfo.GetIndexParameters().Select(i => i.ParameterType).ToArray()
                )
                .SetCustomAttributes(propertyInfo);

            if (propertyInfo.CanWrite)
            {
                var setter = DefineTypePropertyMethod(propertyInfo.SetMethod, propertyInfo);
                if (setter == null)
                {
                    throw new NullReferenceException(nameof(setter));
                }

                propertyBuilder.SetSetMethod(setter);
            }

            if (propertyInfo.CanRead)
            {
                var getter = DefineTypePropertyMethod(propertyInfo.GetMethod, propertyInfo);
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

        protected virtual void DefineTypeConstrcutors()
        {
            DefineTypeDefaultConstructor();
            DefineTypeStaticConstrcutor();
        }

        protected virtual void DefineTypeDefaultConstructor()
        {
            var ilGen = TypeBuilder
                .DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, DefaultConstrcutorParameters)
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

            ilGen.Return();
        }

        protected virtual void DefineTypeStaticConstrcutor()
        {
            var ilGen = TypeBuilder
                .DefineConstructor(MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, Type.EmptyTypes)
                .GetILGenerator();

            foreach (var item in ProxyMembers.Where(i => i.IsProperty).Select(i => i.Member.AsProperty()))
            {
                ilGen.SetStaticField(GetField(item), _ => ilGen.LoadPropertyInfo(item));
            }

            foreach (var item in ProxyMembers.Where(i => i.IsMethod).Select(i => i.Member.AsMethod()))
            {
                ilGen.SetStaticField(GetField(item), _ => ilGen.LoadMethodInfo(item));
            }

            ilGen.Return();
        }

        #endregion

        protected virtual bool IsUsedInterception(MemberInfo memberInfo)
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
    }
}
