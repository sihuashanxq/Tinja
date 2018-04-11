using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Resolving.Context;
using Tinja.Resolving.Service;

namespace Tinja.Resolving.Dependency.Builder
{
    public abstract class ServiceDependencyBuilder
    {
        protected ServiceDependScope ServiceDependScope { get; }

        protected IServiceInfoFactory ServiceInfoFactory { get; }

        protected IResolvingContextBuilder ResolvingContextBuilder { get; }

        public ServiceDependencyBuilder(
            ServiceDependScope serviceChainScope,
            IServiceInfoFactory serviceInfoFactory,
            IResolvingContextBuilder resolvingContextBuilder
        )
        {
            ServiceDependScope = serviceChainScope;
            ServiceInfoFactory = serviceInfoFactory;
            ResolvingContextBuilder = resolvingContextBuilder;
        }

        public virtual ServiceDependChain BuildDependChain(IResolvingContext context)
        {
            return BuildDependChainCore(context);
        }

        /// <summary>
        /// Resolve Circular Dependencies
        /// Constructor|Property
        /// </summary>
        /// <param name="serviceInfo"></param>
        /// <returns></returns>
        protected abstract ServiceDependChain ResolveCircularDependencies(IResolvingContext context, ServiceInfo serviceInfo);

        protected virtual ServiceDependChain BuildDependChainCore(IResolvingContext context)
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
                return ResolveCircularDependencies(context, serviceInfo);
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

        protected virtual ServiceDependChain BuildDependChainCore(IResolvingContext context, ServiceInfo serviceInfo)
        {
            if (context is ResolvingEnumerableContext eResolvingContext)
            {
                return BuildEnumerableDependChainCore(eResolvingContext, serviceInfo);
            }

            var parameters = new Dictionary<ParameterInfo, ServiceDependChain>();

            foreach (var item in serviceInfo.Constructors.OrderByDescending(i => i.Paramters.Length))
            {
                foreach (var parameter in item.Paramters)
                {
                    var paramterContext = ResolvingContextBuilder.BuildResolvingContext(parameter.ParameterType);
                    if (paramterContext == null)
                    {
                        parameters.Clear();
                        break;
                    }

                    var paramterChain = BuildDependChainCore(paramterContext);
                    if (paramterChain == null)
                    {
                        parameters.Clear();
                        break;
                    }

                    parameters[parameter] = paramterChain;
                }

                if (parameters.Count == item.Paramters.Length)
                {
                    return new ServiceDependChain()
                    {
                        Constructor = item,
                        Context = context,
                        Parameters = parameters
                    };
                }
            }

            return null;
        }

        protected ServiceDependChain BuildEnumerableDependChainCore(ResolvingEnumerableContext context, ServiceInfo serviceInfo)
        {
            var elements = new List<ServiceDependChain>();

            for (var i = 0; i < context.ElementContexts.Count; i++)
            {
                var ele = BuildDependChainCore(context.ElementContexts[i]);
                if (ele == null)
                {
                    continue;
                }

                elements.Add(ele);
            }

            return new ServiceEnumerableDependChain()
            {
                Context = context,
                Constructor = serviceInfo.Constructors.FirstOrDefault(i => i.Paramters.Length == 0),
                Elements = elements.ToArray()
            };
        }

        protected ServiceInfo GetServiceInfo(Type serviceType, Type implementionType)
        {
            if (implementionType.IsGenericTypeDefinition && serviceType.IsConstructedGenericType)
            {
                implementionType = implementionType.MakeGenericType(serviceType.GenericTypeArguments);
            }

            var serviceInfo = ServiceInfoFactory.Create(implementionType);
            if (serviceInfo == null || serviceInfo.Constructors == null || serviceInfo.Constructors.Length == 0)
            {
                return null;
            }

            return serviceInfo;
        }
    }
}
