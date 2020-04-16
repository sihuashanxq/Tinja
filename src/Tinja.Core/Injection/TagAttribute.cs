using System;

namespace Tinja.Core.Injection
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class TagAttribute : Attribute
    {
        public string Value { get; set; }

        public bool Optional { get; set; }

        public TagAttribute(string value)
        {
            Value = value;
        }
    }
}
