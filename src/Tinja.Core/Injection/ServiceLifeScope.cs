using System;
using System.Collections.Generic;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection
{
    public class ServiceLifeScope : IServiceLifeScope
    {
        private bool _isDisposed;

        public IServiceFactory Factory { get; }

        public IServiceResolver ServiceResolver { get; }

        public IServiceLifeScope ServiceRootScope { get; }

        protected internal List<IDisposable> DisposableServices { get; }

        protected internal Dictionary<long, object> CacheResolvedServices { get; }

        public ServiceLifeScope(IServiceResolver serviceResolver, IServiceLifeScope scope)
        {
            ServiceResolver = serviceResolver;
            ServiceRootScope = scope.ServiceRootScope ?? scope;

            Factory = new ServiceFactory(this);
            DisposableServices = new List<IDisposable>();
            CacheResolvedServices = new Dictionary<long, object>();
        }

        public ServiceLifeScope(IServiceResolver serviceResolver)
        {
            ServiceRootScope = this;
            ServiceResolver = serviceResolver;
            Factory = new ServiceFactory(this);
            DisposableServices = new List<IDisposable>();
            CacheResolvedServices = new Dictionary<long, object>();
        }

        ~ServiceLifeScope()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing || _isDisposed)
            {
                return;
            }

            lock (this)
            {
                if (_isDisposed)
                {
                    return;
                }

                _isDisposed = true;

                foreach (var item in DisposableServices)
                {

                    item.Dispose();
                }

                DisposableServices.Clear();
                CacheResolvedServices.Clear();
            }
        }
    }
}
