using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Extension;
using Tinja.Interception.TypeMembers;

namespace Tinja.Interception
{
    public abstract class ProxyTypeGenerator : IProxyTypeGenerator
    {
        protected static MethodInfo MethodInvocationExecute { get; }

        protected static ConstructorInfo NewMethodInvocation { get; }

        protected static ConstructorInfo NewPropertyMethodInvocation { get; }

        protected Type BaseType { get; }

        protected Type ImplementionType { get; }

        protected TypeBuilder TypeBuilder { get; set; }

        protected IEnumerable<TypeMember> TypeMembers { get; }

        protected Dictionary<string, FieldBuilder> Fields { get; }

        protected virtual Type[] ExtraConstrcutorParameters { get; }

        static ProxyTypeGenerator()
        {
            MethodInvocationExecute = typeof(IMethodInvocationExecutor).GetMethod("Execute");

            NewMethodInvocation = typeof(MethodInvocation).GetConstructor(new[]
            {
                typeof(object),
                typeof(MethodInfo),
                typeof(object[]),
                typeof(IInterceptor[])
            });

            NewPropertyMethodInvocation = typeof(PropertyMethodInvocation).GetConstructor(new[]
            {
                typeof(object),
                typeof(MethodInfo),
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

            CreateTypeEvents();

            CreateTypeMethods();

            CreateTypeProperties();

            CreateTypeConstrcutors();

            return TypeBuilder.CreateType();
        }

        #region Field

        protected virtual void CreateTypeFields()
        {
            CreateField("__executor", typeof(IMethodInvocationExecutor), FieldAttributes.Private);
            CreateField("__interceptors", typeof(IEnumerable<InterceptionTargetBinding>), FieldAttributes.Private);
            CreateField("__filter", typeof(IMemberInterceptorFilter), FieldAttributes.Private);

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

        #region Event

        protected virtual void CreateTypeEvents()
        {
            foreach (var item in TypeMembers.Where(i => i.IsEvent))
            {
                CreateTypeEvent(item.Member as EventInfo);
            }
        }

        protected virtual EventBuilder CreateTypeEvent(EventInfo @event)
        {
            return null;
        }

        #endregion Event

        #region Method

        protected virtual void CreateTypeMethods()
        {
            foreach (var item in TypeMembers.Where(i => i.IsMethod))
            {
                CreateTypeMethod(item.Member.AsMethod());
            }
        }

        protected abstract MethodBuilder CreateTypeMethod(MethodInfo methodInfo);

        #endregion

        #region Property

        protected virtual void CreateTypeProperties()
        {
            foreach (var item in TypeMembers.Where(i => i.IsProperty))
            {
                CreateTypeProperty(item.Member.AsProperty());
            }
        }

        protected abstract PropertyBuilder CreateTypeProperty(PropertyInfo propertyInfo);

        #endregion  

        #region Constructors

        protected virtual void CreateTypeConstrcutors()
        {
            var bases = GetBaseConstructorInfos();
            if (bases.Any())
            {
                foreach (var item in bases)
                {
                    CreateTypeConstructor(item);
                }
            }
            else
            {
                CreateTypeDefaultConstructor();
            }

            CreateTypeDefaultStaticConstrcutor();
        }

        protected abstract void CreateTypeConstructor(ConstructorInfo constrcutor);

        protected abstract void CreateTypeDefaultConstructor();

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
            return new ConstructorInfo[0];
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
    }
}
