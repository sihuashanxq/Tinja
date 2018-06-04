using System;

namespace Tinja.Resolving.Dependency.Scope
{
    public class ServiceCallDependencyScopeEntry
    {
        public Type ServiceType { get; set; }

        public IServiceContext Context { get; set; }

        public ServiceCallDependencyScopeType ScopeType { get; set; }

        public ServiceCallDependency CallDependency { get; set; }
    }

    public enum ServiceCallDependencyScopeType
    {
        None,

        Parameter,

        Property
    }
}
