using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Tinja.Interception.Generators.Extensions
{
    public static class ConstructorBuilderExtensions
    {
        public static ConstructorBuilder DefineParameters(this ConstructorBuilder builder, ParameterInfo[] parameters, int paramterCount)
        {
            if (builder == null || parameters == null || parameters.Length == 0)
            {
                return builder;
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = builder.DefineParameter(i + 1, parameters[i].Attributes, parameters[i].Name);
                if (parameters[i].HasDefaultValue)
                {
                    parameter.SetConstant(parameters[i].DefaultValue);
                }

                parameter.SetCustomAttributes(parameters[i]);
            }

            for (var i = parameters.Length; i < paramterCount; i++)
            {
                builder.DefineParameter(i + 1, ParameterAttributes.None, "parameter" + (i + 1));
            }

            return builder;
        }

        public static ConstructorBuilder SetCustomAttributes(this ConstructorBuilder builder, ConstructorInfo constructorInfo)
        {
            if (builder == null || constructorInfo == null)
            {
                return builder;
            }

            foreach (var customAttriute in constructorInfo
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
