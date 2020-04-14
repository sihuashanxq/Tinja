using System;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection
{
    /// <summary>
    /// the default implementataion for <see cref="IServiceLifeScopeFactory"/>
    /// </summary>
    internal class ServiceLifeScopeFactory : IServiceLifeScopeFactory
    {
        /// <summary>
        /// root <see cref="IServiceResolver"/>
        /// </summary>
        internal ServiceResolver RootServiceResolver { get; }

        /// <summary>
        /// </summary>
        /// <param name="serviceResolver">must be <see cref="ServiceResolver"/></param>
        public ServiceLifeScopeFactory(ServiceResolver serviceResolver)
        {
            RootServiceResolver = serviceResolver ?? throw new ArgumentNullException(nameof(serviceResolver));
        }

        /// <summary>
        /// create the IScopedServiceResolver
        /// </summary>
        /// <returns></returns>
        public IServiceLifeScope CreateScope()
        {
            return new ServiceResolver(RootServiceResolver).Scope;
        }
    }
}
