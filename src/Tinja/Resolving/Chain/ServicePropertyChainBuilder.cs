using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Resolving.Chain.Node;
using Tinja.Resolving.Service;
using Tinja.Resolving.Context;

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

        public override IServiceChainNode BuildChain(IResolvingContext resolvingContext)
        {
            var node = base.BuildChain(resolvingContext);
            if (node != null)
            {
                BuildProperties(node);
            }

            return node;
        }

        protected override IServiceChainNode BuildChain(IResolvingContext context, ServiceInfo serviceInfo)
        {
            if (ServiceChainScope.ScopeContexts.TryGetValue(serviceInfo.Type, out var cachedContext))
            {
                if (cachedContext.Component.LifeStyle == LifeStyle.Transient)
                {
                    return null;
                }

                if (ServiceChainScope.Chains.TryGetValue(cachedContext, out var cachedNode))
                {
                    return cachedNode;
                }

                return null;
            }

            return base.BuildChain(context, serviceInfo);
        }

        public virtual void BuildProperties(IServiceChainNode node)
        {
            if (node is ServiceEnumerableChainNode eNode)
            {
                BuildProperties(
                    node,
                    ServiceInfoFactory.Create(
                        node.Constructor.ConstructorInfo.DeclaringType
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
                    node,
                    ServiceInfoFactory.Create(
                        node.Constructor.ConstructorInfo.DeclaringType
                    )
                );

                foreach (var item in node.Paramters.Where(i => i.Value.Constructor != null))
                {
                    BuildProperties(
                        item.Value,
                        ServiceInfoFactory.Create(
                            item.Value.Constructor.ConstructorInfo.DeclaringType
                        )
                    );
                }
            }
        }

        protected virtual void BuildProperties(IServiceChainNode node, ServiceInfo serviceInfo)
        {
            if (serviceInfo.Properties == null || serviceInfo.Properties.Length == 0)
            {
                return;
            }

            var propertyNodes = new Dictionary<PropertyInfo, IServiceChainNode>();

            foreach (var item in serviceInfo.Properties)
            {
                var context = ResolvingContextBuilder.BuildResolvingContext(item.PropertyType);
                if (context == null)
                {
                    continue;
                }

                if (context.Component.ImplementionFactory != null)
                {
                    propertyNodes[item] = new ServiceConstrutorChainNode()
                    {
                        Constructor = null,
                        Paramters = null,
                        ResolvingContext = context
                    };

                    continue;
                }

                var implementionType = GetImplementionType(
                    context.ReslovingType,
                    context.Component.ImplementionType
                );

                var propertyDescriptor = ServiceInfoFactory.Create(implementionType);
                if (propertyDescriptor == null)
                {
                    continue;
                }

                var propertyNode = BuildChain(context, propertyDescriptor);
                if (propertyNode == null)
                {
                    continue;
                }

                propertyNodes[item] = propertyNode;
            }

            node.Properties = propertyNodes;
        }
    }
}
