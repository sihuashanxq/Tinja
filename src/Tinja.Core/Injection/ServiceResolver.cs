using System;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Graphs;
using Tinja.Core.Injection.Activations;

namespace Tinja.Core.Injection
{
    public class ServiceResolver : IServiceResolver
    {
        /// <summary>
        /// <see cref="ServiceLifeScope"/>
        /// </summary>
        internal ServiceLifeScope Scope { get; }

        /// <summary>
        /// <see cref="ActivatorProvider"/>
        /// </summary>
        internal ActivatorProvider Provider { get; }

        /// <summary>
        /// create root service resolver
        /// </summary>
        /// <param name="factory"></param>
        internal ServiceResolver(IGraphSiteBuilderFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            Scope = new ServiceLifeScope(this);
            Provider = new ActivatorProvider(Scope, factory);
        }

        /// <summary>
        /// create scoped service resolver
        /// </summary>
        /// <param name="serviceResolver"></param>
        internal ServiceResolver(ServiceResolver serviceResolver)
        {
            if (serviceResolver == null)
            {
                throw new ArgumentNullException(nameof(serviceResolver));
            }

            Provider = serviceResolver.Provider;
            Scope = new ServiceLifeScope(this, serviceResolver.Scope);
        }

        /// <summary>
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object ResolveService(Type serviceType)
        {
            return Provider.Get(serviceType)?.Invoke(this, Scope);
        }

        /// <summary>
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object ResolveService(Type serviceType, string tag)
        {
            return Provider.Get(serviceType, tag, false)?.Invoke(this, Scope);
        }

        /// <summary>
        /// call this method to resolve lazy's value 
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="tag"></param>
        /// <param name="optional"></param>
        /// <returns></returns>
        internal object ResolveService(Type serviceType, string tag, bool optional)
        {
            return Provider.Get(serviceType, tag, optional)?.Invoke(this, Scope);
        }

        ~ServiceResolver()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            Scope.Dispose();
        }
    }
}
