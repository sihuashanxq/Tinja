using System;
using System.Reflection.Emit;

namespace Tinja.Core.DynamicProxy.Generators.Extensions
{
    internal static class GenericTypeParameterBuilderExtensions
    {
        internal static GenericTypeParameterBuilder SetGenericParameterConstraint(this GenericTypeParameterBuilder builder, Type genericArgument)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (genericArgument == null)
            {
                throw new ArgumentNullException(nameof(genericArgument));
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
