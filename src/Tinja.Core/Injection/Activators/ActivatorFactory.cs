using System;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Activators;
using Tinja.Abstractions.Injection.Dependency;

namespace Tinja.Core.Injection.Activators
{
    public class ActivatorFactory : IActivatorFactory
    {
        private readonly ICallDependencyElementBuilderFactory _callDependencyElementBuilderFactory;

        public ActivatorFactory(ICallDependencyElementBuilderFactory factory)
        {
            _callDependencyElementBuilderFactory = factory;
        }

        public Func<IServiceResolver, IServiceLifeScope, object> CreateActivator(Type serviceType)
        {
            var elementBuilder = _callDependencyElementBuilderFactory.CreateBuilder();
            if (elementBuilder == null)
            {
                throw new NullReferenceException(nameof(elementBuilder));
            }

            var element = elementBuilder.Build(serviceType);
            if (element == null)
            {
                return null;
            }

            return ActivatorBuilder.Default.Build(element);
        }
    }
}
