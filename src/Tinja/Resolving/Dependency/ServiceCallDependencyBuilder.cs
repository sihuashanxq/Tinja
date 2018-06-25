using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Resolving.Context;

namespace Tinja.Resolving.Dependency
{
    public class ServiceCallDependencyBuilder
    {
        protected ServiceCallDependencyScope CallDenpendencyScope { get; set; }

        protected IServiceContextFactory ContextFactory { get; set; }

        private ServiceContext _startContext;

        public ServiceCallDependencyBuilder(IServiceContextFactory ctxFactory)
        {
            ContextFactory = ctxFactory;
            CallDenpendencyScope = new ServiceCallDependencyScope();
        }

        public ServiceCallDependencyBuilder(ServiceCallDependencyScope callDenpendencyScope, IServiceContextFactory ctxFactory)
        {
            ContextFactory = ctxFactory;
            CallDenpendencyScope = callDenpendencyScope;
        }

        public virtual ServiceCallDependency Build(ServiceContext ctx)
        {
            if (_startContext == null)
            {
                _startContext = ctx;
            }

            return BuildCallDenpendency(ctx);
        }

        protected virtual ServiceCallDependency BuildCallDenpendency(ServiceContext ctx, ServiceCallDependencyScopeType scopeType = ServiceCallDependencyScopeType.None)
        {
            if (ctx.ImplementionFactory != null)
            {
                return CallDenpendencyScope.AddResolvedService(
                    ctx,
                    new ServiceCallDependency
                    {
                        Constructor = null,
                        Context = ctx
                    }
                );
            }

            using (CallDenpendencyScope.BeginScope(ctx, ctx.ImplementionType, scopeType))
            {
                var callDependency = BuildImplemention(ctx);
                if (callDependency != null)
                {
                    CallDenpendencyScope.AddResolvedService(ctx, callDependency);
                    return BuildPropertyCallDependency(callDependency);
                }

                return null;
            }
        }

        protected virtual ServiceCallDependency BuildPropertyCallDependency(ServiceCallDependency callDependency)
        {
            if (callDependency.Context == _startContext)
            {
                return new ServiceCallDependencyPropertyBuilder(CallDenpendencyScope, ContextFactory).BuildPropertyCallDependency(callDependency);
            }

            return callDependency;
        }

        protected virtual ServiceCallDependency BuildImplemention(ServiceContext ctx)
        {
            switch (ctx)
            {
                case ServiceProxyContext proxyCtx:
                    return BuildProxyImplemention(proxyCtx);
                case ServiceManyContext mayCtx:
                    return BuildManyImplemention(mayCtx);
                case ServiceContext typeCtx:
                    return BuildTypeImplemention(typeCtx);
                default:
                    throw new InvalidOperationException();
            }
        }

        protected virtual ServiceCallDependency BuildTypeImplemention(ServiceContext ctx)
        {
            var callDependencies = new Dictionary<ParameterInfo, ServiceCallDependency>();

            foreach (var constructor in ctx.Constrcutors.OrderByDescending(i => i.Paramters.Length))
            {
                foreach (var item in constructor.Paramters)
                {
                    var context = ContextFactory.CreateContext(item.ParameterType);
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

        protected virtual ServiceCallDependency BuildProxyImplemention(ServiceProxyContext ctx)
        {
            //Override
            if (ctx.ImplementionType.IsAssignableFrom(ctx.ProxyType))
            {
                return BuildTypeImplemention(new ServiceContext()
                {
                    ServiceType = ctx.ServiceType,
                    ImplementionType = ctx.ProxyType,
                    LifeStyle = ctx.LifeStyle,
                    Constrcutors = ctx.ProxyConstructors
                });
            }

            var callDependencies = new Dictionary<ParameterInfo, ServiceCallDependency>();

            foreach (var constructor in ctx.ProxyConstructors.OrderByDescending(i => i.Paramters.Length))
            {
                foreach (var item in constructor.Paramters)
                {
                    var context = ContextFactory.CreateContext(item.ParameterType);
                    if (context == null)
                    {
                        callDependencies.Clear();
                        break;
                    }

                    if (item.ParameterType == ctx.ImplementionType)
                    {
                        context = new ServiceContext()
                        {
                            ServiceType = context.ServiceType,
                            ImplementionType = context.ImplementionType,
                            LifeStyle = context.LifeStyle,
                            Constrcutors = context.Constrcutors ?? new TypeConstructor[0],
                            ImplementionFactory = context.ImplementionFactory
                        };
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

        protected virtual ServiceCallDependency BuildManyImplemention(ServiceManyContext ctx)
        {
            var eles = new List<ServiceCallDependency>();

            for (var i = 0; i < ctx.Elements.Count; i++)
            {
                var ele = BuildImplemention(ctx.Elements[i]);
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

        protected virtual CircularDependencyResolveResult ResolveParameterCircularDependency(ServiceContext instance, ServiceContext constrcutorParameter)
        {
            throw new ServiceCallCircularException(constrcutorParameter.ImplementionType, $"Circulard ependencies at type:{constrcutorParameter.ImplementionType.FullName}");
        }

        protected virtual bool IsCircularDependency(ServiceContext ctx)
        {
            return ctx.ImplementionFactory == null && CallDenpendencyScope.Constains(ctx.ImplementionType);
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
