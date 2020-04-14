using System;
using Tinja.Abstractions.Injection.Dependencies.Elements;

namespace Tinja.Abstractions.Injection.Dependencies
{
    /// <summary>
    /// an interface for build <see cref="CallDependElement"/>
    /// </summary>
    public interface ICallDependElementBuilder
    {
        CallDependElement Build(Type serviceType);

        CallDependElement Build(Type serviceType, string tag);
    }
}
