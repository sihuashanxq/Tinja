using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.LifeStyle;
using Tinja.Resolving.Context;
using Tinja.Resolving.Service;

namespace Tinja.Resolving.Dependency.Builder
{
    public class PropertyDependencyBuilder : ServiceDependencyBuilder
    {
        public PropertyDependencyBuilder(
            ServiceDependScope serviceDependScope,
            IServiceInfoFactory serviceInfoFactory,
            IResolvingContextBuilder resolvingContextBuilder
        ) : base(
                  serviceDependScope,
                  serviceInfoFactory,
                  resolvingContextBuilder
            )
        {

        }

        protected override ServiceDependChain ResolveCircularDependencies(IResolvingContext context, ServiceInfo serviceInfo)
        {
            if (context.Component.LifeStyle == ServiceLifeStyle.Transient)
            {
                return null;
            }

            if (!ServiceDependScope.AllContexts.TryGetValue(serviceInfo.Type, out var cachedContext))
            {
                return null;
            }

            if (ServiceDependScope.Chains.TryGetValue(cachedContext, out var chain))
            {
                if (chain != null)
                {
                    chain.IsPropertyCircularDependencies = true;
                }

                return chain;
            }

            return null;
        }

        protected override ServiceDependChain BuildDependChainCore(IResolvingContext context, ServiceInfo serviceInfo)
        {
            var chain = base.BuildDependChainCore(context, serviceInfo);
            if (chain != null)
            {
                return BuildPropertyDependChain(chain);
            }

            return chain;
        }

        public virtual ServiceDependChain BuildPropertyDependChain(ServiceDependChain chain)
        {
            if (chain == null || chain.Constructor == null)
            {
                return chain;
            }

            if (chain is ServiceEnumerableDependChain eNode)
            {
                foreach (var item in eNode.Elements.Where(i => i.Constructor != null))
                {
                    BuildPropertyDependChain(
                        item,
                        ServiceInfoFactory.Create(
                            item.Constructor.ConstructorInfo.DeclaringType
                        )
                    );
                }
            }
            else
            {
                BuildPropertyDependChain(
                    chain,
                    ServiceInfoFactory.Create(
                        chain.Constructor.ConstructorInfo.DeclaringType
                    )
                );

                foreach (var item in chain.Parameters.Where(i => i.Value.Constructor != null))
                {
                    BuildPropertyDependChain(item.Value);
                }
            }

            return chain;
        }

        protected virtual void BuildPropertyDependChain(ServiceDependChain chain, ServiceInfo serviceInfo)
        {
            if (serviceInfo.Properties == null || serviceInfo.Properties.Length == 0)
            {
                return;
            }

            var properties = new Dictionary<PropertyInfo, ServiceDependChain>();

            foreach (var item in serviceInfo.Properties)
            {
                var context = ResolvingContextBuilder.BuildResolvingContext(item.PropertyType);
                if (context == null)
                {
                    continue;
                }

                var propertyChain = BuildDependChainCore(context);
                if (propertyChain == null)
                {
                    continue;
                }

                properties[item] = propertyChain;
            }

            chain.Properties = properties;
        }
    }
}
