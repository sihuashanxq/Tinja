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

        protected Dictionary<Type, IResolvingContext> CacheContexts { get; set; }

        public ServiceChainScope()
        {
            ScopeContexts = new Dictionary<Type, IResolvingContext>();
            CacheContexts = new Dictionary<Type, IResolvingContext>();
            Chains = new Dictionary<IResolvingContext, IServiceChainNode>();
        }

        private ServiceChainScope(
            Dictionary<Type, IResolvingContext> scopeContexts,
            Dictionary<IResolvingContext, IServiceChainNode> chains
        ) : this()
        {
            foreach (var chain in chains)
            {
                Chains[chain.Key] = chain.Value;
            }

            foreach (var context in scopeContexts)
            {
                ScopeContexts[context.Key] = context.Value;
            }
        }

        public IDisposable BeginScope(IResolvingContext context, ServiceInfo serviceInfo)
        {
            ScopeContexts[serviceInfo.Type] = context;
            CacheContexts[serviceInfo.Type] = context;

            return new DisposableAction(() =>
            {
                ScopeContexts.Remove(serviceInfo.Type);
            });
        }

        public ServiceChainScope CreateCacheContextScope()
        {
            return new ServiceChainScope(CacheContexts, Chains);
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
