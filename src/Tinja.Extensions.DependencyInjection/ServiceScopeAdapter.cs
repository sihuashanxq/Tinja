using Microsoft.Extensions.DependencyInjection;
using System;
using Tinja.Resolving;

namespace Tinja.Extensions.DependencyInjection
{
    public class ServiceScopeAdapter : IServiceScope
    {
        public IServiceProvider ServiceProvider { get; }

        public ServiceScopeAdapter(IServiceResolver resolver)
        {
            ServiceProvider = resolver;
        }

        public void Dispose()
        {
            if (ServiceProvider is IDisposable dispose)
            {
                dispose.Dispose();
            }
        }
    }
}
