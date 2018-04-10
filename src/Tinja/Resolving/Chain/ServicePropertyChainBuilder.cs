using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Resolving.Chain.Node;
using Tinja.Resolving.Service;
using Tinja.Resolving.Context;
using Tinja.LifeStyle;

namespace Tinja.Resolving.Chain
{
    public class ServicePropertyChainBuilder : ServiceChainBuilder
    {
        public ServicePropertyChainBuilder(
            ServiceChainScope serviceChainScope,
            IServiceInfoFactory serviceInfoFactory,
            IResolvingContextBuilder resolvingContextBuilder
        ) : base(
                  serviceChainScope,
                  serviceInfoFactory,
                  resolvingContextBuilder
            )
        {

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

            if (ServiceChainScope.ScopeContexts.TryGetValue(serviceInfo.Type, out var _))
            {
                if (context.Component.LifeStyle == ServiceLifeStyle.Transient)
                {
                    return null;
                }

                if (!ServiceChainScope.AllContexts.TryGetValue(serviceInfo.Type, out var cachedContext))
                {
                    return null;
                }

                if (ServiceChainScope.Chains.TryGetValue(cachedContext, out var cachedNode))
                {
                    return cachedNode;
                }

                return null;
            }

            return base.BuildChainNode(context);
        }

        protected override IServiceChainNode BuildChainNode(IResolvingContext context, ServiceInfo serviceInfo)
        {
            var chain = base.BuildChainNode(context, serviceInfo);
            if (chain != null)
            {
                return BuildProperties(chain);
            }

            return chain;
        }

        public virtual IServiceChainNode BuildProperties(IServiceChainNode chain)
        {
            if (chain == null || chain.Constructor == null)
            {
                return chain;
            }
            
            if (chain is ServiceEnumerableChainNode eNode)
            {
                BuildProperties(
                    chain,
                    ServiceInfoFactory.Create(
                        chain.Constructor.ConstructorInfo.DeclaringType
                    )
                );

                foreach (var item in eNode.Elements.Where(i => i.Constructor != null))
                {
                    BuildProperties(
                        item,
                        ServiceInfoFactory.Create(
                            item.Constructor.ConstructorInfo.DeclaringType
                        )
                    );
                }
            }
            else
            {
                BuildProperties(
                    chain,
                    ServiceInfoFactory.Create(
                        chain.Constructor.ConstructorInfo.DeclaringType
                    )
                );

                foreach (var item in chain.Paramters.Where(i => i.Value.Constructor != null))
                {
                    BuildProperties(
                        item.Value,
                        ServiceInfoFactory.Create(
                            item.Value.Constructor.ConstructorInfo.DeclaringType
                        )
                    );
                }

                foreach (var item in chain.Properties.Where(i => i.Value.Constructor != null))
                {
                    BuildProperties(
                        item.Value,
                        ServiceInfoFactory.Create(
                            item.Value.Constructor.ConstructorInfo.DeclaringType
                        )
                    );
                }
            }

            return chain;
        }

        protected virtual void BuildProperties(IServiceChainNode chain, ServiceInfo serviceInfo)
        {
            if (serviceInfo.Properties == null || serviceInfo.Properties.Length == 0)
            {
                return;
            }

            var propertyChains = new Dictionary<PropertyInfo, IServiceChainNode>();

            foreach (var item in serviceInfo.Properties)
            {
                var context = ResolvingContextBuilder.BuildResolvingContext(item.PropertyType);
                if (context == null)
                {
                    continue;
                }

                var propertyChain = BuildChainNode(context);
                if (propertyChain == null)
                {
                    continue;
                }

                propertyChains[item] = propertyChain;
            }

            chain.Properties = propertyChains;
        }
    }
}
