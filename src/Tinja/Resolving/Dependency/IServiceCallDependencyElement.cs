using System;

namespace Tinja.Resolving.Dependency
{
    public interface IServiceCallDependencyElement
    {
        ServiceCallDependency Build(Type serviceType);
    }
}
