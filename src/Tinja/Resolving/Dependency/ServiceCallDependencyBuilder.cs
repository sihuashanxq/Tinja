using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Extensions;
using Tinja.Resolving.Dependency.Scope;

namespace Tinja.Resolving.Dependency
{
    public class ServiceCallDependencyBuilder
    {
        protected ServiceCallDependencyScope CallDenpendencyScope { get; set; }

        protected IServiceContextBuilder ContextBuilder { get; set; }

        private IServiceContext _startContext;

        public ServiceCallDependencyBuilder(IServiceContextBuilder contextBuilder)
        {
            ContextBuilder = contextBuilder;
            CallDenpendencyScope = new ServiceCallDependencyScope();
        }

        public ServiceCallDependencyBuilder(ServiceCallDependencyScope callDenpendencyScope, IServiceContextBuilder contextBuilder)
        {
            ContextBuilder = contextBuilder;
            CallDenpendencyScope = callDenpendencyScope;
        }

        public virtual ServiceCallDependency Build(IServiceContext ctx)
        {
            if (_startContext == null)
            {
                _startContext = ctx;
            }

            return BuildCallDenpendency(ctx);
        }

        protected virtual ServiceCallDependency BuildCallDenpendency(IServiceContext ctx, ServiceCallDependencyScopeType scopeType = ServiceCallDependencyScopeType.None)
        {
            if (ctx is ServiceFactoryContext fatoryContext)
            {
                return CallDenpendencyScope.AddResolvedService(
                    ctx,
                    new ServiceCallDependency()
                    {
                        Constructor = null,
                        Context = ctx
                    }
                );
            }

            using (CallDenpendencyScope.BeginScope(ctx, ctx.GetImplementionType(), scopeType))
            {
                var callDependency = BuildTypeImplemention(ctx);
                if (callDependency != null)
                {
                    CallDenpendencyScope.AddResolvedService(ctx, callDependency);
                    return BuildPropertyCallDependency(callDependency);
                }

                return callDependency;
            }
        }

        protected virtual ServiceCallDependency BuildPropertyCallDependency(ServiceCallDependency callDependency)
        {
            if (callDependency.Context == _startContext)
            {
                return new ServiceCallDependencyPropertyBuilder(CallDenpendencyScope, ContextBuilder).BuildPropertyCallDependency(callDependency);
            }

            return callDependency;
        }

        protected virtual ServiceCallDependency BuildTypeImplemention(IServiceContext ctx)
        {
            switch (ctx)
            {
                case ServiceProxyContext serviceProxy:
                    return BuildProxyImplemention(serviceProxy);
                case ServiceEnumerableContext serviceEnumerable:
                    return BuildManyImplemention(serviceEnumerable);
                case ServiceTypeContext context:
                    return BuildNormalImplemention(context);
                default:
                    throw new InvalidOperationException();
            }
        }

        protected virtual ServiceCallDependency BuildNormalImplemention(ServiceTypeContext ctx)
        {
            var callDependencies = new Dictionary<ParameterInfo, ServiceCallDependency>();

            foreach (var constructor in ctx.GetConstructors().OrderByDescending(i => i.Paramters.Length))
            {
                foreach (var item in constructor.Paramters)
                {
                    var context = ContextBuilder.BuildContext(item.ParameterType);
                    if (context == null)
                    {
                        callDependencies.Clear();
                        break;
                    }

                    if (IsCircularDependency(context))
                    {
                        var result = ResolveParameterCircularDependency(ctx, context);
                        if (result.Break)
                        {
                            callDependencies.Clear();
                            break;
                        }
                        else if (result.CallDependency != null)
                        {
                            callDependencies[item] = result.CallDependency;
                        }
                    }

                    var callDependency = BuildCallDenpendency(context, ServiceCallDependencyScopeType.Parameter);
                    if (callDependency == null)
                    {
                        callDependencies.Clear();
                        break;
                    }

                    callDependencies[item] = callDependency;
                }

                if (callDependencies.Count == constructor.Paramters.Length)
                {
                    return new ServiceCallDependency()
                    {
                        Constructor = constructor,
                        Context = ctx,
                        Parameters = callDependencies
                    };
                }
            }

            return null;
        }

        protected virtual ServiceCallDependency BuildProxyImplemention(ServiceProxyContext ctx)
        {
            //Override
            if (ctx.ImplementionType.IsAssignableFrom(ctx.ProxyType))
            {
                return BuildNormalImplemention(new ServiceTypeContext(ctx.ServiceType, ctx.ProxyType, ctx.LifeStyle, ctx.ProxyConstructors));
            }

            var callDependencies = new Dictionary<ParameterInfo, ServiceCallDependency>();

            foreach (var constructor in ctx.ProxyConstructors.OrderByDescending(i => i.Paramters.Length))
            {
                foreach (var item in constructor.Paramters)
                {
                    var context = ContextBuilder.BuildContext(item.ParameterType);
                    if (context == null)
                    {
                        callDependencies.Clear();
                        break;
                    }

                    if (item.ParameterType == ctx.ImplementionType)
                    {
                        context = new ServiceTypeContext(context.ServiceType, context.GetImplementionType(), context.LifeStyle, context.GetConstructors());
                    }

                    if (IsCircularDependency(context))
                    {
                        var result = ResolveParameterCircularDependency(ctx, context);
                        if (result.Break)
                        {
                            callDependencies.Clear();
                            break;
                        }

                        if (result.CallDependency != null)
                        {
                            callDependencies[item] = result.CallDependency;
                        }
                    }

                    var callDependency = BuildCallDenpendency(context, ServiceCallDependencyScopeType.Parameter);
                    if (callDependency == null)
                    {
                        callDependencies.Clear();
                        break;
                    }

                    callDependencies[item] = callDependency;
                }

                if (callDependencies.Count == constructor.Paramters.Length)
                {
                    return new ServiceCallDependency()
                    {
                        Constructor = constructor,
                        Context = ctx,
                        Parameters = callDependencies
                    };
                }
            }

            return null;
        }

        protected virtual ServiceCallDependency BuildManyImplemention(ServiceEnumerableContext ctx)
        {
            var eles = new List<ServiceCallDependency>();

            for (var i = 0; i < ctx.ElementContexts.Count; i++)
            {
                var ele = BuildTypeImplemention(ctx.ElementContexts[i]);
                if (ele == null)
                {
                    continue;
                }

                eles.Add(ele);
            }

            return new ServiceManyCallDependency()
            {
                Context = ctx,
                Constructor = ctx.Constrcutors.FirstOrDefault(i => i.Paramters.Length == 0),
                Elements = eles.ToArray()
            };
        }

        protected virtual CircularDependencyResolveResult ResolveParameterCircularDependency(IServiceContext instance, IServiceContext constrcutorParameter)
        {
            throw new ServiceCallCircularExpcetion(constrcutorParameter.GetImplementionType(), $"Circulard ependencies at type:{constrcutorParameter.GetImplementionType().FullName}");
        }

        protected virtual bool IsCircularDependency(IServiceContext ctx)
        {
            if (ctx is ServiceFactoryContext)
            {
                return false;
            }

            return CallDenpendencyScope.Constains(ctx.GetImplementionType());
        }

        protected class CircularDependencyResolveResult
        {
            public bool Break { get; set; }

            public ServiceCallDependency CallDependency { get; set; }

            public static CircularDependencyResolveResult BreakResult = new CircularDependencyResolveResult
            {
                Break = true,
                CallDependency = null
            };
        }
    }
}
