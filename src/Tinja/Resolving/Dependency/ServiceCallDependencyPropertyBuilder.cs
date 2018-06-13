using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.ServiceLife;
using Tinja.Resolving.Context;

namespace Tinja.Resolving.Dependency
{
    public class ServiceCallDependencyPropertyBuilder : ServiceCallDependencyBuilder
    {
        public ServiceCallDependencyPropertyBuilder(ServiceCallDependencyScope scope, IServiceContextFactory ctxFactory)
            : base(scope, ctxFactory)
        {

        }

        protected override ServiceCallDependency BuildPropertyCallDependency(ServiceCallDependency callDependency)
        {
            if (callDependency is ServiceManyCallDependency manyCallDependency)
            {
                foreach (var item in manyCallDependency
                    .Elements
                    .Where(i => i.Constructor != null))
                {
                    BuildPropertyCallDependencyCore(item);
                }

                return callDependency;
            }

            if (callDependency != null && callDependency.Constructor != null)
            {
                BuildPropertyCallDependencyCore(callDependency);

                if (callDependency.Parameters != null)
                {
                    foreach (var item in callDependency
                        .Parameters
                        .Where(i => i.Value.Constructor != null))
                    {
                        BuildPropertyCallDependency(item.Value);
                    }
                }
            }

            return callDependency;
        }

        protected virtual void BuildPropertyCallDependencyCore(ServiceCallDependency callDependency)
        {
            var properties = callDependency.Context.ImplementionType
                .GetProperties()
                .Where(i => i.CanRead && i.CanWrite && i.IsDefined(typeof(InjectAttribute)))
                .ToArray();

            if (properties.Length == 0)
            {
                return;
            }

            var callDependencies = new Dictionary<PropertyInfo, ServiceCallDependency>();

            foreach (var item in properties)
            {
                var context = ContextFactory.CreateContext(item.PropertyType);
                if (context == null)
                {
                    continue;
                }

                if (IsCircularDependency(context))
                {
                    var result = ResolvePropertyCircularDependency(context);
                    if (result.CallDependency != null)
                    {
                        callDependencies[item] = result.CallDependency;
                        continue;
                    }

                    if (result.Break)
                    {
                        continue;
                    }
                }

                var propertyChain = BuildCallDenpendency(context, ServiceCallDependencyScopeType.Property);
                if (propertyChain != null)
                {
                    callDependencies[item] = propertyChain;
                }
            }

            callDependency.Properties = callDependencies;
        }

        protected CircularDependencyResolveResult ResolvePropertyCircularDependency(ServiceContext context)
        {
            if (!CallDenpendencyScope
                .ServiceDependStack
                .Any(i => i.Context.LifeStyle != ServiceLifeStyle.Transient))
            {
                return new CircularDependencyResolveResult()
                {
                    Break = true
                };
            }

            var result = new CircularDependencyResolveResult()
            {
                Break = false,
                CallDependency = CallDenpendencyScope.ResolvedServices.GetValueOrDefault(context.ImplementionType)
            };

            if (result.CallDependency != null)
            {
                result.CallDependency.IsPropertyCircularDependencies = true;
            }

            return result;
        }

        protected override CircularDependencyResolveResult ResolveParameterCircularDependency(ServiceContext target, ServiceContext parameter)
        {
            if (target.LifeStyle == ServiceLifeStyle.Transient)
            {
                return CircularDependencyResolveResult.BreakResult;
            }

            //singleton /scope
            if (parameter.LifeStyle != ServiceLifeStyle.Transient)
            {
                return new CircularDependencyResolveResult()
                {
                    Break = false,
                    CallDependency = CallDenpendencyScope.ResolvedServices.GetValueOrDefault(parameter.ImplementionType)
                };
            }

            //parameter->property->parameter?
            var startIndex = CallDenpendencyScope.ServiceDependStack.ToList().FindIndex(i => i.Context.ServiceType == parameter.ServiceType);
            if (startIndex < 0)
            {
                return CircularDependencyResolveResult.BreakResult;
            }

            var scopes = CallDenpendencyScope.ServiceDependStack.Skip(startIndex).ToList();
            if (scopes.Any(i => i.ScopeType == ServiceCallDependencyScopeType.Parameter))
            {
                return CircularDependencyResolveResult.BreakResult;
            }

            if (scopes.Any(i => i.Context.LifeStyle != ServiceLifeStyle.Transient))
            {
                return new CircularDependencyResolveResult()
                {
                    Break = false,
                    CallDependency = null
                };
            }

            //twice to circle
            return new CircularDependencyResolveResult()
            {
                Break = scopes.Where(i => i.Context.ServiceType == parameter.ServiceType).Count() > 1,
                CallDependency = null
            };
        }
    }
}
