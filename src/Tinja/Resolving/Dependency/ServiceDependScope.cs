using System;
using System.Collections.Generic;
using Tinja.Resolving.Context;

namespace Tinja.Resolving.Dependency
{
    public class ScopeItem
    {
        public IResolvingContext Context { get; set; }

        public int Counter { get; set; }
    }

    public class ServiceDependScope
    {
        public Dictionary<Type, ScopeItem> ScopeContexts { get; private set; }

        public Dictionary<IResolvingContext, ServiceDependChain> Chains { get; private set; }

        public Dictionary<Type, ScopeItem> AllContexts { get; private set; }

        public ServiceDependScope()
        {
            ScopeContexts = new Dictionary<Type, ScopeItem>();
            AllContexts = new Dictionary<Type, ScopeItem>();
            Chains = new Dictionary<IResolvingContext, ServiceDependChain>();
        }

        private ServiceDependScope(
            Dictionary<Type, ScopeItem> allContexts,
            Dictionary<IResolvingContext, ServiceDependChain> chains
        ) : this()
        {
            foreach (var chain in chains)
            {
                Chains[chain.Key] = chain.Value;
            }

            foreach (var context in allContexts)
            {
                AllContexts[context.Key] = context.Value;
            }
        }

        public ServiceDependScope CreateAllContextScope(ServiceDependChain startedChain)
        {
            var scope = new ServiceDependScope(AllContexts, Chains);
            if (!scope.Constains(startedChain))
            {
                scope.AddChain(startedChain);
            }

            return scope;
        }

        public IDisposable BeginScope(IResolvingContext context, ServiceInfo serviceInfo)
        {
            AllContexts[serviceInfo.Type] = new ScopeItem()
            {
                Context = context
            };

            if (!ScopeContexts.ContainsKey(serviceInfo.Type))
            {
                ScopeContexts[serviceInfo.Type] = new ScopeItem()
                {
                    Context = context,
                    Counter = 0
                };
            }

            ScopeContexts[serviceInfo.Type].Counter++;

            return new DisposableAction(() =>
            {
                ScopeContexts[serviceInfo.Type].Counter--;
                if (ScopeContexts[serviceInfo.Type].Counter == 0)
                {
                    ScopeContexts.Remove(serviceInfo.Type);
                }
            });
        }

        public void AddChain(ServiceDependChain chain)
        {
            Chains[chain.Context] = chain;
            AllContexts[chain.Constructor.ConstructorInfo.DeclaringType] = new ScopeItem()
            {
                Context = chain.Context
            };
            ScopeContexts[chain.Constructor.ConstructorInfo.DeclaringType] =
                new ScopeItem()
                {
                    Context = chain.Context,
                    Counter = 1
                };
        }

        public bool Constains(ServiceDependChain chain)
        {
            return ScopeContexts.ContainsKey(chain.Constructor?.ConstructorInfo?.DeclaringType);
        }

        private class DisposableAction : IDisposable
        {
            private Action _dispose;

            public DisposableAction(Action dispose)
            {
                _dispose = dispose;
            }

            public void Dispose()
            {
                _dispose?.Invoke();
            }
        }
    }
}
