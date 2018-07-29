using System;
using Tinja.Abstractions.Injection.Dependency.Elements;

namespace Tinja.Abstractions.Injection.Dependency
{
    /// <summary>
    /// an interface for build <see cref="CallDepenencyElement"/>
    /// </summary>
    public interface ICallDependencyElementBuilder
    {
        CallDepenencyElement Build(Type serviceType);
    }
}
