using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Abstractions.Injection.Extensions;

namespace Tinja.Core.DynamicProxy.Generators.Extensions
{
    public static class PropertyInfoBuilderExtensions
    {
        public static PropertyBuilder SetCustomAttributes(this PropertyBuilder builder, PropertyInfo propertyInfo)
        {
            if (builder == null)
            {
                throw new NullReferenceException(nameof(builder));
            }

            if (propertyInfo == null)
            {
                throw new NullReferenceException(nameof(propertyInfo));
            }

            foreach (var customAttriute in propertyInfo
                .CustomAttributes
                .Where(item => !item.AttributeType.Is(typeof(InjectAttribute)) &&
                               !item.AttributeType.Is(typeof(InterceptorAttribute))))
            {
                var attributeBuilder = GeneratorUtility.CreateCustomAttribute(customAttriute);
                if (attributeBuilder != null)
                {
                    builder.SetCustomAttribute(attributeBuilder);
                }
            }

            return builder;
        }
    }
}
