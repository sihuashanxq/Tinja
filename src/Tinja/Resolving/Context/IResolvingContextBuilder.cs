using System;

namespace Tinja.Resolving.Context
{
    public interface IResolvingContextBuilder
    {
        IResolvingContext BuildResolvingContext(Type serviceType);
    }
}
