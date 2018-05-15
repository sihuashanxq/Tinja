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
    public class ProxyTypeGenerator : IProxyTypeGenerator
    {
        protected Type BaseType { get; }

        protected Type ImplementionType { get; }

        protected int PostfixCounter { get; set; }

        protected TypeBuilder TypeBuilder { get; set; }

        protected TypeFieldEmitter TypeFieldEmitter { get; set; }

        protected FieldBuilder ImplenmetionField { get; set; }

        protected FieldBuilder ResolverField { get; set; }

        protected IEnumerable<TypeMemberMetadata> ProxyMembers { get; }

        protected Dictionary<MemberInfo, FieldBuilder> MethodFields { get; }

        static MethodInfo MethodInvocationExecute { get; }

        static MethodInfo MethodServiceResolve { get; }

        static ProxyTypeGenerator()
        {
            MethodServiceResolve = typeof(IServiceResolver).GetMethod("Resolve");
            MethodInvocationExecute = typeof(IMethodInvocationExecutor).GetMethod("Execute");
        }

        public ProxyTypeGenerator(Type baseType, Type implemetionType)
        {
            BaseType = baseType;
            ImplementionType = implemetionType;

            //if (BaseType.IsInterface)
            //{
            //    ProxyMembers = new InterfaceTypeMemberCollector(BaseType, implemetionType).Collect();
            //}
            //else
            //{
            //    ProxyMembers = new ClassTypeMemberCollector(BaseType, implemetionType).Collect();
            //}

            TypeBuilder = TypeGeneratorUtil.DefineType(ImplementionType, BaseType);
            TypeFieldEmitter = new TypeFieldEmitter(TypeBuilder);
        }

        public virtual Type CreateProxyType()
        {
            TypeFieldEmitter.CreateField("__target", ImplementionType, FieldAttributes.Private);
            TypeFieldEmitter.CreateField("__resolver", typeof(IServiceResolver), FieldAttributes.Private);

            CreateTypeConstrcutors();

            CreateTypeMethods();

            return TypeBuilder.CreateType();
        }

        protected FieldBuilder CreateField(Type fieldType, FieldAttributes fieldAttributes = FieldAttributes.Private)
        {
            return TypeBuilder.DefineField(GetMemberPostfix(), fieldType, fieldAttributes);
        }

        protected virtual void CreateTypeMethods()
        {
            foreach (var meta in ProxyMembers)
            {
                var paramterInfos = meta.GetParameters();
                var paramterTypes = paramterInfos.Select(i => i.ParameterType).ToArray();
                var methodBudiler = TypeBuilder.DefineMethod(
                    meta.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    CallingConventions.HasThis,
                    meta.ReturnType,
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
                il.Emit(meta.IsVoidMethod() ? OpCodes.Pop : OpCodes.Nop);
                il.Emit(OpCodes.Ret);
            }
        }

        protected virtual void CreateTypeConstrcutors()
        {
            CreateStaticConstrcutor();

            var constructorInfos = BaseType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (constructorInfos == null || constructorInfos.Length == 0)
            {
                CreateConstructor(null);
                return;
            }

            foreach (var item in constructorInfos)
            {
                CreateConstructor(item);
            }
        }

        protected virtual void CreateStaticConstrcutor()
        {
            var ctor = TypeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, Type.EmptyTypes);
            var il = ctor.GetILGenerator();

            foreach (var item in ProxyMembers.Where(u => u.IsMethod))
            {
                MethodFields[item.ImplementionMember] = CreateField(typeof(MethodInfo), FieldAttributes.Private | FieldAttributes.Static);
                TypeGeneratorUtil.AssignFieldWithMethodInfo(il, MethodFields[item], item);
            }

            il.Emit(OpCodes.Ret);
        }

        protected virtual void CreateConstructor(ConstructorInfo baseConstrcutor)
        {
            var extraConstructorParameters = GetExtraConstrcutorParameters();
            var constructorParameters = extraConstructorParameters
                .Concat(baseConstrcutor?.GetParameters().Select(i => i.ParameterType) ?? Type.EmptyTypes)
                .ToArray();

            var constructorBuilder = TypeBuilder.DefineConstructor(
                baseConstrcutor?.Attributes ?? MethodAttributes.Public,
                baseConstrcutor?.CallingConvention ?? CallingConventions.HasThis,
                constructorParameters
            );

            var il = constructorBuilder.GetILGenerator();

            for (var i = 0; i < extraConstructorParameters.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg, i + 1);

                switch (i)
                {
                    case 0:
                        il.Emit(OpCodes.Stfld, ImplenmetionField);
                        break;
                    case 1:
                        il.Emit(OpCodes.Stfld, ResolverField);
                        break;
                }
            }

            if (baseConstrcutor == null)
            {
                il.Emit(OpCodes.Ret);
                return;
            }

            //call base
            il.Emit(OpCodes.Ldarg_0);

            for (var i = extraConstructorParameters.Length; i < constructorParameters.Length; i++)
            {
                il.Emit(OpCodes.Ldarg, i + 1);
            }

            il.Emit(OpCodes.Call, baseConstrcutor);
            il.Emit(OpCodes.Ret);
        }

        protected string GetMemberPostfix()
        {
            return "_Member_" + PostfixCounter++;
        }

        protected virtual Type[] GetExtraConstrcutorParameters()
        {
            return new[] { ImplementionType, typeof(IServiceResolver) };
        }
    }

    public class TypeFieldEmitter
    {
        private TypeBuilder _typeBuilder;

        private Dictionary<string, FieldBuilder> _fields;

        public TypeFieldEmitter(TypeBuilder typeBuilder)
        {
            _typeBuilder = typeBuilder;
            _fields = new Dictionary<string, FieldBuilder>();
        }

        public FieldBuilder GetField(string name)
        {
            return _fields.GetValueOrDefault(name);
        }

        public void CreateField(string name, Type fieldType, FieldAttributes attributes)
        {
            if (_fields.ContainsKey(name))
            {
                throw new NotSupportedException();
            }

            _fields[name] = _typeBuilder.DefineField(name, fieldType, attributes);
        }
    }
}
