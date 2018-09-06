using System;

namespace Tinja.Abstractions.Injection
{
    public interface IServiceLifeScope : IDisposable
    {
        IServiceResolver ServiceResolver { get; }
    }
}
