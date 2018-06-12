using System;

namespace Tinja.Resolving.Context
{
    public interface IServiceContextFactory
    {
        IServiceContext CreateContext(Type serviceType);
    }
}
