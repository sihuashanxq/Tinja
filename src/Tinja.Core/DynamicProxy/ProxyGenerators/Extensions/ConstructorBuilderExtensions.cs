using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.Injection.Extensions;
using Tinja.Core.Injection;

namespace Tinja.Core.DynamicProxy.ProxyGenerators.Extensions
{
    public static class ConstructorBuilderExtensions
    {
        public static ConstructorBuilder DefineParameters(this ConstructorBuilder builder, ParameterInfo[] parameterInfos, int paramterCount)
        {
            if (builder == null)
            {
                throw new NullReferenceException(nameof(builder));
            }

            if (parameterInfos == null)
            {
                throw new NullReferenceException(nameof(parameterInfos));
            }

            if (parameterInfos.Length == 0)
            {
                return builder;
            }

            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var parameter = builder.DefineParameter(i + 1, parameterInfos[i].Attributes, parameterInfos[i].Name);
                if (parameterInfos[i].HasDefaultValue)
                {
                    parameter.SetConstant(parameterInfos[i].DefaultValue);
                }

                parameter.SetCustomAttributes(parameterInfos[i]);
            }

            for (var i = parameterInfos.Length; i < paramterCount; i++)
            {
                builder.DefineParameter(i + 1, ParameterAttributes.None, "parameter" + (i + 1));
            }

            return builder;
        }

        public static ConstructorBuilder SetCustomAttributes(this ConstructorBuilder builder, ConstructorInfo constructorInfo)
        {
            if (builder == null)
            {
                throw new NullReferenceException(nameof(builder));
            }

            if (constructorInfo == null)
            {
                throw new NullReferenceException(nameof(constructorInfo));
            }

            foreach (var customAttriute in constructorInfo
                .CustomAttributes
                .Where(item => !item.AttributeType.Is(typeof(InjectAttribute)) &&
                               !item.AttributeType.Is(typeof(InterceptorAttribute))))
            {
                var attrBuilder = GeneratorUtility.CreateCustomAttribute(customAttriute);
                if (attrBuilder != null)
                {
                    builder.SetCustomAttribute(attrBuilder);
                }
            }

            return builder;
        }
    }
}
