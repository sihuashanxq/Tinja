using System;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Interception.Generators.Utils;

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

            foreach (var customAttriute in parameterInfo.CustomAttributes)
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
