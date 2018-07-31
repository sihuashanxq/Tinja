using Tinja.Abstractions.Injection.Configurations;
using Tinja.Abstractions.Injection.Dependency;
using Tinja.Abstractions.Injection.Descriptors;

namespace Tinja.Core.Injection.Dependency
{
    /// <summary>
    /// the default implementation for <see cref="ICallDependencyElementBuilderFactory"/>
    /// </summary>
    public class CallDependencyElementBuilderFactory : ICallDependencyElementBuilderFactory
    {
        private readonly ICallDependencyElementBuilder _builder;

        public CallDependencyElementBuilderFactory(IServiceDescriptorFactory serviceDescriptorFactory, IInjectionConfiguration configuration)
        {
            _builder = new CallDependencyElementBuilder(serviceDescriptorFactory, configuration);
        }

        public ICallDependencyElementBuilder CreateBuilder()
        {
            return _builder;
        }
    }
}
