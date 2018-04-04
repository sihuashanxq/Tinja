using System;

namespace Tinja.Resolving.Service
{
    public interface IServiceInfoFactory
    {
        ServiceInfo Create(Type type);
    }
}
