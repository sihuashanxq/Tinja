using System;

namespace Tinja.Resolving.Context
{
    public interface IServiceContextFactory
    {
        ServiceContext CreateContext(Type serviceType);
    }
}
