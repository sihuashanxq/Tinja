using System;
using System.Collections.Concurrent;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Graphs;

namespace Tinja.Core.Injection.Activations
{
    internal class ActivatorProvider
    {
        protected ActivatorBuilder ActivatorBuilder { get; }

        protected IGraphSiteBuilderFactory SiteBuilderFactory { get; }

        protected ConcurrentDictionary<Type, ActivatorCacheEntry> ActivatorCaches { get; }

        static readonly Func<IServiceResolver, IServiceLifeScope, object> DefaultActivator = (r, s) => null;

        internal ActivatorProvider(ServiceLifeScope scope, IGraphSiteBuilderFactory factory)
        {
            ActivatorCaches = new ConcurrentDictionary<Type, ActivatorCacheEntry>();
            ActivatorBuilder = new ActivatorBuilder(scope.Root);
            SiteBuilderFactory = factory;
        }

        internal Func<IServiceResolver, ServiceLifeScope, object> Get(Type serviceType)
        {
            if (ActivatorCaches.TryGetValue(serviceType, out var cacheEntry))
            {
                if (cacheEntry.Activator != null)
                {
                    return cacheEntry.Activator;
                }

                return Create(serviceType, null, cacheEntry);
            }

            return Create(serviceType, null, null);
        }

        internal Func<IServiceResolver, ServiceLifeScope, object> Get(Type serviceType, string tag)
        {
            if (tag == null)
            {
                return Get(serviceType);
            }

            if (ActivatorCaches.TryGetValue(serviceType, out var cacheEntry))
            {
                if (cacheEntry.Tags != null &&
                    cacheEntry.Tags.TryGetValue(tag, out var activator))
                {
                    return activator;
                }

                return Create(serviceType, tag, cacheEntry);
            }

            return Create(serviceType, tag, null);
        }

        private Func<IServiceResolver, ServiceLifeScope, object> Create(Type serviceType, string tag, ActivatorCacheEntry cacheEntry)
        {
            if (tag == null)
            {
                if (cacheEntry == null)
                {
                    cacheEntry = new ActivatorCacheEntry(Create(serviceType, tag));
                    ActivatorCaches[serviceType] = cacheEntry;
                }
                else
                {
                    cacheEntry.Activator = Create(serviceType, tag);
                }

                return cacheEntry.Activator;
            }

            var activator = Create(serviceType, tag);
            if (cacheEntry == null)
            {
                cacheEntry = new ActivatorCacheEntry(null)
                {
                    Tags = new ConcurrentDictionary<string, Func<IServiceResolver, ServiceLifeScope, object>>()
                };

                ActivatorCaches[serviceType] = cacheEntry;
            }

            if (cacheEntry.Tags == null)
            {
                cacheEntry.Tags = new ConcurrentDictionary<string, Func<IServiceResolver, ServiceLifeScope, object>>();
            }

            cacheEntry.Tags[tag] = activator;

            return activator;
        }

        private Func<IServiceResolver, ServiceLifeScope, object> Create(Type serviceType, string tag)
        {
            var siteBuilder = SiteBuilderFactory.Create();
            if (siteBuilder == null)
            {
                return DefaultActivator;
            }

            var site = siteBuilder.Build(serviceType, tag);
            if (site == null)
            {
                return DefaultActivator;
            }

            return ActivatorBuilder.Build(site) ?? DefaultActivator;
        }

        protected class ActivatorCacheEntry
        {
            public Func<IServiceResolver, ServiceLifeScope, object> Activator;

            public ConcurrentDictionary<string, Func<IServiceResolver, ServiceLifeScope, object>> Tags;

            public ActivatorCacheEntry(Func<IServiceResolver, ServiceLifeScope, object> activator)
            {
                Activator = activator;
            }
        }
    }
}

