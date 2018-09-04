using System;
using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Activations;
using Tinja.Abstractions.Injection.Dependencies;
using Tinja.Core.Injection.Activations;

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
        internal ServiceResolver(ICallDependElementBuilderFactory factory)
        {
            if (factory == null)
            {
                throw new NullReferenceException(nameof(factory));
            }

            Scope = new ServiceLifeScope(this);
            Provider = new ActivatorProvider(Scope, factory);
        }

        /// <summary>
        /// 创建ServiceResolver:Scope
        /// </summary>
        /// <param name="serviceResolver"></param>
        internal ServiceResolver(IServiceResolver serviceResolver)
        {
            if (serviceResolver == null)
            {
                throw new NullReferenceException(nameof(serviceResolver));
            }

            var scope = serviceResolver.ResolveService<IServiceLifeScope>();
            if (scope == null)
            {
                throw new NullReferenceException(nameof(scope));
            }

            Provider = serviceResolver.Provider;
            Scope = (IServiceLifeScope)scope.Factory.CreateCapturedService((r, s) => new ServiceLifeScope(this, scope));
        }

        public object ResolveService(Type serviceType)
        {
            return Provider.Get(serviceType)?.Invoke(this, Scope);
        }

        public void Dispose()
        {
            Scope.Dispose();
        }
    }
}
