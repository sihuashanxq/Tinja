using System;
using Tinja.Resolving.Dependency;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Activation.Builder
{
    public interface IActivatorBuilder
    {
        Func<IServiceResolver, IServiceLifeScope, object> Build(ServiceCallDependency callDependency);
    }
}
