using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Tinja.Interception.Generators.Utils;

namespace Tinja.Interception.Generators.Extensions
{
    public static class TypeBuilderExtensions
    {
        public static TypeBuilder DefineGenericParameters(this TypeBuilder builder, Type typeInfo)
        {
            if (typeInfo == null || builder == null || !typeInfo.IsGenericType)
            {
                return builder;
            }

            var genericArguments = typeInfo.GetGenericArguments();
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

        public static TypeBuilder SetCustomAttributes(this TypeBuilder builder, Type typeInfo)
        {
            if (builder == null || typeInfo == null)
            {
                return builder;
            }

            foreach (var customAttriute in typeInfo.CustomAttributes)
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
