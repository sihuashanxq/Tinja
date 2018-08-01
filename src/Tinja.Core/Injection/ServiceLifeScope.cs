using System;
using System.Collections.Generic;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection
{
    internal class ServiceLifeScope : IServiceLifeScope
    {
        private bool _isDisposed;

        public IServiceFactory Factory { get; }

        public IServiceResolver ServiceResolver { get; }

        public IServiceLifeScope Root { get; }

        protected internal List<IDisposable> DisposableServices { get; }

        protected internal Dictionary<int, object> ResolvedServices { get; }

        public ServiceLifeScope(IServiceResolver serviceResolver, IServiceLifeScope scope)
        {
            ServiceResolver = serviceResolver;
            Root = scope.Root ?? scope;

            Factory = new ServiceFactory(this);
            DisposableServices = new List<IDisposable>();
            ResolvedServices = new Dictionary<int, object>();
        }

        public ServiceLifeScope(IServiceResolver serviceResolver)
        {
            Root = this;
            ServiceResolver = serviceResolver;
            Factory = new ServiceFactory(this);
            DisposableServices = new List<IDisposable>();
            ResolvedServices = new Dictionary<int, object>();
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
                ResolvedServices.Clear();
            }
        }
    }
}
