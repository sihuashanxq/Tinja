using System.Reflection;

namespace Tinja.Resolving.Descriptor
{
    public class ConstructorDescriptor
    {
        public ConstructorInfo ConstructorInfo { get; }

        public ParameterInfo[] Paramters { get; }

        public ConstructorDescriptor(ConstructorInfo constructor, ParameterInfo[] parameters)
        {
            ConstructorInfo = constructor;
            Paramters = parameters;
        }
    }
}
