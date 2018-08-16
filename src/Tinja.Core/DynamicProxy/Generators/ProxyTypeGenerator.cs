using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Executions;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Abstractions.Extensions;
using Tinja.Core.DynamicProxy.Executions;
using Tinja.Core.DynamicProxy.Generators.Extensions;

namespace Tinja.Core.DynamicProxy.Generators
{
    public abstract class ProxyTypeGenerator
    {
        protected Type TargetType { get; }

        protected TypeBuilder TypeBuilder { get; set; }

        protected IEnumerable<MemberMetadata> Members { get; }

        protected Dictionary<string, FieldBuilder> Fields { get; }

        protected virtual Type[] ExtraConstrcutorParameterTypes => new[]
        {
            typeof(MethodInvocationInvokerBuilder)
        };

        protected ProxyTypeGenerator(Type targetType, IEnumerable<MemberMetadata> members)
        {
            Members = members;
            TargetType = targetType;
            Fields = new Dictionary<string, FieldBuilder>();
        }

        public virtual Type BuildProxyType()
        {
            BuildTypeBuilder();

            BuildTypeFields();

            BuildTypeEvents();

            BuildTypeMethods();

            BuildTypeProperties();

            BuildTypeConstrcutors();

            return TypeBuilder.CreateType();
        }

        protected virtual void BuildTypeBuilder()
        {
            if (TargetType.IsValueType)
            {
                throw new NotSupportedException($"implementation type:{TargetType.FullName} must not be value type");
            }

            TypeBuilder = GeneratorUtils
                .ModuleBuilder
                .DefineType(
                    GeneratorUtils.GetProxyTypeName(TargetType),
                    TypeAttributes.Class | TypeAttributes.Public,
                    TargetType.IsInterface ? typeof(object) : TargetType,
                    TargetType.IsInterface ? new[] { TargetType } : TargetType.GetInterfaces()
                )
                .DefineGenericParameters(TargetType)
                .SetCustomAttributes(TargetType);
        }

        #region Field

        protected virtual void BuildTypeFields()
        {
            BuildField("__builder", typeof(MethodInvocationInvokerBuilder), FieldAttributes.Private);

            foreach (var item in Members.Where(i => i.IsEvent).Select(i => i.Member.AsEvent()))
            {
                BuildField(GetMemberIdentifier(item), typeof(EventInfo), FieldAttributes.Private | FieldAttributes.Static);
            }

            foreach (var item in Members.Where(i => i.IsMethod).Select(i => i.Member.AsMethod()))
            {
                BuildField(GetMemberIdentifier(item), typeof(MethodInfo), FieldAttributes.Private | FieldAttributes.Static);
            }

            foreach (var item in Members.Where(i => i.IsProperty).Select(i => i.Member.AsProperty()))
            {
                if (item.CanWrite)
                {
                    BuildField(GetMemberIdentifier(item.SetMethod), typeof(MethodInfo), FieldAttributes.Private | FieldAttributes.Static);
                }

                if (item.CanRead)
                {
                    BuildField(GetMemberIdentifier(item.GetMethod), typeof(MethodInfo), FieldAttributes.Private | FieldAttributes.Static);
                }

                BuildField(GetMemberIdentifier(item), typeof(PropertyInfo), FieldAttributes.Private | FieldAttributes.Static);
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

        public FieldBuilder BuildField(string field, Type fieldType, FieldAttributes attributes)
        {
            if (!Fields.ContainsKey(field))
            {
                return Fields[field] = TypeBuilder.DefineField(field, fieldType, attributes);
            }

            return Fields[field];
        }

        #endregion

        #region Method

        protected virtual void BuildTypeMethods()
        {
            foreach (var item in Members.Where(i => i.IsMethod))
            {
                BuildMethodBody(item.Member.AsMethod(), item.Member.AsMethod());
            }
        }

        #region Method

        protected virtual MethodBuilder BuildMethodBody(MethodInfo methodInfo, MemberInfo targetMember)
        {
            var methodBuilder = TypeBuilder.DefineMethod(methodInfo);
            var ilGen = methodBuilder.GetILGenerator();
            var parameters = methodInfo.GetParameters();

            var arguments = ilGen.DeclareLocal(typeof(object[]));
            var methodReturnValue = ilGen.DeclareLocal(methodInfo.IsVoidMethod() ? typeof(object) : methodInfo.ReturnType);
            var methodInvocation = ilGen.DeclareLocal(typeof(IMethodInvocation));

            ilGen.MakeArgumentArray(parameters);
            ilGen.SetVariableValue(arguments);

            ilGen.This();
            ilGen.LoadStaticField(GetField(methodInfo));
            ilGen.LoadMethodGenericArguments(methodInfo);

            //new Parameters[]
            ilGen.LoadVariable(arguments);

            ilGen.LoadStaticField(GetField(targetMember));
            ilGen.New(GeneratorUtils.NewMethodInvocation);
            ilGen.SetVariableValue(methodInvocation);

            ilGen.LoadThisField(GetField("__builder"));
            ilGen.LoadVariable(methodInvocation);

            ilGen.InvokeBuildInvokerMethod(methodInfo);
            ilGen.LoadVariable(methodInvocation);

            ilGen.InvokeExecuteMethodInvocationMethod(methodInfo);
            ilGen.SetVariableValue(methodReturnValue);

            //update ref out
            ilGen.SetRefArgumentsWithArray(parameters, arguments);

            ilGen.LoadVariable(methodReturnValue);
            ilGen.Emit(methodInfo.IsVoidMethod() ? OpCodes.Pop : OpCodes.Nop);
            ilGen.Return();

            return methodBuilder;
        }

        #endregion

        #endregion

        #region Property

        protected virtual void BuildTypeProperties()
        {
            foreach (var item in Members.Where(i => i.IsProperty))
            {
                BuildTypeProperty(item.Member.AsProperty());
            }
        }

        protected virtual PropertyBuilder BuildTypeProperty(PropertyInfo propertyInfo)
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
                var setter = BuildMethodBody(propertyInfo.SetMethod, propertyInfo);
                if (setter == null)
                {
                    throw new NullReferenceException(nameof(setter));
                }

                propertyBuilder.SetSetMethod(setter);
            }

            if (propertyInfo.CanRead)
            {
                var getter = BuildMethodBody(propertyInfo.GetMethod, propertyInfo);
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

        protected virtual void BuildTypeEvents()
        {
            foreach (var @event in Members.Where(i => i.IsEvent).Select(i => i.Member as EventInfo))
            {
                if (@event == null)
                {
                    continue;
                }

                var builder = TypeBuilder.DefineEvent(@event.Name, @event.Attributes, @event.EventHandlerType).SetCustomAttributes(@event);

                if (@event.AddMethod != null)
                {
                    builder.SetAddOnMethod(BuildMethodBody(@event.AddMethod, @event));
                }

                if (@event.RaiseMethod != null)
                {
                    builder.SetRaiseMethod(BuildMethodBody(@event.RaiseMethod, @event));
                }

                if (@event.RemoveMethod != null)
                {
                    builder.SetRemoveOnMethod(BuildMethodBody(@event.RemoveMethod, @event));
                }
            }
        }

        #endregion

        #region Constructors

        protected virtual void BuildTypeConstrcutors()
        {
            BuildTypeStaticConstrcutor();
        }

        protected virtual void BuildTypeStaticConstrcutor()
        {
            var ilGen = TypeBuilder
                .DefineConstructor(MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, Type.EmptyTypes)
                .GetILGenerator();

            foreach (var item in Members.Where(i => i.IsEvent).Select(i => i.Member.AsEvent()))
            {
                ilGen.SetStaticField(GetField(item), _ => ilGen.LoadEventInfo(item));
            }

            foreach (var item in Members.Where(i => i.IsProperty).Select(i => i.Member.AsProperty()))
            {
                if (item.CanWrite)
                {
                    ilGen.SetStaticField(GetField(item.SetMethod), _ => ilGen.LoadMethodInfo(item.SetMethod));
                }

                if (item.CanRead)
                {
                    ilGen.SetStaticField(GetField(item.GetMethod), _ => ilGen.LoadMethodInfo(item.GetMethod));
                }

                ilGen.SetStaticField(GetField(item), _ => ilGen.LoadPropertyInfo(item));
            }

            foreach (var item in Members.Where(i => i.IsMethod).Select(i => i.Member.AsMethod()))
            {
                ilGen.SetStaticField(GetField(item), _ => ilGen.LoadMethodInfo(item));
            }

            ilGen.Return();
        }

        #endregion

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
