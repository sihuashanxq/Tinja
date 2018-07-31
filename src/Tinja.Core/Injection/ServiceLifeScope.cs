using System;
using System.Collections.Generic;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection
{
    public class ServiceLifeScope : IServiceLifeScope
    {
        private bool _isDisposed;

        public IServiceResolver ServiceResolver { get; }

        public IServiceLifeScope ServiceRootScope { get; }

        protected List<IDisposable> DisposableServices { get; }

        protected Dictionary<long, object> CacheResolvedServices { get; }

        public ServiceLifeScope(IServiceResolver serviceResolver, IServiceLifeScope scope)
        {
            ServiceResolver = serviceResolver;
            ServiceRootScope = scope.ServiceRootScope ?? scope;
            DisposableServices = new List<IDisposable>();
            CacheResolvedServices = new Dictionary<long, object>();
        }

        public ServiceLifeScope(IServiceResolver serviceResolver)
        {
            ServiceRootScope = this;
            ServiceResolver = serviceResolver;
            DisposableServices = new List<IDisposable>();
            CacheResolvedServices = new Dictionary<long, object>();
        }

        public object ResolveService(Func<IServiceResolver, object> factory)
        {
            if (_isDisposed)
            {
                throw new NotSupportedException("the scope has disposed!");
            }

            var service = factory(ServiceResolver);

            CaptureDisposableService(service);

            return service;
        }

        public object ResolveCachedService(long cacheKey, Func<IServiceResolver, object> factory)
        {
            if (_isDisposed)
            {
                throw new NotSupportedException("the scope has disposed!");
            }

            if (CacheResolvedServices.TryGetValue(cacheKey, out var service))
            {
                return service;
            }

            lock (CacheResolvedServices)
            {
                if (CacheResolvedServices.TryGetValue(cacheKey, out service))
                {
                    return service;
                }

                service = CacheResolvedServices[cacheKey] = factory(ServiceResolver);

                CaptureDisposableService(service);

                return service;
            }
        }

        protected virtual void CaptureDisposableService(object service)
        {
            if (service is IDisposable disposable)
            {
                DisposableServices.Add(disposable);
            }
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
