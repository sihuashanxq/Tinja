using System;

namespace Tinja.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class InjectAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
