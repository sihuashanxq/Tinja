using System;
using System.Reflection.Emit;

namespace Tinja.Interception.Generators.Extensions
{
    public static class GenericTypeParameterBuilderExtensions
    {
        public static GenericTypeParameterBuilder SetGenericParameterConstraint(this GenericTypeParameterBuilder builder, Type genericArgument)
        {
            if (genericArgument == null || builder == null)
            {
                return builder;
            }

            foreach (var constraint in genericArgument.GetGenericParameterConstraints())
            {
                if (constraint.IsInterface)
                {
                    builder.SetInterfaceConstraints(constraint);
                }
                else
                {
                    builder.SetBaseTypeConstraint(constraint);
                }
            }

            builder.SetGenericParameterAttributes(genericArgument.GenericParameterAttributes);

            return builder;
        }
    }
}
