using System.Collections.Generic;
using System.Reflection;
using Tinja.Resolving.Context;

namespace Tinja.Resolving.Chain.Node
{
    public interface IServiceChainNode
    {
        IResolvingContext ResolvingContext { get; set; }

        ServiceConstructorInfo Constructor { get; set; }

        Dictionary<PropertyInfo, IServiceChainNode> Properties { get; set; }

        Dictionary<ParameterInfo, IServiceChainNode> Paramters { get; set; }
    }

}
