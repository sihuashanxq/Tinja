using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Tinja.Interception
{
    public abstract class ProxyGeneratorBase : IProxyGenerator
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

        public ProxyGeneratorBase(Type baseType, Type implemetionType)
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

            CreateType();

            CreateFields();

            CreateStaticConstrcutor();
            CreateConstrcutors();

            CreateMethods();
            CreateProperties();

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

        protected virtual void CreateType()
        {
            TypeBuilder = TypeGeneratorUtil.DefineType(ImplementionType, BaseType);
        }

        protected FieldBuilder CreateField(Type fieldType, FieldAttributes fieldAttributes = FieldAttributes.Private)
        {
            return TypeBuilder.DefineField(GetMemberPostfix(), fieldType, fieldAttributes);
        }

        protected virtual void CreateFields()
        {
            ImplenmetionField = CreateField(ImplementionType);

            foreach (var item in MethodInterecptors.SelectMany(i => i.Value).Distinct())
            {
                InterecetorFields[item] = CreateField(item);
            }
        }

        protected virtual void CreateMethods()
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

                for (var i = 0; i < paramterInfos.Length; i++)
                {
                    var item = paramterInfos[i];

                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldarg, i + 1);

                    if (item.ParameterType.IsValueType)
                    {
                        il.Emit(OpCodes.Box, item.ParameterType);
                    }

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

                il.Emit(OpCodes.Newobj, MethodInvocation.MethodInvocationConstrcutor);
                il.Emit(OpCodes.Call, typeof(ProxyGeneratorBase).GetMethod(nameof(Invoke)));
                il.Emit(OpCodes.Ret);
            }
        }

        protected abstract void CreateProperties();

        protected abstract void CreateConstrcutors();

        protected virtual void CreateStaticConstrcutor()
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

        public static object Invoke(MethodInvocation invocation)
        {
            foreach (var item in invocation.Intereceptors)
            {
                item.IntereceptAsync()
            }

            return null;
        }
    }

    public class MethodInvocation
    {
        public static readonly ConstructorInfo MethodInvocationConstrcutor = typeof(MethodInvocation)
            .GetConstructor(new[] { typeof(object), typeof(MethodInfo), typeof(object[]), typeof(IIntereceptor[]) });

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
}
