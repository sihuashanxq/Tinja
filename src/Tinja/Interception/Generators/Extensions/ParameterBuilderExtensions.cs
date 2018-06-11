using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Tinja.Interception.Generators.Extensions
{
    public static class ParameterBuilderExtensions
    {
        public static ParameterBuilder SetCustomAttributes(this ParameterBuilder builder, ParameterInfo parameterInfo)
        {
            if (builder == null || parameterInfo == null)
            {
                return builder;
            }

            foreach (var customAttriute in parameterInfo
                .CustomAttributes
                .Where(item => item.AttributeType != typeof(InjectAttribute) && item.AttributeType != typeof(InterceptorAttribute)))
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
