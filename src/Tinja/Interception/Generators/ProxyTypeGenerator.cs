using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using Tinja.Extension;
using Tinja.Interception.TypeMembers;
using Tinja.Resolving;

namespace Tinja.Interception
{
    public abstract class ProxyTypeGenerator : IProxyTypeGenerator
    {
        static MethodInfo MethodServiceResolve { get; }

        static MethodInfo MethodInvocationExecute { get; }

        protected Type BaseType { get; }

        protected Type ImplementionType { get; }

        protected TypeBuilder TypeBuilder { get; set; }

        protected IEnumerable<TypeMember> TypeMembers { get; }

        protected Dictionary<string, FieldBuilder> Fields { get; }

        static ProxyTypeGenerator()
        {
            MethodServiceResolve = typeof(IServiceResolver).GetMethod("Resolve");
            MethodInvocationExecute = typeof(IMethodInvocationExecutor).GetMethod("Execute");
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

        #region field

        protected virtual void CreateTypeFields()
        {
            CreateField("__executor", typeof(IMethodInvocationExecutor), FieldAttributes.Private);
            CreateField("__interceptors", typeof(IEnumerable<InterceptionTargetBinding>), FieldAttributes.Private);
        }

        public FieldBuilder GetField(string field)
        {
            return Fields.GetValueOrDefault(field);
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

        protected virtual void CreateTypeEvents()
        {

        }

        protected virtual void CreateTypeMethods()
        {
            foreach (var typeMember in TypeMembers.Where(m => m.IsMethod))
            {
                var methodInfo = typeMember.Member.AsMethod();

                var paramterInfos = methodInfo.GetParameters();
                var paramterTypes = paramterInfos.Select(i => i.ParameterType).ToArray();
                var methodBudiler = TypeBuilder.DefineMethod(
                    methodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    CallingConventions.HasThis,
                    methodInfo.ReturnType,
                    paramterTypes
                );

                var il = methodBudiler.GetILGenerator();

                //this.Resolver.Resolve(IMethodInvocationExecutor);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, ResolverField);
                il.Emit(OpCodes.Ldtoken, typeof(IMethodInvocationExecutor));
                il.Emit(OpCodes.Callvirt, MethodServiceResolve);

                //executor.Execute(new MethodInvocation)
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, ImplenmetionField);
                il.Emit(OpCodes.Ldsfld, MethodFields[meta]);

                //new Parameters[]
                il.Emit(OpCodes.Ldc_I4, paramterTypes.Length);
                il.Emit(OpCodes.Newarr, typeof(object));

                for (var i = 0; i < paramterTypes.Length; i++)
                {
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldarg, i + 1);
                    il.Box(paramterTypes[i]);
                    il.Emit(OpCodes.Stelem_Ref);
                }

                il.Emit(OpCodes.Newobj, MethodInvocation.Constrcutor);
                il.Emit(OpCodes.Callvirt, MethodInvocationExecute);
                il.Emit(methodInfo.IsVoidMethod() ? OpCodes.Pop : OpCodes.Nop);
                il.Emit(OpCodes.Ret);
            }
        }

        protected virtual void CreateTypeProperties()
        {

        }

        #region Constructors

        protected virtual void CreateTypeConstrcutors()
        {
            var ctors = GetBaseConstructorInfos();
            if (!ctors.Any())
            {
                CreateTypeDefaultConstructor();
            }
            else
            {
                foreach (var item in ctors)
                {
                    CreateTypeConstructor(item);
                }
            }
        }

        protected abstract void CreateTypeDefaultConstructor();

        protected abstract void CreateTypeConstructor(ConstructorInfo constrcutor);

        protected virtual ConstructorInfo[] GetBaseConstructorInfos()
        {
            return new ConstructorInfo[0];
        }

        #endregion
    }
}
