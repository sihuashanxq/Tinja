using System;
using Tinja.Resolving.Context;
using Tinja.Resolving.Service;

namespace Tinja.Resolving.Dependency.Builder
{
    public class ConstructorDependencyBuilder : ServiceDependencyBuilder
    {
        public ConstructorDependencyBuilder(
            IServiceInfoFactory serviceInfoFactory,
            IResolvingContextBuilder resolvingContextBuilder
        ) : base(
                  new ServiceDependScope(),
                  serviceInfoFactory,
                  resolvingContextBuilder
            )
        {

        }

        public override ServiceDependChain BuildDependChain(IResolvingContext resolvingContext)
        {
            var chain = base.BuildDependChain(resolvingContext);
            if (chain == null || chain.Constructor == null)
            {
                return chain;
            }

            return new PropertyDependencyBuilder(
                  ServiceDependScope.CreateAllContextScope(chain),
                  ServiceInfoFactory,
                  ResolvingContextBuilder
            ).BuildPropertyDependChain(chain);
        }

        protected override ServiceDependChain ResolveCircularDependencies(IResolvingContext context, ServiceInfo serviceInfo)
        {
            throw new ConstructorCircularExpcetion(serviceInfo.Type, $"Circulard ependencies at type:{serviceInfo.Type.FullName}");
        }
    }
}
