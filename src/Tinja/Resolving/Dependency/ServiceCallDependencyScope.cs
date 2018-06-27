using System;
using System.Collections.Generic;
using System.Linq;
using Tinja.Resolving.Context;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Dependency
{
    public class ServiceCallDependencyScope
    {
        public Stack<ServiceCallDependencyScopeEntry> ServiceDependStack { get; }

        public Dictionary<Type, ServiceCallDependency> ResolvedServices { get; }

        public ServiceCallDependencyScope()
        {
            ServiceDependStack = new Stack<ServiceCallDependencyScopeEntry>();
            ResolvedServices = new Dictionary<Type, ServiceCallDependency>();
        }

        public bool Constains(Type serviceType)
        {
            return ServiceDependStack.Any(i => i.ServiceType == serviceType);
        }

        public ServiceCallDependency AddResolvedService(ServiceContext ctx, ServiceCallDependency callDependency)
        {
            if (ctx.ImplementionFactory != null || ctx.ImplementionInstance != null)
            {
                return callDependency;
            }

            if (ctx.LifeStyle != ServiceLifeStyle.Transient)
            {
                ResolvedServices[ctx.ImplementionType] = callDependency;
            }

            return callDependency;
        }

        public ScopeEntryDisposableWrapper BeginScope(
            ServiceContext target,
            Type servieType,
            ServiceCallDependencyScopeType scopeType = ServiceCallDependencyScopeType.None
        )
        {
            return BeginScope(new ServiceCallDependencyScopeEntry()
            {
                Context = target,
                ScopeType = scopeType,
                ServiceType = servieType
            });
        }

        public ScopeEntryDisposableWrapper BeginScope(ServiceCallDependencyScopeEntry entry)
        {
            ServiceDependStack.Push(entry);

            return new ScopeEntryDisposableWrapper(entry, () =>
            {
                ServiceDependStack.Pop();
            });
        }

        public class ScopeEntryDisposableWrapper : IDisposable
        {
            public ServiceCallDependencyScopeEntry ScopeEntry { get; }

            private Action _dispose;

            public ScopeEntryDisposableWrapper(ServiceCallDependencyScopeEntry scopeEntry, Action dispose)
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
