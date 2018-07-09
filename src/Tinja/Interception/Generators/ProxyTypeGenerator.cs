using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Extensions;
using Tinja.Interception.Executors;
using Tinja.Interception.Generators.Extensions;
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

        protected IMemberInterceptionCollector InterceptionCollector { get; }

        protected IEnumerable<MemberInterception> MemberInterceptions { get; }

        protected virtual Type[] DefaultConstrcutorParameters => new[]
        {
            typeof(IInterceptorCollector),
            typeof(IMethodInvocationExecutor)
        };

        protected static MethodInfo MemberInterceptorFilter = typeof(MemberInterceptorFilter).GetMethod("Filter");

        protected static ConstructorInfo NewMethodInvocation = typeof(MethodInvocation).GetConstructor(new[]
        {
            typeof(object),
            typeof(Type),
            typeof(MethodInfo),
            typeof(Type[]),
            typeof(object[]),
            typeof(IInterceptor[])
        });

        protected static ConstructorInfo NewPropertyMethodInvocation = typeof(MethodPropertyInvocation).GetConstructor(new[]
        {
            typeof(object),
            typeof(Type),
            typeof(MethodInfo),
            typeof(Type[]),
            typeof(object[]),
            typeof(IInterceptor[]),
            typeof(PropertyInfo)
        });

        public ProxyTypeGenerator(Type serviceType, Type implemetionType, IMemberInterceptionCollector collector)
        {
            ServiceType = serviceType;
            ProxyTargetType = implemetionType;
            InterceptionCollector = collector;

            ProxyMembers = MemberCollectorFactory
                .Default
                .Create(serviceType, implemetionType)
                .Collect();

            MemberInterceptions = InterceptionCollector.Collect(serviceType, implemetionType);
            Fields = new Dictionary<string, FieldBuilder>();
        }

        public virtual Type CreateProxyType()
        {
            DefineTypeBuilder();

            DefineTypeFields();

            DefineTypeEvents();

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
            return null;
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

        #region  Event

        protected virtual void DefineTypeEvents()
        {
            foreach (var @event in ProxyMembers.Where(i => i.IsEvent).Select(i => i.Member as EventInfo))
            {
                if (@event == null)
                {
                    continue;
                }

                var builder = TypeBuilder.DefineEvent(@event.Name, @event.Attributes, @event.EventHandlerType).SetCustomAttributes(@event);

                if (@event.AddMethod != null)
                {
                    builder.SetAddOnMethod(DefineTypeMethod(@event.AddMethod));
                }

                if (@event.RaiseMethod != null)
                {
                    builder.SetRaiseMethod(DefineTypeMethod(@event.RaiseMethod));
                }

                if (@event.RemoveMethod != null)
                {
                    builder.SetRemoveOnMethod(DefineTypeMethod(@event.RemoveMethod));
                }
            }
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
