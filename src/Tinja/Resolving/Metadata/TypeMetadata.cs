using System;
using System.Linq;
using System.Reflection;

namespace Tinja.Resolving
{
    public class TypeMetadata
    {
        public Type Type { get; }

        public PropertyInfo[] Properties { get; }

        public TypeConstructorMetadata[] Constructors { get; }

        public TypeMetadata(Type type)
        {
            Type = type;

            Properties = type
                .GetProperties()
                .Where(i => i.CanRead && i.IsDefined(typeof(InjectAttribute)))
                .ToArray();

            Constructors = type
                .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(i => new TypeConstructorMetadata(i, i.GetParameters()))
                .ToArray();
        }
    }
}
