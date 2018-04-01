using System;

namespace Tinja.Resolving.ReslovingContext
{
    public interface IResolvingContextBuilder
    {
        IResolvingContext BuildResolvingContext(Type serviceType);
    }
}
