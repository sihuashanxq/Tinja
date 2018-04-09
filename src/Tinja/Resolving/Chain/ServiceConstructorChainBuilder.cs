using System;
using Tinja.Resolving.Chain.Node;
using Tinja.Resolving.Service;
using Tinja.Resolving.Context;

namespace Tinja.Resolving.Chain
{
    public class ServiceConstructorChainBuilder : ServiceChainBuilder
    {
        public ServiceConstructorChainBuilder(
            IServiceInfoFactory serviceInfoFactory,
            IResolvingContextBuilder resolvingContextBuilder
        ) : base(
                  new ServiceChainScope(),
                  serviceInfoFactory,
                  resolvingContextBuilder
            )
        {

        }

        public override IServiceChainNode BuildChain(IResolvingContext resolvingContext)
        {
            var node = base.BuildChain(resolvingContext);
            if (node != null && node.Constructor != null)
            {
                var propertyBinder = new ServicePropertyChainBuilder(
                    ServiceChainScope.CreateCacheContextScope(),
                    ServiceInfoFactory,
                    ResolvingContextBuilder
                );

                propertyBinder.BuildProperties(node);
            }

            return node;
        }

        protected override IServiceChainNode BuildChain(IResolvingContext context, ServiceInfo serviceInfo)
        {
            if (ServiceChainScope.ScopeContexts.ContainsKey(serviceInfo.Type))
            {
                throw new NotSupportedException($"Circulard ependencies at type:{serviceInfo.Type.FullName}");
            }

            return base.BuildChain(context, serviceInfo);
        }
    }
}
