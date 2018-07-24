using System;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Activators;
using Tinja.Abstractions.Injection.Extensions;

namespace Tinja.Core.Injection
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
        protected IActivatorProvider ActivatorProvider { get; }

        public ServiceResolver(IActivatorProvider serviceActivatorProvider, IServiceLifeScopeFactory serviceLifeScopeFactory)
        {
            ActivatorProvider = serviceActivatorProvider;
            ServiceLifeScope = serviceLifeScopeFactory.Create(this);
        }

        internal ServiceResolver(IServiceResolver parent)
        {
            ActivatorProvider = parent.Resolve<IActivatorProvider>();
            ServiceLifeScope = parent.Resolve<IServiceLifeScopeFactory>().Create(this, parent.ServiceLifeScope);
        }

        public object Resolve(Type serviceType)
        {
            return ActivatorProvider.Get(serviceType)?.Invoke(this, ServiceLifeScope);
        }

        public void Dispose()
        {
            ServiceLifeScope.Dispose();
        }
    }
}
