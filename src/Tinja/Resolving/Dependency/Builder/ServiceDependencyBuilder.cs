using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Resolving.Context;
using Tinja.Resolving.Dependency.Scope;

namespace Tinja.Resolving.Dependency.Builder
{
    public class ServiceDependencyBuilder
    {
        protected ServiceDependScope ServiceDependScope { get; set; }

        protected IResolvingContextBuilder ResolvingContextBuilder { get; set; }

        private IResolvingContext _startContext;

        public ServiceDependencyBuilder(IResolvingContextBuilder resolvingContextBuilder)
            : this(new ServiceDependScope(), resolvingContextBuilder)
        {

        }

        public ServiceDependencyBuilder(ServiceDependScope scope, IResolvingContextBuilder resolvingContextBuilder)
        {
            ServiceDependScope = scope;
            ResolvingContextBuilder = resolvingContextBuilder;
        }

        public virtual ServiceDependChain BuildDependChain(IResolvingContext target)
        {
            if (_startContext == null)
            {
                _startContext = target;
            }

            return BuildDependChainCore(target);
        }

        protected virtual ServiceDependChain BuildDependChainCore(IResolvingContext target)
        {
            if (target.Component.ImplementionFactory != null)
            {
                return ServiceDependScope.AddResolvedService(
                    target,
                    new ServiceDependChain()
                    {
                        Constructor = null,
                        Context = target
                    }
                );
            }

            using (ServiceDependScope.BeginScope(target, target.ServiceInfo.Type))
            {
                var chain = BuildConstructorDependChain(target);
                if (chain != null)
                {
                    ServiceDependScope.AddResolvedService(target, chain);
                }

                return BuildPropertyDependChain(chain);
            }
        }

        protected virtual ServiceDependChain BuildPropertyDependChain(ServiceDependChain chain)
        {
            if (chain.Context == _startContext)
            {
                return new ServicePropertyDependencyBuilder(ServiceDependScope, ResolvingContextBuilder).BuildPropertyDependChain(chain);
            }

            return chain;
        }

        protected virtual ServiceDependChain BuildConstructorDependChain(IResolvingContext target)
        {
            return target is ResolvingEnumerableContext eContext
                 ? BuildEnumerableCommonConstructorDependChain(eContext)
                 : BuildCommonConstructorDependChain(target);
        }

        protected virtual ServiceDependChain BuildCommonConstructorDependChain(IResolvingContext target)
        {
            var constructors = target.ServiceInfo.Constructors;
            var parameters = new Dictionary<ParameterInfo, ServiceDependChain>();

            foreach (var item in constructors.OrderByDescending(i => i.Paramters.Length))
            {
                foreach (var parameter in item.Paramters)
                {
                    var context = ResolvingContextBuilder.BuildResolvingContext(parameter.ParameterType);
                    if (context == null)
                    {
                        parameters.Clear();
                        break;
                    }

                    if (IsCircularDependency(context))
                    {
                        var result = ResolveParameterCircularDependency(target, context);
                        if (result.Break)
                        {
                            parameters.Clear();
                            break;
                        }
                        else if (result.Chain != null)
                        {
                            parameters[parameter] = result.Chain;
                        }
                    }

                    var paramterChain = BuildConstructorDependChain(context);
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
                        Context = target,
                        Parameters = parameters
                    };
                }
            }

            return null;
        }

        protected virtual ServiceDependChain BuildEnumerableCommonConstructorDependChain(ResolvingEnumerableContext target)
        {
            var elements = new List<ServiceDependChain>();

            for (var i = 0; i < target.ElementContexts.Count; i++)
            {
                var ele = BuildConstructorDependChain(target.ElementContexts[i]);
                if (ele == null)
                {
                    continue;
                }

                elements.Add(ele);
            }

            return new ServiceEnumerableDependChain()
            {
                Context = target,
                Constructor = target.ServiceInfo.Constructors.FirstOrDefault(i => i.Paramters.Length == 0),
                Elements = elements.ToArray()
            };
        }

        protected virtual CircularDependencyResolveResult ResolveParameterCircularDependency(IResolvingContext target, IResolvingContext parameter)
        {
            throw new ServiceConstructorCircularExpcetion(parameter.ServiceInfo.Type, $"Circulard ependencies at type:{parameter.ServiceInfo.Type.FullName}");
        }

        protected virtual bool IsCircularDependency(IResolvingContext target)
        {
            return ServiceDependScope.Constains(target.ServiceInfo.Type);
        }

        protected class CircularDependencyResolveResult
        {
            public bool Break { get; set; }

            public ServiceDependChain Chain { get; set; }
        }
    }
}
