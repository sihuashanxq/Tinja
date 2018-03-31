using System;
using Tinja.Registration;
using Tinja.Resolving;

namespace Tinja
{
    public interface IContainer : IServiceRegistrar, IServiceResolver, IDisposable
    {

    }
}
