using System;
using Tinja.Resolving;

namespace Tinja.Extensions.DependencyInjection
{
    public class ServiceProviderAdapter : IServiceProvider, IDisposable
    {
        private bool _disposed;

        protected IServiceResolver Resolver { get; }

        public ServiceProviderAdapter(IServiceResolver resolver)
        {
            Resolver = resolver;
        }

        public virtual object GetService(Type serviceType)
        {
            return Resolver.Resolve(serviceType);
        }

        ~ServiceProviderAdapter()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool dispoing)
        {
            if (!dispoing || _disposed)
            {
                return;
            }

            lock (this)
            {
                if (!_disposed)
                {
                    _disposed = true;
                    Resolver.Dispose();
                }
            }
        }
    }
}
