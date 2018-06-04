using System;

namespace Tinja.Resolving
{
    public interface IServiceContextBuilder
    {
        IServiceContext BuildContext(Type serviceType);
    }
}
