using System;
using Tinja.Abstractions.Injection.Dependency.Elements;

namespace Tinja.Abstractions.Injection.Activators
{
    public interface IActivatorBuilder
    {
        Func<IServiceResolver, IServiceLifeScope, object> Build(CallDepenencyElement element);
    }
}
