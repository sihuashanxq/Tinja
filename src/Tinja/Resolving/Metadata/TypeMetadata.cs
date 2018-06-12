using System;
using System.Linq;
using System.Reflection;

namespace Tinja.Resolving.Metadata
{
    public class TypeMetadata
    {
        public Type Type { get; }

        public TypeConstructor[] Constructors { get; }

        public TypeMetadata(Type type)
        {
            Type = type;

            Constructors = type
                .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(item => new TypeConstructor(item, item.GetParameters()))
                .ToArray();
        }
    }
}
