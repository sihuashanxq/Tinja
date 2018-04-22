using System;
using System.Collections.Generic;
using System.Linq;
using Tinja.ServiceLife;
using Tinja.Resolving.Context;

namespace Tinja.Resolving.Dependency.Scope
{
    public class ServiceDependScope
    {
        public Stack<ServiceDependScopeEntry> ServiceDependStack { get; }

        public Dictionary<Type, ServiceDependChain> ResolvedServices { get; }

        public ServiceDependScope()
        {
            ServiceDependStack = new Stack<ServiceDependScopeEntry>();
            ResolvedServices = new Dictionary<Type, ServiceDependChain>();
        }

        public bool Constains(Type serviceType)
        {
            return ServiceDependStack.Any(i => i.ResolveServiceType == serviceType);
        }

        public ServiceDependChain AddResolvedService(IResolvingContext target, ServiceDependChain chain)
        {
            if (target.Component.LifeStyle != ServiceLifeStyle.Transient)
            {
                ResolvedServices[target.ServiceInfo.Type] = chain;
            }

            return chain;
        }

        public ScopeEntryDisposableWrapper BeginScope(
            IResolvingContext target,
            Type servieType,
            ServiceDependScopeType scopeType = ServiceDependScopeType.None
        )
        {
            return BeginScope(new ServiceDependScopeEntry()
            {
                Context = target,
                ScopeType = scopeType,
                ResolveServiceType = servieType
            });
        }

        public ScopeEntryDisposableWrapper BeginScope(ServiceDependScopeEntry entry)
        {
            ServiceDependStack.Push(entry);

            return new ScopeEntryDisposableWrapper(entry, () =>
            {
                ServiceDependStack.Pop();
            });
        }

        public class ScopeEntryDisposableWrapper : IDisposable
        {
            public ServiceDependScopeEntry ScopeEntry { get; }

            private Action _dispose;

            public ScopeEntryDisposableWrapper(ServiceDependScopeEntry scopeEntry, Action dispose)
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
