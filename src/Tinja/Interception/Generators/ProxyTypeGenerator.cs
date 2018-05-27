using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Extension;
using Tinja.Interception.Executors;
using Tinja.Interception.TypeMembers;

namespace Tinja.Interception
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
            typeof(IMethodInvocationExecutor),
            typeof(IMemberInterceptorFilter)
        };

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

            NewPropertyMethodInvocation = typeof(MethodPropertyInvocation).GetConstructor(new[]
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

        protected virtual void CreateTypeConstructor(ConstructorInfo consturctor)
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
            ilGen.Emit(OpCodes.Ldarg_3);
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
    }
}
