using System;
using System.Reflection;

namespace Tinja.Abstractions.Injection
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public abstract class ValueProviderAttribute : Attribute
    {
        public abstract object GetValue(IServiceResolver serviceResolver, ICustomAttributeProvider memberInfo);
    }
}
