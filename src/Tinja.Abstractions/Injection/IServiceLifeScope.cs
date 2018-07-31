using System;

namespace Tinja.Abstractions.Injection
{
    public interface IServiceLifeScope : IDisposable
    {
        IServiceFactory Factory { get; }

        IServiceLifeScope ServiceRootScope { get; }
    }
}
