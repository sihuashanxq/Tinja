using System.Collections.Generic;
using System.Reflection;
using Tinja.Resolving.Context;

namespace Tinja.Resolving.Dependency
{
    public class ServiceDependChain
    {
        public IResolvingContext Context { get; set; }

        public ServiceConstructorInfo Constructor { get; set; }

        public Dictionary<PropertyInfo, ServiceDependChain> Properties { get; set; }

        public Dictionary<ParameterInfo, ServiceDependChain> Parameters { get; set; }

        public ServiceDependChain()
        {
            Properties = new Dictionary<PropertyInfo, ServiceDependChain>();
            Parameters = new Dictionary<ParameterInfo, ServiceDependChain>();
        }
    }
}
