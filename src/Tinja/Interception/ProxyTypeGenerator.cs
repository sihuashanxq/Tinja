using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Tinja.Extension;

namespace Tinja.Interception
{
    public class ProxyTypeGenerator : IProxyTypeGenerator
    {
        protected Type BaseType { get; }

        protected Type ImplementionType { get; }

        protected int PostfixCounter { get; set; }

        protected TypeBuilder TypeBuilder { get; set; }

        protected FieldBuilder ImplenmetionField { get; set; }

        protected List<MethodInfo> OverrideMethodInfos { get; }

        protected Dictionary<Type, FieldBuilder> InterecetorFields { get; }

        protected Dictionary<MethodInfo, FieldBuilder> MethodFields { get; }

        protected Dictionary<MethodInfo, List<Type>> MethodInterecptors { get; }

        static MethodInfo MethodInvocationExecute { get; }

        static MethodInfo MethodInvocationExecuteAsync { get; }

        static ProxyTypeGenerator()
        {
            MethodInvocationExecute = typeof(IMethodInvocationExecutor).GetMethod("Execute");
            MethodInvocationExecuteAsync = typeof(IMethodInvocationExecutor).GetMethod("ExecuteAsync");
        }

        public ProxyTypeGenerator(Type baseType, Type implemetionType)
        {
            BaseType = baseType;
            ImplementionType = implemetionType;

            OverrideMethodInfos = TypeGeneratorUtil.GetOverrideableMethods(baseType).ToList();

            MethodFields = new Dictionary<MethodInfo, FieldBuilder>();
            InterecetorFields = new Dictionary<Type, FieldBuilder>();
            MethodInterecptors = new Dictionary<MethodInfo, List<Type>>();
        }

        public virtual Type CreateProxyType()
        {
            InitializeMethodIntereceptors();

            CreateTypeBuilder();

            CreateTypeFields();

            CreateTypeConstrcutors();

            CreateTypeMethods();

            return TypeBuilder.CreateType();
        }

        protected virtual void InitializeMethodIntereceptors()
        {
            var intereceptors = BaseType.GetCustomAttributes<InterceptorAttribute>();

            foreach (var overrideMethodInfo in OverrideMethodInfos)
            {
                MethodInterecptors[overrideMethodInfo] = overrideMethodInfo
                    .GetCustomAttributes<InterceptorAttribute>()
                    .Concat(intereceptors)
                    .Select(i => i.InterceptorType)
                    .Distinct()
                    .ToList();
            }
        }

        protected virtual void CreateTypeBuilder()
        {
            TypeBuilder = TypeGeneratorUtil.DefineType(ImplementionType, BaseType);
        }

        protected FieldBuilder CreateField(Type fieldType, FieldAttributes fieldAttributes = FieldAttributes.Private)
        {
            return TypeBuilder.DefineField(GetMemberPostfix(), fieldType, fieldAttributes);
        }

        protected virtual void CreateTypeFields()
        {
            ImplenmetionField = CreateField(ImplementionType);

            foreach (var item in MethodInterecptors.SelectMany(i => i.Value).Distinct())
            {
                InterecetorFields[item] = CreateField(item);
            }
        }

        protected virtual void CreateTypeMethods()
        {
            foreach (var overrideMethodInfo in OverrideMethodInfos)
            {
                var paramterInfos = overrideMethodInfo.GetParameters();
                var paramterTypes = paramterInfos.Select(i => i.ParameterType).ToArray();
                var methodBudiler = TypeBuilder.DefineMethod(
                    overrideMethodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    CallingConventions.HasThis,
                    overrideMethodInfo.ReturnType,
                    paramterTypes
                );

                var methodIntereceptors = MethodInterecptors[overrideMethodInfo];
                var il = methodBudiler.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, ImplenmetionField);
                il.Emit(OpCodes.Ldsfld, MethodFields[overrideMethodInfo]);
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

                il.Emit(OpCodes.Ldc_I4, methodIntereceptors.Count);
                il.Emit(OpCodes.Newarr, typeof(IIntereceptor));

                for (var i = 0; i < methodIntereceptors.Count; i++)
                {
                    var item = methodIntereceptors[i];

                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, InterecetorFields[item]);
                    il.Emit(OpCodes.Stelem_Ref);
                }

                il.Emit(OpCodes.Newobj, MethodInvocation.Constrcutor);
                il.Emit(OpCodes.Call, typeof(ProxyTypeGenerator).GetMethod(nameof(Invoke)));
                il.Emit(OpCodes.Ret);
            }
        }

        protected virtual void CreateTypeConstrcutors()
        {
            CreateTypeStaticConstrcutor();

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

        protected virtual void CreateTypeStaticConstrcutor()
        {
            if (OverrideMethodInfos.Any())
            {
                var ctor = TypeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, Type.EmptyTypes);
                var il = ctor.GetILGenerator();

                foreach (var item in OverrideMethodInfos)
                {
                    MethodFields[item] = CreateField(typeof(MethodInfo), FieldAttributes.Private | FieldAttributes.Static);
                    TypeGeneratorUtil.AssignFieldWithMethodInfo(il, MethodFields[item], item);
                }

                il.Emit(OpCodes.Ret);
            }
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
                var item = extraConstructorParameters[i];

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg, i + 1);
                il.Emit(OpCodes.Stfld, i == 0 ? ImplenmetionField : InterecetorFields[item]);
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
            return new[] { ImplementionType }.Concat(InterecetorFields.Keys).ToArray();
        }
    }

    public class MethodInvocation
    {
        public static ConstructorInfo Constrcutor { get; }

        static MethodInvocation()
        {
            Constrcutor = typeof(MethodInvocation)
            .GetConstructor(new[]
            {
                typeof(object),
                typeof(MethodInfo),
                typeof(object[]), typeof(IIntereceptor[])
            });
        }

        public object Target { get; }

        public MethodInfo Method { get; }

        public object[] ParameterValues { get; }

        internal IIntereceptor[] Intereceptors { get; }

        public object ReturnValue { get; internal set; }

        public MethodInvocation(
            object target,
            MethodInfo method,
            object[] parameterValues,
            IIntereceptor[] intereceptors
        )
        {
            Target = target;
            Method = method;
            ParameterValues = parameterValues;
            Intereceptors = intereceptors;
        }
    }

    public interface IMethodInvokerFactory
    {
        Func<object[], object> Create(MethodInvocation invocation);
    }

    public class MethodInvokerFactory
    {
        public Func<object[], object> Create(MethodInvocation context)
        {
            return null;
        }
    }

    public interface IMethodInvocationExecutor
    {
        object Execute(MethodInvocation invocation);

        object ExecuteAsync(MethodInvocation invocation);
    }

    public class MethodInvocationExecutor : IMethodInvocationExecutor
    {
        public object Execute(MethodInvocation invocation)
        {
            throw new NotImplementedException();
        }

        public object ExecuteAsync(MethodInvocation invocation)
        {
            throw new NotImplementedException();
        }
    }
}
