using System.Reflection;

namespace Tinja.Resolving
{
    public class TypeConstructorMetadata
    {
        public ConstructorInfo ConstructorInfo { get; }

        public ParameterInfo[] Paramters { get; }

        public TypeConstructorMetadata(ConstructorInfo constructor, ParameterInfo[] parameters)
        {
            ConstructorInfo = constructor;
            Paramters = parameters;
        }
    }
}
