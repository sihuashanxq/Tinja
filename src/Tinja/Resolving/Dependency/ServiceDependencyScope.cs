using System;
using System.Collections.Generic;
using System.Linq;
using Tinja.ServiceLife;
using Tinja.Resolving;

namespace Tinja.Resolving.Dependency.Scope
{
    public class ServiceDependencyScope
    {
        public Stack<ServiceDependencyScopeEntry> ServiceDependStack { get; }

        public Dictionary<Type, ServiceDependencyChain> ResolvedServices { get; }

        public ServiceDependencyScope()
        {
            ServiceDependStack = new Stack<ServiceDependencyScopeEntry>();
            ResolvedServices = new Dictionary<Type, ServiceDependencyChain>();
        }

        public bool Constains(Type serviceType)
        {
            return ServiceDependStack.Any(i => i.ServiceType == serviceType);
        }

        public ServiceDependencyChain AddResolvedService(IServiceResolvingContext target, ServiceDependencyChain chain)
        {
            if (target.Component.LifeStyle != ServiceLifeStyle.Transient)
            {
                ResolvedServices[target.ImplementationTypeMeta.Type] = chain;
            }

            return chain;
        }

        public ScopeEntryDisposableWrapper BeginScope(
            IServiceResolvingContext target,
            Type servieType,
            ServiceDependScopeType scopeType = ServiceDependScopeType.None
        )
        {
            return BeginScope(new ServiceDependencyScopeEntry()
            {
                Context = target,
                ScopeType = scopeType,
                ServiceType = servieType
            });
        }

        public ScopeEntryDisposableWrapper BeginScope(ServiceDependencyScopeEntry entry)
        {
            ServiceDependStack.Push(entry);

            return new ScopeEntryDisposableWrapper(entry, () =>
            {
                ServiceDependStack.Pop();
            });
        }

        public class ScopeEntryDisposableWrapper : IDisposable
        {
            public ServiceDependencyScopeEntry ScopeEntry { get; }

            private Action _dispose;

            public ScopeEntryDisposableWrapper(ServiceDependencyScopeEntry scopeEntry, Action dispose)
            {
                _dispose = dispose;
                ScopeEntry = scopeEntry;
            }

            public void Dispose()
            {
                _dispose?.Invoke();
            }
        }
    }
}
