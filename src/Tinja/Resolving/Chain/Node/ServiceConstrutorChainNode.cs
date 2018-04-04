using System.Collections.Generic;
using System.Reflection;
using Tinja.Resolving.Context;

namespace Tinja.Resolving.Chain.Node
{
    public class ServiceConstrutorChainNode : IServiceChainNode
    {
        public ServiceConstructorInfo Constructor { get; set; }

        public IResolvingContext ResolvingContext { get; set; }

        public Dictionary<PropertyInfo, IServiceChainNode> Properties { get; set; }

        public Dictionary<ParameterInfo, IServiceChainNode> Paramters { get; set; }
    }
}
