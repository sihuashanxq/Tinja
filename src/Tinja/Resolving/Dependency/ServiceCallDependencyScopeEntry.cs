using System;
using Tinja.Resolving.Context;

namespace Tinja.Resolving.Dependency
{
    public class ServiceCallDependencyScopeEntry
    {
        public Type ServiceType { get; set; }

        public ServiceContext Context { get; set; }

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
