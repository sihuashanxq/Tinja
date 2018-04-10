using Microsoft.Extensions.DependencyInjection;
using System;

namespace Tinja.Extensions.DependencyInjection
{
    public class ServiceScopeAdapter : IServiceScope
    {
        public IServiceProvider ServiceProvider { get; }

        public ServiceScopeAdapter(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
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
