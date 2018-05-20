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
        static MethodInfo MethodServiceResolve { get; }

        static MethodInfo MethodInvocationExecute { get; }

        protected Type BaseType { get; }

        protected Type ImplementionType { get; }

        protected TypeBuilder TypeBuilder { get; set; }

        protected IEnumerable<TypeMember> TypeMembers { get; }

        private Dictionary<string, FieldBuilder> _nameFields;

        private Dictionary<MethodInfo, FieldBuilder> _methodFields;

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

            _nameFields = new Dictionary<string, FieldBuilder>();
            _methodFields = new Dictionary<MethodInfo, FieldBuilder>();
        }

        public virtual Type CreateProxyType()
        {
            CreateField("__target", ImplementionType, FieldAttributes.Private);
            CreateField("__resolver", typeof(IServiceResolver), FieldAttributes.Private);
            CreateField("__interceptors", typeof(IEnumerable<InterceptionTargetBinding>), FieldAttributes.Private);

            CreateTypeConstrcutors();

            CreateTypeMethods();

            return TypeBuilder.CreateType();
        }

        #region field

        public FieldBuilder GetField(string name)
        {
            return _nameFields.GetValueOrDefault(name);
        }

        public FieldBuilder GetField(MethodInfo methodInfo)
        {
            return _methodFields.GetValueOrDefault(methodInfo);
        }

        public FieldBuilder CreateField(string name, Type fieldType, FieldAttributes attributes)
        {
            if (!_nameFields.ContainsKey(name))
            {
                return _nameFields[name] = TypeBuilder.DefineField(name, fieldType, attributes);
            }

            return _nameFields[name];
        }

        public FieldBuilder CreateField(MethodInfo method, FieldAttributes attributes)
        {
            if (!_methodFields.ContainsKey(method))
            {
                return _methodFields[method] = TypeBuilder.DefineField("__method__" + method.Name + _methodFields.Count, typeof(MethodInfo), attributes);
            }

            return _methodFields[method];
        }
        #endregion

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

        protected virtual void CreateTypeConstrcutors()
        {
            CreateStaticConstrcutor();

            foreach (var item in ImplementionType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                CreateConstructor(item);
            }
        }

        protected virtual void CreateStaticConstrcutor()
        {
            var il = TypeBuilder
                .DefineConstructor(MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, Type.EmptyTypes)
                .GetILGenerator();

            foreach (var item in TypeMembers.Where(u => u.IsMethod))
            {
                var methodInfo = item.Member.AsMethod();
                var field = CreateField(methodInfo, FieldAttributes.Private | FieldAttributes.Static);

                StoreMethodToField(il, methodInfo, field);
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
                        il.Emit(OpCodes.Stfld, GetField("__target"));
                        break;
                    case 1:
                        il.Emit(OpCodes.Stfld, GetField("__resolver"));
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

        protected virtual Type[] GetExtraConstrcutorParameters()
        {
            return new[] { ImplementionType, typeof(IServiceResolver) };
        }

        private static void StoreMethodToField(ILGenerator il, MethodInfo methodInfo, FieldBuilder field)
        {
            var getMethod = typeof(Type).GetMethod("GetMethod", new[] { typeof(string), typeof(BindingFlags), typeof(Binder), typeof(Type[]), typeof(ParameterModifier[]) });
            var parameterTypes = methodInfo.GetParameters().Select(info => info.ParameterType).ToArray();
            var GetTypeFromRuntimeHandleMethod = typeof(Type).GetMethod("GetTypeFromHandle");

            il.Emit(OpCodes.Ldtoken, methodInfo.DeclaringType);

            il.Emit(OpCodes.Ldstr, methodInfo.Name);
            il.Emit(OpCodes.Ldc_I4, (int)(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ldc_I4, parameterTypes.Length);
            il.Emit(OpCodes.Newarr, typeof(Type));

            for (var i = 0; i < parameterTypes.Length; i++)
            {
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldtoken, parameterTypes[i]);
                il.Emit(OpCodes.Stelem, typeof(Type));
            }

            il.Emit(OpCodes.Ldnull);
            il.EmitCall(OpCodes.Call, getMethod, null);
            il.Emit(OpCodes.Stsfld, field);
        }
    }
}
