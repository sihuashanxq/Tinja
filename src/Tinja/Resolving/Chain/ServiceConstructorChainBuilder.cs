using System;
using Tinja.Resolving.Chain.Node;
using Tinja.Resolving.Service;
using Tinja.Resolving.Context;
using System.Collections.Generic;
using System.Reflection;

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
            var chain = base.BuildChain(resolvingContext);
            if (chain == null || chain.Constructor == null)
            {
                return chain;
            }

            var resolvedCacheScope = ServiceChainScope.CreateResolvedCacheScope();
            var builder = new ServicePropertyChainBuilder(resolvedCacheScope, ServiceInfoFactory, ResolvingContextBuilder);

            if (!resolvedCacheScope.Constains(chain))
            {
                resolvedCacheScope.AddChain(chain);
            }

            return builder.BuildProperties(chain);
        }

        protected override IServiceChainNode BuildChainNode(IResolvingContext context)
        {
            if (context.Component.ImplementionFactory != null)
            {
                return new ServiceConstrutorChainNode()
                {
                    Constructor = null,
                    Paramters = new Dictionary<ParameterInfo, IServiceChainNode>(),
                    Properties = new Dictionary<PropertyInfo, IServiceChainNode>(),
                    ResolvingContext = context
                };
            }

            var implementionType = GetImplementionType(
                 context.ReslovingType,
                 context.Component.ImplementionType
             );

            var serviceInfo = ServiceInfoFactory.Create(implementionType);
            if (serviceInfo == null || serviceInfo.Constructors == null || serviceInfo.Constructors.Length == 0)
            {
                return null;
            }

            if (ServiceChainScope.ScopeContexts.ContainsKey(serviceInfo.Type))
            {
                throw new NotSupportedException($"Circulard ependencies at type:{serviceInfo.Type.FullName}");
            }

            return base.BuildChainNode(context);
        }
    }
}
