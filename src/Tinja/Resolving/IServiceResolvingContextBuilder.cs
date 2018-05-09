using System;

namespace Tinja.Resolving
{
    public interface IServiceResolvingContextBuilder
    {
        IServiceResolvingContext BuildResolvingContext(Type serviceType);
    }
}
