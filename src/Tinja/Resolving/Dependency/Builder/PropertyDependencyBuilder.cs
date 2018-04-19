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

        protected override ServiceDependChain BuildDependChainCore(IResolvingContext context)
        {
            if (context.Component.ImplementionFactory != null)
            {
                return new ServiceDependChain()
                {
                    Constructor = null,
                    Context = context
                };
            }

            var serviceInfo = GetServiceInfo(context.ServiceType, context.Component.ImplementionType);
            if (serviceInfo == null)
            {
                return null;
            }

            if (ServiceDependScope.ScopeContexts.ContainsKey(serviceInfo.Type))
            {
                var chain = ResolveCircularDependencies(context, serviceInfo);
                if (chain != null)
                {
                    return chain;
                }
            }

            using (ServiceDependScope.BeginScope(context, serviceInfo))
            {
                var chain = BuildDependChainCore(context, serviceInfo);
                if (chain != null)
                {
                    ServiceDependScope.Chains[context] = chain;
                }

                return chain;
            }
        }

        /// <summary>
        /// Resolve Circular Dependencies
        /// Constructor|Property
        /// </summary>
        /// <param name="serviceInfo"></param>
        /// <returns></returns>
        protected virtual ServiceDependChain ResolveCircularDependencies2(IResolvingContext context, ServiceInfo serviceInfo)
        {
            if (!ServiceDependScope.AllContexts.TryGetValue(serviceInfo.Type, out var cachedContext))
            {
                return null;
            }

            if (ServiceDependScope.Chains.TryGetValue(cachedContext.Context, out var chain))
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
            ServiceDependChain chain = null;

            try
            {
                chain = base.BuildDependChainCore(context, serviceInfo);
            }
            catch
            {
                return null;
            }

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

                ServiceDependChain propertyChain = null;

                try
                {
                    propertyChain = BuildDependChainCore(context);
                }
                catch (ConstructorCircularExpcetion)
                {
                    propertyChain = ResolveCircularDependencies2(context, GetServiceInfo(item.PropertyType, context.Component.ImplementionType));
                }
                catch
                {
                    continue;
                }

                if (propertyChain != null)
                {
                    properties[item] = propertyChain;
                }
            }

            chain.Properties = properties;
        }

        protected override ServiceDependChain ResolveCircularDependencies(IResolvingContext context, ServiceInfo serviceInfo)
        {
            if (context.Component.LifeStyle != ServiceLifeStyle.Transient)
            {
                throw new ConstructorCircularExpcetion(serviceInfo.Type, "");
            }

            if (ServiceDependScope.ScopeContexts[serviceInfo.Type].Counter > 0)
            {
                throw new ConstructorCircularExpcetion(serviceInfo.Type, "");
            }

            return null;
        }
    }
}
