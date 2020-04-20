using System;

namespace Tinja.Core.Injection.Graphs
{
    /// <inheritdoc />
    public class GraphCircularException : Exception
    {
        public Type ServiceType { get; }

        public GraphSiteScope CallScope { get; }

        public GraphCircularException(Type serviceType, GraphSiteScope callScope, string message) : base(message)
        {
            CallScope = callScope;
            ServiceType = serviceType;
        }
    }
}
