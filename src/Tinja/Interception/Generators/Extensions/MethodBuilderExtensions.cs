using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Interception.Generators.Utils;

namespace Tinja.Interception.Generators.Extensions
{
    public static class MethodBuilderExtensions
    {
        public static MethodBuilder DefineGenericParameters(this MethodBuilder builder, MethodInfo methodInfo)
        {
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
            if (builder == null || methodInfo == null)
            {
                return builder;
            }

            var parameters = methodInfo.GetParameters();
            if (parameters.Length == 0)
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

            return builder;
        }

        public static MethodBuilder DefineReturnParameter(this MethodBuilder builder, MethodInfo methodInfo)
        {
            if (builder == null || methodInfo == null)
            {
                return builder;
            }

            builder
                .DefineParameter(0, methodInfo.ReturnParameter.Attributes, null)
                .SetCustomAttributes(methodInfo.ReturnParameter);

            return builder;
        }

        public static MethodBuilder SetCustomAttributes(this MethodBuilder builder, MethodInfo methodInfo)
        {
            if (builder == null || methodInfo == null)
            {
                return builder;
            }

            foreach (var customAttriute in methodInfo.CustomAttributes)
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
