using System;

namespace Tinja.Abstractions.Injection
{
    public interface IServiceEntryFactory
    {
        ServiceEntry CreateEntry(Type serviceType, string tag);
    }
}
