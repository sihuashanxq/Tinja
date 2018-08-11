using System;
using System.Collections.Generic;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection
{
    internal class ServiceLifeScope : IServiceLifeScope
    {
        internal bool IsDisposed { get; private set; }

        public IServiceLifeScope Root { get; }

        public IServiceCapturedFactory Factory { get; }

        public IServiceResolver ServiceResolver { get; }

        protected internal List<IDisposable> DisposableServices { get; }

        protected internal Dictionary<int, object> ResolvedServices { get; }

        public ServiceLifeScope(IServiceResolver serviceResolver, IServiceLifeScope scope)
        {
            if (scope == null)
            {
                throw new NullReferenceException(nameof(scope));
            }

            Root = scope.Root ?? scope;
            ServiceResolver = serviceResolver ?? throw new NullReferenceException(nameof(serviceResolver));

            Factory = new ServiceCapturedFactory(this);
            ResolvedServices = new Dictionary<int, object>();
            DisposableServices = new List<IDisposable>() { serviceResolver };
        }

        public ServiceLifeScope(IServiceResolver serviceResolver)
        {
            Root = this;
            ServiceResolver = serviceResolver ?? throw new NullReferenceException(nameof(serviceResolver));
            Factory = new ServiceCapturedFactory(this);
            ResolvedServices = new Dictionary<int, object>();
            DisposableServices = new List<IDisposable>() { serviceResolver };
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
            if (!disposing || IsDisposed)
            {
                return;
            }

            lock (this)
            {
                if (IsDisposed)
                {
                    return;
                }

                IsDisposed = true;

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
