using System;
using Tinja.Extensions;
using Tinja.Resolving.Activation;
using Tinja.ServiceLife;

namespace Tinja.Resolving
{
    public class ServiceResolver : IServiceResolver
    {
        /// <summary>
        /// <see cref="IServiceLifeScope"/>
        /// </summary>
        public IServiceLifeScope ServiceLifeScope { get; }

        /// <summary>
        /// <see cref="IActivatorProvider"/>
        /// </summary>
        protected IActivatorProvider ServiceActivatorProvider { get; }

        public ServiceResolver(IActivatorProvider serviceActivatorProvider, IServiceLifeScopeFactory serviceLifeScopeFactory)
        {
            ServiceLifeScope = serviceLifeScopeFactory.Create(this);
            ServiceActivatorProvider = serviceActivatorProvider;
        }

        internal ServiceResolver(IServiceResolver root)
        {
            ServiceLifeScope = root.Resolve<IServiceLifeScopeFactory>().Create(this, root.ServiceLifeScope);
            ServiceActivatorProvider = root.Resolve<IActivatorProvider>();
        }

        public object Resolve(Type serviceType)
        {
            return ServiceActivatorProvider.Get(serviceType)?.Invoke(this, ServiceLifeScope);
        }

        public void Dispose()
        {
            ServiceLifeScope.Dispose();
        }
    }
}
