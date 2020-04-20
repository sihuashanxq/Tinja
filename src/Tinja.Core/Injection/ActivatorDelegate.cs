using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection
{
    internal delegate object ActivatorDelegate(IServiceResolver r, ServiceLifeScope s);
}
