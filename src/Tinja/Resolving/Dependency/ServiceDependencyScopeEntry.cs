using System;
using Tinja.Resolving;

namespace Tinja.Resolving.Dependency.Scope
{
    public class ServiceDependencyScopeEntry
    {
        public Type ServiceType { get; set; }

        public ServiceDependencyChain Chain { get; set; }

        public IServiceResolvingContext Context { get; set; }

        public ServiceDependScopeType ScopeType { get; set; }
    }

    public enum ServiceDependScopeType
    {
        None,

        Parameter,

        Property
    }
}
