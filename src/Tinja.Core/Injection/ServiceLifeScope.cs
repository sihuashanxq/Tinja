using System;
using System.Collections.Generic;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection
{
    internal class ServiceLifeScope : IServiceLifeScope
    {
        internal bool IsDisposed { get; private set; }

        public ServiceLifeScope Root { get; }

        public IServiceResolver ServiceResolver => InternalServiceResolver;

        public ServiceResolver InternalServiceResolver { get; }

        protected internal List<IDisposable> DisposableServices { get; }

        protected internal Dictionary<int, object> ResolvedScopedServices { get; }

        internal ServiceLifeScope(ServiceResolver serviceResolver, ServiceLifeScope root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            Root = root.Root ?? root;

            DisposableServices = new List<IDisposable>() { serviceResolver };
            ResolvedScopedServices = new Dictionary<int, object>();
            InternalServiceResolver = serviceResolver ?? throw new ArgumentNullException(nameof(serviceResolver));
        }

        internal ServiceLifeScope(ServiceResolver serviceResolver)
        {
            Root = this;

            DisposableServices = new List<IDisposable>() { serviceResolver };
            ResolvedScopedServices = new Dictionary<int, object>();
            InternalServiceResolver = serviceResolver ?? throw new ArgumentNullException(nameof(serviceResolver));
        }

        public object CreateCapturedService(ActivatorDelegate factory)
        {
            if (IsDisposed)
            {
                throw new InvalidOperationException($"this scope is disposed!");
            }

            var service = factory(ServiceResolver, this);
            if (service is IDisposable disposable)
            {
                DisposableServices.Add(disposable);
            }

            return service;
        }

        public object CreateCapturedScopedService(int serviceId, ActivatorDelegate factory)
        {
            if (IsDisposed)
            {
                throw new InvalidOperationException($"this scope is disposed!");
            }

            lock (ResolvedScopedServices)
            {
                if (ResolvedScopedServices.TryGetValue(serviceId, out var service))
                {
                    return service;
                }

                return ResolvedScopedServices[serviceId] = CreateCapturedService(factory);
            }
        }

        ~ServiceLifeScope()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

                foreach (var disposable in DisposableServices)
                {
                    disposable.Dispose();
                }

                DisposableServices.Clear();
                ResolvedScopedServices.Clear();
            }
        }
    }
}
