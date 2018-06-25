using System;

namespace Tinja.Resolving.Dependency
{
    public interface IServiceCallDependencyBuilder
    {
        ServiceCallDependency Build(Type serviceType);
    }
}
