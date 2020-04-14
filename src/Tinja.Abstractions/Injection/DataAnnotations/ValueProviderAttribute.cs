using System;
using System.Reflection;

namespace Tinja.Abstractions.Injection.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public abstract class ValuerProviderAttribute : Attribute
    {
        public abstract object GetValue(IServiceResolver serviceResolver, ICustomAttributeProvider memberInfo);
    }
}
