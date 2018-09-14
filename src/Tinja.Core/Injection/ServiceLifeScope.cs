using System;
using System.Collections.Generic;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection
{
    internal class ServiceLifeScope : IServiceLifeScope
    {
        internal bool IsDisposed { get; private set; }

        public ServiceLifeScope Root { get; }

        public IServiceResolver ServiceResolver { get; }

        protected internal List<IDisposable> DisposableServices { get; }

        protected internal Dictionary<int, object> ResolvedServices { get; }

        internal ServiceLifeScope(ServiceResolver serviceResolver, ServiceLifeScope root)
        {
            if (root == null)
            {
                throw new NullReferenceException(nameof(root));
            }

            Root = root.Root ?? root;
            Root.CaputreDisposable(this);

            ResolvedServices = new Dictionary<int, object>();
            DisposableServices = new List<IDisposable>() { serviceResolver };
            ServiceResolver = serviceResolver ?? throw new NullReferenceException(nameof(serviceResolver));
        }

        internal ServiceLifeScope(ServiceResolver serviceResolver)
        {
            Root = this;

            ResolvedServices = new Dictionary<int, object>();
            DisposableServices = new List<IDisposable>() { serviceResolver };
            ServiceResolver = serviceResolver ?? throw new NullReferenceException(nameof(serviceResolver));
        }

        internal void CaputreDisposable(IDisposable disposable)
        {
            if (IsDisposed)
            {
                throw new InvalidOperationException($"this scope is disposed!");
            }

            DisposableServices.Add(disposable);
        }

        public object CreateCapturedService(Func<IServiceResolver, ServiceLifeScope, object> factory)
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

        public object CreateCapturedService(int serviceCacheId, Func<IServiceResolver, ServiceLifeScope, object> factory)
        {
            if (IsDisposed)
            {
                throw new InvalidOperationException($"this scope is disposed!");
            }

            lock (ResolvedServices)
            {
                if (ResolvedServices.TryGetValue(serviceCacheId, out var service))
                {
                    return service;
                }

                return ResolvedServices[serviceCacheId] = CreateCapturedService(factory);
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
                ResolvedServices.Clear();
            }
        }
    }
}
