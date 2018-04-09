using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Resolving.Chain.Node;
using Tinja.Resolving.Service;
using Tinja.Resolving.Context;

namespace Tinja.Resolving.Chain
{
    public class ServiceChainBuilder
    {
        protected ServiceChainScope ServiceChainScope { get; }

        protected IServiceInfoFactory ServiceInfoFactory { get; }

        protected IResolvingContextBuilder ResolvingContextBuilder { get; }

        public ServiceChainBuilder(
            ServiceChainScope serviceChainScope,
            IServiceInfoFactory serviceInfoFactory,
            IResolvingContextBuilder resolvingContextBuilder
        )
        {
            ServiceChainScope = serviceChainScope;
            ServiceInfoFactory = serviceInfoFactory;
            ResolvingContextBuilder = resolvingContextBuilder;
        }

        public virtual IServiceChainNode BuildChain(IResolvingContext resolvingContext)
        {
            if (resolvingContext.Component.ImplementionFactory != null)
            {
                return new ServiceConstrutorChainNode()
                {
                    Constructor = null,
                    Paramters = new Dictionary<ParameterInfo, IServiceChainNode>(),
                    Properties = new Dictionary<PropertyInfo, IServiceChainNode>(),
                    ResolvingContext = resolvingContext
                };
            }

            var implementionType = GetImplementionType(
                 resolvingContext.ReslovingType,
                 resolvingContext.Component.ImplementionType
             );

            var serviceInfo = ServiceInfoFactory.Create(implementionType);
            if (serviceInfo == null || serviceInfo.Constructors == null || serviceInfo.Constructors.Length == 0)
            {
                return null;
            }

            return BuildChain(resolvingContext, serviceInfo);
        }

        protected virtual IServiceChainNode BuildChain(IResolvingContext context, ServiceInfo serviceInfo)
        {
            using (ServiceChainScope.BeginScope(context, serviceInfo))
            {
                var node = BuildChainNode(context, serviceInfo);
                if (node != null)
                {
                    ServiceChainScope.Chains[context] = node;
                }

                return node;
            }
        }

        protected virtual IServiceChainNode BuildChainNode(IResolvingContext context, ServiceInfo serviceInfo)
        {
            if (context is ResolvingEnumerableContext eResolvingContext)
            {
                return BuildEnumerableChainNode(eResolvingContext, serviceInfo);
            }

            var parameters = new Dictionary<ParameterInfo, IServiceChainNode>();

            foreach (var item in serviceInfo.Constructors.OrderByDescending(i => i.Paramters.Length))
            {
                foreach (var parameter in item.Paramters)
                {
                    var paramterResolvingContext = ResolvingContextBuilder.BuildResolvingContext(parameter.ParameterType);
                    if (paramterResolvingContext == null)
                    {
                        parameters.Clear();
                        break;
                    }

                    var paramterChain = BuildChain(paramterResolvingContext);
                    if (paramterChain == null)
                    {
                        parameters.Clear();
                        break;
                    }

                    parameters[parameter] = paramterChain;
                }

                if (parameters.Count == item.Paramters.Length)
                {
                    return new ServiceConstrutorChainNode()
                    {
                        Constructor = item,
                        ResolvingContext = context,
                        Paramters = parameters
                    };
                }
            }

            return null;
        }

        protected IServiceChainNode BuildEnumerableChainNode(ResolvingEnumerableContext context, ServiceInfo serviceInfo)
        {
            var elements = new IServiceChainNode[context.ElementsResolvingContext.Count];

            for (var i = 0; i < elements.Length; i++)
            {
                var chain = BuildChain(context.ElementsResolvingContext[i]);
                if (chain == null)
                {
                    continue;
                }

                elements[i] = chain;
            }

            return new ServiceEnumerableChainNode()
            {
                Constructor = serviceInfo.Constructors.FirstOrDefault(i => i.Paramters.Length == 0),
                Paramters = new Dictionary<ParameterInfo, IServiceChainNode>(),
                ResolvingContext = context,
                Elements = elements
            };
        }

        internal static Type GetImplementionType(Type resolvingType, Type implementionType)
        {
            if (implementionType.IsGenericTypeDefinition && resolvingType.IsConstructedGenericType)
            {
                return implementionType.MakeGenericType(resolvingType.GenericTypeArguments);
            }

            return implementionType;
        }
    }
}
