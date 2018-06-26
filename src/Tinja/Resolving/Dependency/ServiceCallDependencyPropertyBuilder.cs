using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.ServiceLife;
using Tinja.Resolving.Context;
using Tinja.Configuration;

namespace Tinja.Resolving.Dependency
{
    internal class ServiceCallDependencyPropertyBuilder : ServiceCallDependencyBuilder
    {
        internal ServiceCallDependencyPropertyBuilder(ServiceCallDependencyScope scope, IServiceContextFactory ctxFactory, IServiceConfiguration configuration)
            : base(scope, ctxFactory, configuration)
        {

        }

        protected override ServiceCallDependency ResolvePropertyCallDependency(ServiceCallDependency callDependency)
        {
            if (callDependency is ServiceManyCallDependency manyCallDependency)
            {
                foreach (var item in manyCallDependency.Elements.Where(i => i.Constructor != null))
                {
                    ResolvePropertyCallDependencyCore(item);
                }

                return callDependency;
            }

            ResolvePropertyCallDependencyCore(callDependency);

            foreach (var item in callDependency.Parameters.Where(i => i.Value.Constructor != null))
            {
                ResolvePropertyCallDependency(item.Value);
            }

            return callDependency;
        }

        protected virtual void ResolvePropertyCallDependencyCore(ServiceCallDependency callDependency)
        {
            if (callDependency.Context.ImplementionType == null ||
                callDependency.Context is ServiceProxyContext)
            {
                return;
            }

            var properties = callDependency
                .Context
                .ImplementionType
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

                var propCallDependency = BuildCallDenpendency(context, ServiceCallDependencyScopeType.Property);
                if (propCallDependency != null)
                {
                    callDependencies[item] = propCallDependency;
                }
            }

            callDependency.Properties = callDependencies;
        }

        protected override CircularDependencyResolveResult ResolveParameterCircularDependency(ServiceContext parameter, ServiceContext instance)
        {
            if (instance.LifeStyle == ServiceLifeStyle.Transient)
            {
                return CircularDependencyResolveResult.BreakResult;
            }

            //singleton /scope
            if (parameter.LifeStyle != ServiceLifeStyle.Transient)
            {
                return new CircularDependencyResolveResult()
                {
                    Break = false,
                    CallDependency = CallDenpendencyScope
                        .ResolvedServices
                        .GetValueOrDefault(parameter.ImplementionType)
                };
            }

            //parameter->property->parameter?
            var startIndex = CallDenpendencyScope
                .ServiceDependStack
                .ToList()
                .FindIndex(i => i.Context.ServiceType == parameter.ServiceType);

            if (startIndex < 0)
            {
                return CircularDependencyResolveResult.BreakResult;
            }

            var scopes = CallDenpendencyScope
                .ServiceDependStack
                .Skip(startIndex)
                .ToList();

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

            //circle depth
            return new CircularDependencyResolveResult()
            {
                Break = scopes.Count(i => i.Context.ServiceType == parameter.ServiceType) >= Configuration.Injection.PropertyCircularDepth,
                CallDependency = null
            };
        }

        protected CircularDependencyResolveResult ResolvePropertyCircularDependency(ServiceContext context)
        {
            if (CallDenpendencyScope.ServiceDependStack.All(i => i.Context.LifeStyle == ServiceLifeStyle.Transient))
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
    }
}
