using System;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Activations;
using Tinja.Abstractions.Injection.Dependencies;

namespace Tinja.Core.Injection.Activations
{
    public class ActivatorFactory : IActivatorFactory
    {
        private readonly IActivatorBuilder _activatorBuilder;

        private readonly ICallDependElementBuilderFactory _callDependElementBuilderFactory;

        public ActivatorFactory(IServiceLifeScope serviceScope, ICallDependElementBuilderFactory factory)
        {
            _activatorBuilder = new ActivatorBuilder(serviceScope);
            _callDependElementBuilderFactory = factory;
        }

        public Func<IServiceResolver, IServiceLifeScope, object> CreateActivator(Type serviceType)
        {
            var builder = _callDependElementBuilderFactory.CreateBuilder();
            if (builder == null)
            {
                throw new NullReferenceException(nameof(builder));
            }

            var element = builder.Build(serviceType);
            if (element == null)
            {
                return null;
            }

            return _activatorBuilder.Build(element);
        }
    }
}
