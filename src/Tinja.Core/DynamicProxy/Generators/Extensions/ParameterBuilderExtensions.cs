using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Abstractions.DynamicProxy.Registrations;
using Tinja.Abstractions.Extensions;
using Tinja.Core.Injection;

namespace Tinja.Core.DynamicProxy.Generators.Extensions
{
    internal static class ParameterBuilderExtensions
    {
        internal static ParameterBuilder SetCustomAttributes(this ParameterBuilder builder, ParameterInfo parameterInfo)
        {
            if (builder == null)
            {
                throw new NullReferenceException(nameof(builder));
            }

            if (parameterInfo == null)
            {
                throw new NullReferenceException(nameof(parameterInfo));
            }

            foreach (var customAttriute in parameterInfo
                .CustomAttributes
                .Where(item => item.AttributeType.IsNotType<InjectAttribute>() && item.AttributeType.IsNotType<InterceptorAttribute>()))
            {
                var attrBuilder = GeneratorUtils.CreateCustomAttribute(customAttriute);
                if (attrBuilder != null)
                {
                    builder.SetCustomAttribute(attrBuilder);
                }
            }

            return builder;
        }
    }
}
