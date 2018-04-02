using System.Collections.Generic;
using System.Reflection;
using Tinja.Resolving.Descriptor;
using Tinja.Resolving.ReslovingContext;

namespace Tinja.Resolving.Builder
{
    public interface IServiceNode
    {
        IResolvingContext ResolvingContext { get; set; }

        ConstructorDescriptor Constructor { get; set; }

        Dictionary<PropertyInfo, IServiceNode> Properties { get; set; }

        Dictionary<ParameterInfo, IServiceNode> Paramters { get; set; }
    }

    public class ServiceConstrutorNode : IServiceNode
    {
        public ConstructorDescriptor Constructor { get; set; }

        public IResolvingContext ResolvingContext { get; set; }

        public Dictionary<PropertyInfo, IServiceNode> Properties { get; set; }

        public Dictionary<ParameterInfo, IServiceNode> Paramters { get; set; }
    }

    public class ServiceEnumerableNode : ServiceConstrutorNode
    {
        public IServiceNode[] Elements { get; set; }
    }
}
