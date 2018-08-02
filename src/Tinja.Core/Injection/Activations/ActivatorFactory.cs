using System;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Activations;
using Tinja.Abstractions.Injection.Dependencies;

namespace Tinja.Core.Injection.Activations
{
    public class ActivatorFactory : IActivatorFactory
    {
        private readonly IActivatorBuilder _builder;

        private readonly ICallDependencyElementBuilderFactory _callDependencyElementBuilderFactory;

        public ActivatorFactory(IServiceLifeScope serviceScope, ICallDependencyElementBuilderFactory factory)
        {
            _builder = new ActivatorBuilder(serviceScope);
            _callDependencyElementBuilderFactory = factory;
        }

        public Func<IServiceResolver, IServiceLifeScope, object> CreateActivator(Type serviceType)
        {
            var builder = _callDependencyElementBuilderFactory.CreateBuilder();
            if (builder == null)
            {
                throw new NullReferenceException(nameof(builder));
            }

            var element = builder.Build(serviceType);
            if (element == null)
            {
                return null;
            }

            return _builder.Build(element);
        }
    }
}
