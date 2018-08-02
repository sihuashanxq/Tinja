using System;

namespace Tinja.Abstractions.Injection
{
    public interface IServiceLifeScope : IDisposable
    {
        IServiceLifeScope Root { get; }

        IServiceCapturedFactory Factory { get; }
    }
}
