using System;
using Tinja.Resolving.Context;

namespace Tinja.Resolving.Dependency.Scope
{
    public class ServiceDependScopeEntry
    {
        public Type ResolveServiceType { get; set; }

        public ServiceDependChain Chain { get; set; }

        public IResolvingContext Context { get; set; }

        public ServiceDependScopeType ScopeType { get; set; }
    }

    public enum ServiceDependScopeType
    {
        None,

        Parameter,

        Property
    }
}
