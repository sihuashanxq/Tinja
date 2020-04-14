using System;

namespace Tinja.Core.Injection
{
    public class BindTagAttribute : Attribute
    {
        public string[] Tags { get; }

        public BindTagAttribute(params string[] tags)
        {
            Tags = tags ?? new string[0];
        }
    }
}
