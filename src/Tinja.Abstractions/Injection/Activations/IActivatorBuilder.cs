using System;
using Tinja.Abstractions.Injection.Dependencies.Elements;

namespace Tinja.Abstractions.Injection.Activations
{
    public interface IActivatorBuilder
    {
        Func<IServiceResolver, IServiceLifeScope, object> Build(CallDependElement element);
    }
}
