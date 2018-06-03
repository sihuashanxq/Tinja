using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Resolving.Dependency.Scope;

namespace Tinja.Resolving.Dependency.Builder
{
    public class ServiceConstructorDependencyChainBuilder
    {
        protected ServiceDependencyScope ServiceDependScope { get; set; }

        protected IServiceResolvingContextBuilder ResolvingContextBuilder { get; set; }

        private IServiceResolvingContext _startContext;

        public ServiceConstructorDependencyChainBuilder(IServiceResolvingContextBuilder resolvingContextBuilder)
            : this(new ServiceDependencyScope(), resolvingContextBuilder)
        {

        }

        public ServiceConstructorDependencyChainBuilder(ServiceDependencyScope scope, IServiceResolvingContextBuilder resolvingContextBuilder)
        {
            ServiceDependScope = scope;
            ResolvingContextBuilder = resolvingContextBuilder;
        }

        public virtual ServiceDependencyChain Build(IServiceResolvingContext target)
        {
            if (_startContext == null)
            {
                _startContext = target;
            }

            return BuildCore(target);
        }

        protected virtual ServiceDependencyChain BuildCore(
            IServiceResolvingContext target,
            ServiceDependScopeType scopeType = ServiceDependScopeType.None
        )
        {
            if (target.Component.ImplementionFactory != null)
            {
                return ServiceDependScope.AddResolvedService(
                    target,
                    new ServiceDependencyChain()
                    {
                        Constructor = null,
                        Context = target
                    }
                );
            }

            using (ServiceDependScope.BeginScope(target, target.ImplementationMeta.Type, scopeType))
            {
                var chain = BuildConstructor(target);
                if (chain == null)
                {
                    return chain;
                }

                ServiceDependScope.AddResolvedService(target, chain);

                return BuildProperties(chain);
            }
        }

        protected virtual ServiceDependencyChain BuildProperties(ServiceDependencyChain chain)
        {
            if (chain.Context == _startContext)
            {
                return new ServicePropertyDependencyChainBuilder(ServiceDependScope, ResolvingContextBuilder).BuildProperties(chain);
            }

            return chain;
        }

        protected virtual ServiceDependencyChain BuildConstructor(IServiceResolvingContext target)
        {
            return target is ServiceResolvingEnumerableContext eContext
                 ? BuildEnumerableConstructor(eContext)
                 : BuildCommonConstructor(target);
        }

        protected virtual ServiceDependencyChain BuildCommonConstructor(IServiceResolvingContext target)
        {
            var constructors = target.ImplementationMeta.Constructors;
            var parameters = new Dictionary<ParameterInfo, ServiceDependencyChain>();

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

                    var paramterChain = BuildCore(context, ServiceDependScopeType.Parameter);
                    if (paramterChain == null)
                    {
                        parameters.Clear();
                        break;
                    }

                    parameters[parameter] = paramterChain;
                }

                if (parameters.Count == item.Paramters.Length)
                {
                    return new ServiceDependencyChain()
                    {
                        Constructor = item,
                        Context = target,
                        Parameters = parameters
                    };
                }
            }

            return null;
        }

        protected virtual ServiceDependencyChain BuildEnumerableConstructor(ServiceResolvingEnumerableContext target)
        {
            var elements = new List<ServiceDependencyChain>();

            for (var i = 0; i < target.ElementContexts.Count; i++)
            {
                var ele = BuildConstructor(target.ElementContexts[i]);
                if (ele == null)
                {
                    continue;
                }

                elements.Add(ele);
            }

            return new ServiceDependencyEnumerableChain()
            {
                Context = target,
                Constructor = target.ImplementationMeta.Constructors.FirstOrDefault(i => i.Paramters.Length == 0),
                Elements = elements.ToArray()
            };
        }

        protected virtual CircularDependencyResolveResult ResolveParameterCircularDependency(IServiceResolvingContext target, IServiceResolvingContext parameter)
        {
            throw new ServiceCircularExpcetion(parameter.ImplementationMeta.Type, $"Circulard ependencies at type:{parameter.ImplementationMeta.Type.FullName}");
        }

        protected virtual bool IsCircularDependency(IServiceResolvingContext target)
        {
            return ServiceDependScope.Constains(target.ImplementationMeta.Type);
        }

        protected class CircularDependencyResolveResult
        {
            public bool Break { get; set; }

            public ServiceDependencyChain Chain { get; set; }

            public static CircularDependencyResolveResult BreakResult = new CircularDependencyResolveResult
            {
                Break = true,
                Chain = null
            };
        }
    }
}
