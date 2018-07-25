using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.Injection.Extensions;
using Tinja.Core.Injection;

namespace Tinja.Core.DynamicProxy.Generators.Extensions
{
    public static class ParameterBuilderExtensions
    {
        public static ParameterBuilder SetCustomAttributes(this ParameterBuilder builder, ParameterInfo parameterInfo)
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
                .Where(item => !item.AttributeType.Is(typeof(InjectAttribute)) &&
                               !item.AttributeType.Is(typeof(InterceptorAttribute))))
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
