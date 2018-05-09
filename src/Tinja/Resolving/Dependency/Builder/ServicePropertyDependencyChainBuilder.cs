using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.ServiceLife;
using Tinja.Resolving;
using Tinja.Resolving.Dependency.Scope;

namespace Tinja.Resolving.Dependency.Builder
{
    public class ServicePropertyDependencyChainBuilder : ServiceConstructorDependencyChainBuilder
    {
        public ServicePropertyDependencyChainBuilder(ServiceDependencyScope scope, IServiceResolvingContextBuilder resolvingContextBuilder)
            : base(scope, resolvingContextBuilder)
        {

        }

        protected override ServiceDependencyChain BuildProperties(ServiceDependencyChain chain)
        {
            if (chain == null || chain.Constructor == null)
            {
                return chain;
            }

            if (chain is ServiceDependencyEnumerableChain eNode)
            {
                foreach (var item in eNode.Elements.Where(i => i.Constructor != null))
                {
                    BuildPropertiesCore(item);
                }
            }
            else
            {
                BuildPropertiesCore(chain);

                foreach (var item in chain.Parameters.Where(i => i.Value.Constructor != null))
                {
                    BuildProperties(item.Value);
                }
            }

            return chain;
        }

        protected virtual void BuildPropertiesCore(ServiceDependencyChain chain)
        {
            var meta = chain.Context.ImplementationTypeMeta;
            if (meta.Properties == null || meta.Properties.Length == 0)
            {
                return;
            }

            var properties = new Dictionary<PropertyInfo, ServiceDependencyChain>();

            foreach (var item in meta.Properties)
            {
                var context = ResolvingContextBuilder.BuildResolvingContext(item.PropertyType);
                if (context == null)
                {
                    continue;
                }

                if (IsCircularDependency(context))
                {
                    var result = ResolvePropertyCircularDependency(context);
                    if (result.Chain != null)
                    {
                        properties[item] = result.Chain;
                        continue;
                    }

                    if (result.Break)
                    {
                        continue;
                    }
                }

                var propertyChain = BuildCore(context, ServiceDependScopeType.Property);
                if (propertyChain != null)
                {
                    properties[item] = propertyChain;
                }
            }

            chain.Properties = properties;
        }

        protected CircularDependencyResolveResult ResolvePropertyCircularDependency(IServiceResolvingContext context)
        {
            if (!ServiceDependScope
                .ServiceDependStack
                .Any(i => i.Context.Component.LifeStyle != ServiceLifeStyle.Transient))
            {
                return new CircularDependencyResolveResult()
                {
                    Break = true
                };
            }

            var result = new CircularDependencyResolveResult()
            {
                Break = false,
                Chain = ServiceDependScope.ResolvedServices.GetValueOrDefault(context.ImplementationTypeMeta.Type)
            };

            if (result.Chain != null)
            {
                result.Chain.IsPropertyCircularDependencies = true;
            }

            return result;
        }

        protected override CircularDependencyResolveResult ResolveParameterCircularDependency(IServiceResolvingContext target, IServiceResolvingContext parameter)
        {
            if (target.Component.LifeStyle == ServiceLifeStyle.Transient)
            {
                return CircularDependencyResolveResult.BreakResult;
            }

            //singleton /scope
            if (parameter.Component.LifeStyle != ServiceLifeStyle.Transient)
            {
                return new CircularDependencyResolveResult()
                {
                    Break = false,
                    Chain = ServiceDependScope.ResolvedServices.GetValueOrDefault(parameter.ImplementationTypeMeta.Type)
                };
            }

            //parameter->property->parameter?
            var startIndex = ServiceDependScope.ServiceDependStack.ToList().FindIndex(i => i.Context.ServiceType == parameter.ServiceType);
            if (startIndex < 0)
            {
                return CircularDependencyResolveResult.BreakResult;
            }

            var scopes = ServiceDependScope.ServiceDependStack.Skip(startIndex).ToList();
            if (scopes.Any(i => i.ScopeType == ServiceDependScopeType.Parameter))
            {
                return CircularDependencyResolveResult.BreakResult;
            }

            if (scopes.Any(i => i.Context.Component.LifeStyle != ServiceLifeStyle.Transient))
            {
                return new CircularDependencyResolveResult()
                {
                    Break = false,
                    Chain = null
                };
            }

            //twice to circle
            return new CircularDependencyResolveResult()
            {
                Break = scopes.Where(i => i.Context.ServiceType == parameter.ServiceType).Count() > 1,
                Chain = null
            };
        }
    }
}
