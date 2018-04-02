using System;
using System.Linq;
using System.Reflection;

namespace Tinja.Resolving.Descriptor
{
    public class TypeDescriptor
    {
        public Type Type { get; }

        public PropertyInfo[] Properties { get; }

        public ConstructorDescriptor[] Constructors { get; }

        public TypeDescriptor(Type type)
        {
            Type = type;
            Properties = new PropertyInfo[0];//type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
            Constructors = type
                .GetConstructors()
                .Select(i => new ConstructorDescriptor(i, i.GetParameters()))
                .ToArray();
        }
    }
}
