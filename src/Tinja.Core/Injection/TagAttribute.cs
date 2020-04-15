using System;

namespace Tinja.Core.Injection
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class TagAttribute : Attribute
    {
        public string Value { get; }

        public TagAttribute(string value)
        {
            Value = value;
        }
    }
}
