using System;
using Tinja.Resolving.Activation.Builder;
using Tinja.Resolving.Dependency;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Activation
{
    public class ActivatorFactory : IActivatorFactory
    {
        private readonly IServiceCallDependencyBuilderFactory _callDependencyBuilderFactory;

        public ActivatorFactory(IServiceCallDependencyBuilderFactory factory)
        {
            _callDependencyBuilderFactory = factory;
        }

        public Func<IServiceResolver, IServiceLifeScope, object> CreateActivator(Type serviceType)
        {
            var callDependencyBuilder = _callDependencyBuilderFactory.CreateBuilder();
            if (callDependencyBuilder == null)
            {
                throw new NullReferenceException(nameof(callDependencyBuilder));
            }

            var callDependency = callDependencyBuilder.Build(serviceType);
            if (callDependency == null)
            {
                return null;
            }

            if (callDependency.ContainsPropertyCircular())
            {
                return PropertyCircularActivatorBuilder
                    .Default
                    .Build(callDependency);
            }

            return ActivatorBuilder
                .Default
                .Build(callDependency);
        }
    }
}
