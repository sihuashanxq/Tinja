using System;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Activators;
using Tinja.Abstractions.Injection.Dependency;
using Tinja.Core.Injection.Activators;

namespace Tinja.Core.Injection
{
    public class ServiceResolver : IServiceResolver
    {
        /// <summary>
        /// <see cref="IServiceLifeScope"/>
        /// </summary>
        public IServiceLifeScope Scope { get; }

        /// <summary>
        /// <see cref="IActivatorProvider"/>
        /// </summary>
        public IActivatorProvider Provider { get; }

        /// <summary>
        /// 创建ServiceResolver:root
        /// </summary>
        internal ServiceResolver(ICallDependencyElementBuilderFactory factory)
        {
            Scope = new ServiceLifeScope(this);
            Provider = new ActivatorProvider(Scope, factory);
        }

        /// <summary>
        /// 创建ServiceResolver:Scope
        /// </summary>
        /// <param name="serviceResolver"></param>
        internal ServiceResolver(IServiceResolver serviceResolver)
        {
            Provider = serviceResolver.Provider;
            Scope = new ServiceLifeScope(this, serviceResolver.Scope);
        }

        public object Resolve(Type serviceType)
        {
            return Provider.Get(serviceType)?.Invoke(this, Scope);
        }

        public void Dispose()
        {
            Scope.Dispose();
        }
    }
}
