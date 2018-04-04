using System.Reflection;

namespace Tinja.Resolving
{
    public class ServiceConstructorInfo
    {
        public ConstructorInfo ConstructorInfo { get; }

        public ParameterInfo[] Paramters { get; }

        public ServiceConstructorInfo(ConstructorInfo constructor, ParameterInfo[] parameters)
        {
            ConstructorInfo = constructor;
            Paramters = parameters;
        }
    }
}
