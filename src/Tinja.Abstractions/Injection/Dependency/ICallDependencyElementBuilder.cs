using System;
using Tinja.Abstractions.Injection.Dependency.Elements;

namespace Tinja.Abstractions.Injection.Dependency
{
    public interface ICallDependencyElementBuilder
    {
        CallDepenencyElement Build(Type serviceType);
    }
}
