using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.Extensions;
using Tinja.Core.Injection;

namespace Tinja.Core.DynamicProxy.Generators.Extensions
{
    public static class MethodBuilderExtensions
    {
        public static MethodBuilder DefineGenericParameters(this MethodBuilder builder, MethodInfo methodInfo)
        {
            if (builder == null)
            {
                throw new NullReferenceException(nameof(builder));
            }

            if (methodInfo == null)
            {
                throw new NullReferenceException(nameof(methodInfo));
            }

            if (!methodInfo.IsGenericMethod)
            {
                return builder;
            }

            var genericArguments = methodInfo.GetGenericArguments();
            if (genericArguments.Length == 0)
            {
                return builder;
            }

            var genericArgumentBuilders = builder.DefineGenericParameters(genericArguments.Select(i => i.Name).ToArray());
            if (genericArgumentBuilders.Length == 0)
            {
                return builder;
            }

            for (var i = 0; i < genericArgumentBuilders.Length; i++)
            {
                genericArgumentBuilders[i].SetGenericParameterConstraint(genericArguments[i]);
            }

            return builder;
        }

        public static MethodBuilder DefineParameters(this MethodBuilder builder, MethodInfo methodInfo)
        {
            if (builder == null)
            {
                throw new NullReferenceException(nameof(builder));
            }

            if (methodInfo == null)
            {
                throw new NullReferenceException(nameof(methodInfo));
            }

            var parameterInfos = methodInfo.GetParameters();
            if (parameterInfos.Length == 0)
            {
                return builder;
            }

            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var parameterBuilder = builder.DefineParameter(i + 1, parameterInfos[i].Attributes, parameterInfos[i].Name);
                if (parameterInfos[i].HasDefaultValue)
                {
                    parameterBuilder.SetConstant(parameterInfos[i].DefaultValue);
                }

                parameterBuilder.SetCustomAttributes(parameterInfos[i]);
            }

            return builder;
        }

        public static MethodBuilder DefineReturnParameter(this MethodBuilder builder, MethodInfo methodInfo)
        {
            if (builder == null)
            {
                throw new NullReferenceException(nameof(builder));
            }

            if (methodInfo == null)
            {
                throw new NullReferenceException(nameof(methodInfo));
            }

            builder
                .DefineParameter(0, methodInfo.ReturnParameter.Attributes, null)
                .SetCustomAttributes(methodInfo.ReturnParameter);

            return builder;
        }

        public static MethodBuilder SetCustomAttributes(this MethodBuilder builder, MethodInfo methodInfo)
        {
            if (builder == null)
            {
                throw new NullReferenceException(nameof(builder));
            }

            if (methodInfo == null)
            {
                throw new NullReferenceException(nameof(methodInfo));
            }

            foreach (var customAttriute in methodInfo
                .CustomAttributes
                .Where(item => !item.AttributeType.IsType(typeof(InjectAttribute)) &&
                               !item.AttributeType.IsType(typeof(InterceptorAttribute))))
            {
                var attrBuilder = GeneratorUtils.CreateCustomAttribute(customAttriute);
                if (attrBuilder != null)
                {
                    builder.SetCustomAttribute(attrBuilder);
                }
            }

            return builder;
        }

        public static MethodBuilder MakeDefaultMethodBody(this MethodBuilder builder, MethodInfo methodInfo)
        {
            if (builder == null)
            {
                throw new NullReferenceException(nameof(builder));
            }

            if (methodInfo == null)
            {
                throw new NullReferenceException(nameof(methodInfo));
            }

            var ilGen = builder.GetILGenerator();

            if (methodInfo.ReturnType.IsVoid())
            {
                ilGen.Return();
                return builder;
            }

            ilGen.LoadDefaultValue(builder.ReturnType);
            ilGen.Return();

            return builder;
        }
    }
}
