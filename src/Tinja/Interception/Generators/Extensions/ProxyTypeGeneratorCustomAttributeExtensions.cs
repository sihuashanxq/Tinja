using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Tinja.Interception.Generators
{
    public static class ProxyTypeGeneratorCustomAttributeExtensions
    {
        public static void CreateTypeCustomAttribute(this IProxyTypeGenerator _, TypeBuilder typeBuilder, Type target)
        {
            foreach (var customAttriute in target.CustomAttributes)
            {
                typeBuilder.SetCustomAttribute(CreateCustomAttribute(customAttriute));
            }
        }

        public static void CreateTypeMethodCustomAttributes(this IProxyTypeGenerator _, MethodBuilder methodBuilder, MethodInfo methodInfo)
        {
            foreach (var customAttriute in methodInfo.CustomAttributes)
            {
                methodBuilder.SetCustomAttribute(CreateCustomAttribute(customAttriute));
            }
        }

        public static void CreateTypePropertyCustomAttributes(this IProxyTypeGenerator _, PropertyBuilder propertyBuilder, PropertyInfo propertyInfo)
        {
            foreach (var customAttriute in propertyInfo.CustomAttributes)
            {
                propertyBuilder.SetCustomAttribute(CreateCustomAttribute(customAttriute));
            }
        }

        public static void CreateTypeConstructorCustomAttributes(this IProxyTypeGenerator _, ConstructorBuilder constructorBuilder, ConstructorInfo constructorInfo)
        {
            foreach (var customAttriute in constructorInfo.CustomAttributes)
            {
                constructorBuilder.SetCustomAttribute(CreateCustomAttribute(customAttriute));
            }
        }

        private static CustomAttributeBuilder CreateCustomAttribute(CustomAttributeData customAttribute)
        {
            if (customAttribute.NamedArguments == null)
            {
                return new CustomAttributeBuilder(customAttribute.Constructor, customAttribute.ConstructorArguments.Select(c => c.Value).ToArray());
            }

            var args = new object[customAttribute.ConstructorArguments.Count];
            for (var i = 0; i < args.Length; i++)
            {
                if (typeof(IEnumerable).IsAssignableFrom(customAttribute.ConstructorArguments[i].ArgumentType))
                {
                    args[i] = (customAttribute.ConstructorArguments[i].Value as IEnumerable<CustomAttributeTypedArgument>).Select(x => x.Value).ToArray();
                    continue;
                }

                args[i] = customAttribute.ConstructorArguments[i].Value;
            }

            var namedProperties = customAttribute
                .NamedArguments
                .Where(n => !n.IsField)
                .Select(n => customAttribute.AttributeType.GetProperty(n.MemberName))
                .ToArray();

            var properties = customAttribute
                .NamedArguments
                .Where(n => !n.IsField)
                .Select(n => n.TypedValue.Value)
                .ToArray();

            var namedFields = customAttribute
                .NamedArguments
                .Where(n => n.IsField)
                .Select(n => customAttribute.AttributeType.GetField(n.MemberName))
                .ToArray();

            var fields = customAttribute
                .NamedArguments
                .Where(n => n.IsField)
                .Select(n => n.TypedValue.Value)
                .ToArray();

            return new CustomAttributeBuilder(customAttribute.Constructor, args
               , namedProperties
               , properties, namedFields, fields);
        }
    }
}
