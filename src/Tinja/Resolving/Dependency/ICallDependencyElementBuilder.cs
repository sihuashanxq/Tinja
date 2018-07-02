using System;
using Tinja.Resolving.Dependency.Elements;

namespace Tinja.Resolving.Dependency
{
    public interface ICallDependencyElementBuilder
    {
        CallDepenencyElement Build(Type serviceType);
    }
}
