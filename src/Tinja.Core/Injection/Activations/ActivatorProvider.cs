using System;
using System.Collections.Concurrent;
using Tinja.Abstractions.Injection.Graphs;

namespace Tinja.Core.Injection.Activations
{
    internal class ActivatorProvider
    {
        protected ActivatorBuilder ActivatorBuilder { get; }

        protected IGraphSiteBuilderFactory SiteBuilderFactory { get; }

        protected ConcurrentDictionary<Type, ActivatorCacheEntry> ActivatorCaches { get; }

        internal ActivatorProvider(ServiceLifeScope scope, IGraphSiteBuilderFactory factory)
        {
            ActivatorCaches = new ConcurrentDictionary<Type, ActivatorCacheEntry>();
            ActivatorBuilder = new ActivatorBuilder(scope.Root);
            SiteBuilderFactory = factory;
        }

        internal ActivatorDelegate Get(Type serviceType)
        {
            if (ActivatorCaches.TryGetValue(serviceType, out var cacheEntry))
            {
                if (cacheEntry.ActivatorDelegate != null)
                {
                    return cacheEntry.ActivatorDelegate;
                }

                return CreateDelegate(serviceType, null, false, cacheEntry);
            }

            return CreateDelegate(serviceType, null, false, null);
        }

        internal ActivatorDelegate Get(Type serviceType, string tag, bool tagOptional)
        {
            if (tag == null)
            {
                return Get(serviceType);
            }

            if (tagOptional)
            {
                //this situation only for lazy,no cache
                return CreateDelegate(serviceType, tag, tagOptional);
            }

            if (ActivatorCaches.TryGetValue(serviceType, out var cacheEntry))
            {
                if (cacheEntry.TaggedActivatorDelegates != null &&
                    cacheEntry.TaggedActivatorDelegates.TryGetValue(tag, out var activator))
                {
                    return activator;
                }

                return CreateDelegate(serviceType, tag, tagOptional, cacheEntry);
            }

            return CreateDelegate(serviceType, tag, tagOptional, null);
        }

        private ActivatorDelegate CreateDelegate(Type serviceType, string tag, bool tagOptional, ActivatorCacheEntry cacheEntry)
        {
            if (cacheEntry == null)
            {
                cacheEntry = new ActivatorCacheEntry(null);
                ActivatorCaches[serviceType] = cacheEntry;
            }

            if (tag == null)
            {
                return cacheEntry.ActivatorDelegate = CreateDelegate(serviceType, tag, tagOptional);
            }

            if (cacheEntry.TaggedActivatorDelegates == null)
            {
                cacheEntry.TaggedActivatorDelegates = new ConcurrentDictionary<string, ActivatorDelegate>();
            }

            return cacheEntry.TaggedActivatorDelegates[tag] = CreateDelegate(serviceType, tag, tagOptional);
        }

        private ActivatorDelegate CreateDelegate(Type serviceType, string tag, bool tagOptional)
        {
            var siteBuilder = SiteBuilderFactory.Create();
            if (siteBuilder == null)
            {
                return ActivatorUtil.DefaultActivator;
            }

            var site = siteBuilder.Build(serviceType, tag, tagOptional);
            if (site == null)
            {
                return ActivatorUtil.DefaultActivator;
            }

            return ActivatorBuilder.Build(site) ?? ActivatorUtil.DefaultActivator;
        }

        protected class ActivatorCacheEntry
        {
            public ActivatorDelegate ActivatorDelegate;

            public ConcurrentDictionary<string, ActivatorDelegate> TaggedActivatorDelegates;

            public ActivatorCacheEntry(ActivatorDelegate activator)
            {
                ActivatorDelegate = activator;
            }
        }
    }
}

