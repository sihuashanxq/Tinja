using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

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

            foreach (var customAttriute in typeInfo
                .CustomAttributes
                .Where(item => item.AttributeType != typeof(InterceptorAttribute)))
            {
                var attributeBuilder = GeneratorUtility.CreateCustomAttribute(customAttriute);
                if (attributeBuilder != null)
                {
                    builder.SetCustomAttribute(attributeBuilder);
                }
            }

            return builder;
        }

        internal static MethodBuilder DefineMethod(this TypeBuilder typeBuilder, MethodInfo methodInfo)
        {
            return typeBuilder
                 .DefineMethod(
                    methodInfo.Name,
                    GetMethodAttributes(methodInfo),
                    CallingConventions.HasThis,
                    methodInfo.ReturnType,
                    methodInfo.GetParameters().Select(i => i.ParameterType).ToArray()
                 )
                 .DefineGenericParameters(methodInfo)
                 .DefineParameters(methodInfo)
                 .DefineReturnParameter(methodInfo)
                 .SetCustomAttributes(methodInfo);
        }

        private static MethodAttributes GetMethodAttributes(MethodInfo methodInfo)
        {
            var attributes = MethodAttributes.HideBySig | MethodAttributes.Virtual;
            if (methodInfo.IsPublic)
            {
                return MethodAttributes.Public | attributes;
            }

            if (methodInfo.IsFamily)
            {
                return MethodAttributes.Family | attributes;
            }

            if (methodInfo.IsFamilyAndAssembly)
            {
                return MethodAttributes.FamANDAssem | attributes;
            }

            if (methodInfo.IsFamilyOrAssembly)
            {
                return MethodAttributes.FamORAssem | attributes;
            }

            if (methodInfo.IsPrivate)
            {
                return MethodAttributes.Private | attributes;
            }

            return attributes;
        }
    }
}
