using System;
using System.Collections.Generic;
using Tinja.Resolving.Context;

namespace Tinja.Resolving.Dependency
{
    public class ServiceDependScope
    {
        public Dictionary<Type, IResolvingContext> ScopeContexts { get; private set; }

        public Dictionary<IResolvingContext, ServiceDependChain> Chains { get; private set; }

        public Dictionary<Type, IResolvingContext> AllContexts { get; private set; }

        public ServiceDependScope()
        {
            ScopeContexts = new Dictionary<Type, IResolvingContext>();
            AllContexts = new Dictionary<Type, IResolvingContext>();
            Chains = new Dictionary<IResolvingContext, ServiceDependChain>();
        }

        private ServiceDependScope(
            Dictionary<Type, IResolvingContext> allContexts,
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
            AllContexts[serviceInfo.Type] = context;
            ScopeContexts[serviceInfo.Type] = context;

            return new DisposableAction(() =>
            {
                ScopeContexts.Remove(serviceInfo.Type);
            });
        }

        public void AddChain(ServiceDependChain chain)
        {
            Chains[chain.Context] = chain;
            AllContexts[chain.Constructor.ConstructorInfo.DeclaringType] = chain.Context;
            ScopeContexts[chain.Constructor.ConstructorInfo.DeclaringType] = chain.Context;
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
