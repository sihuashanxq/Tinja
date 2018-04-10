using System;
using System.Collections.Generic;
using Tinja.Resolving.Chain.Node;
using Tinja.Resolving.Context;

namespace Tinja.Resolving.Chain
{
    public class ServiceChainScope
    {
        public Dictionary<Type, IResolvingContext> ScopeContexts { get; private set; }

        public Dictionary<IResolvingContext, IServiceChainNode> Chains { get; private set; }

        public Dictionary<Type, IResolvingContext> AllContexts { get; private set; }

        public ServiceChainScope()
        {
            ScopeContexts = new Dictionary<Type, IResolvingContext>();
            AllContexts = new Dictionary<Type, IResolvingContext>();
            Chains = new Dictionary<IResolvingContext, IServiceChainNode>();
        }

        private ServiceChainScope(
            Dictionary<Type, IResolvingContext> allContexts,
            Dictionary<IResolvingContext, IServiceChainNode> chains
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

        public ServiceChainScope CreateResolvedCacheScope()
        {
            return new ServiceChainScope(AllContexts, Chains);
        }

        public IDisposable BeginScope(IResolvingContext context, ServiceInfo serviceInfo)
        {
            ScopeContexts[serviceInfo.Type] = context;
            AllContexts[serviceInfo.Type] = context;

            return new DisposableAction(() =>
            {
                ScopeContexts.Remove(serviceInfo.Type);
            });
        }

        public void AddChain(IServiceChainNode chain)
        {
            Chains[chain.ResolvingContext] = chain;
            AllContexts[chain.Constructor.ConstructorInfo.DeclaringType] = chain.ResolvingContext;
            ScopeContexts[chain.Constructor.ConstructorInfo.DeclaringType] = chain.ResolvingContext;
        }

        public bool Constains(IServiceChainNode chain)
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
