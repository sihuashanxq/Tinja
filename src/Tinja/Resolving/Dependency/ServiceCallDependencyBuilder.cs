using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Configuration;
using Tinja.Resolving.Context;
using Tinja.Resolving.Metadata;

namespace Tinja.Resolving.Dependency
{
    public class ServiceCallDependencyBuilder : IServiceCallDependencyElement
    {
        private ServiceContext _startContext;

        protected IServiceConfiguration Configuration { get; }

        protected IServiceContextFactory ContextFactory { get; set; }

        protected ServiceCallDependencyScope CallDenpendencyScope { get; set; }

        public ServiceCallDependencyBuilder(IServiceContextFactory contextFactory, IServiceConfiguration configuration)
        {
            Configuration = configuration;
            ContextFactory = contextFactory;
            CallDenpendencyScope = new ServiceCallDependencyScope();
        }

        public ServiceCallDependencyBuilder(
            ServiceCallDependencyScope callDenpendencyScope,
            IServiceContextFactory contextFactory,
            IServiceConfiguration configuration
        )
        {
            Configuration = configuration;
            ContextFactory = contextFactory;
            CallDenpendencyScope = callDenpendencyScope;
        }

        public virtual ServiceCallDependency Build(Type serviceType)
        {
            var ctx = ContextFactory.CreateContext(serviceType);
            if (ctx == null)
            {
                return null;
            }

            _startContext = ctx;

            return BuildCallDenpendency(ctx);
        }

        protected virtual ServiceCallDependency BuildCallDenpendency(ServiceContext ctx, ServiceCallDependencyScopeType scopeType = ServiceCallDependencyScopeType.None)
        {
            if (ctx.ImplementionFactory != null || ctx.ImplementionInstance != null)
            {
                return new ServiceCallDependency(ctx);
            }

            using (CallDenpendencyScope.BeginScope(ctx, ctx.ImplementionType, scopeType))
            {
                var callDependency = ResolveCallDependency(ctx);
                if (callDependency == null)
                {
                    return null;
                }

                AddResolvedService(ctx, callDependency);
                ResolvePropertyCallDependency(callDependency);

                return callDependency;
            }
        }

        protected virtual ServiceCallDependency ResolvePropertyCallDependency(ServiceCallDependency callDependency)
        {
            if (!Configuration.Injection.EnablePropertyInjection)
            {
                return callDependency;
            }

            if (callDependency.Context == _startContext)
            {
                return new ServiceCallDependencyPropertyBuilder(
                    CallDenpendencyScope,
                    ContextFactory,
                    Configuration
                ).ResolvePropertyCallDependency(callDependency);
            }

            return callDependency;
        }

        protected virtual ServiceCallDependency ResolveCallDependency(ServiceContext ctx)
        {
            switch (ctx)
            {
                case ServiceManyContext many:
                    return ResolveManyCallDependency(many);
                case ServiceProxyContext proxy:
                    return ResolveProxyCallDependency(proxy);
                default:
                    return ResolveDefaultCallDependency(ctx);
            }
        }

        protected virtual ServiceCallDependency ResolveDefaultCallDependency(ServiceContext ctx)
        {
            var callDependencies = new Dictionary<ParameterInfo, ServiceCallDependency>();

            foreach (var constructor in ctx.Constrcutors.OrderByDescending(i => i.Paramters.Length))
            {
                foreach (var item in constructor.Paramters)
                {
                    if (ResolveConstrucotrParameter(ctx, item, callDependencies))
                    {
                        break;
                    }
                }

                if (callDependencies.Count == constructor.Paramters.Length)
                {
                    return new ServiceCallDependency(ctx, constructor, callDependencies);
                }
            }

            return null;
        }

        protected bool ResolveConstrucotrParameter(
            ServiceContext ctx,
            ParameterInfo item,
            Dictionary<ParameterInfo, ServiceCallDependency> callDependencies
        )
        {
            var context = ContextFactory.CreateContext(item.ParameterType);
            if (context == null)
            {
                callDependencies.Clear();
                return true;
            }

            if (IsCircularDependency(context))
            {
                var result = ResolveParameterCircularDependency(context, ctx);
                if (result.Break)
                {
                    callDependencies.Clear();
                    return true;
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
                return true;
            }

            callDependencies[item] = callDependency;
            return false;
        }

        protected void AddResolvedService(ServiceContext ctx, ServiceCallDependency callDependency)
        {
            CallDenpendencyScope.AddResolvedService(ctx, callDependency);
        }

        protected virtual ServiceCallDependency ResolveProxyCallDependency(ServiceProxyContext ctx)
        {

            var callDependencies = new Dictionary<ParameterInfo, ServiceCallDependency>();

            foreach (var constructor in ctx.ProxyConstructors.OrderByDescending(i => i.Paramters.Length))
            {
                foreach (var item in constructor.Paramters)
                {
                    if (item.ParameterType == ctx.ImplementionType)
                    {
                        var proxyContext = new ServiceContext()
                        {
                            ServiceType = ctx.ServiceType,
                            ImplementionType = ctx.ImplementionType,
                            LifeStyle = ctx.LifeStyle,
                            Constrcutors = ctx.Constrcutors ?? new TypeConstructor[0],
                            ImplementionFactory = ctx.ImplementionFactory
                        };

                        var callProxyDependency = BuildCallDenpendency(proxyContext, ServiceCallDependencyScopeType.Parameter);
                        if (callProxyDependency == null)
                        {
                            callDependencies.Clear();
                            break;
                        }

                        callDependencies[item] = callProxyDependency;
                        continue;
                    }

                    if (ResolveConstrucotrParameter(ctx, item, callDependencies))
                    {
                        break;
                    }
                }

                if (callDependencies.Count == constructor.Paramters.Length)
                {
                    return new ServiceCallDependency(ctx, constructor, callDependencies);
                }
            }

            return null;
        }

        protected virtual ServiceCallDependency ResolveManyCallDependency(ServiceManyContext ctx)
        {
            var eles = new List<ServiceCallDependency>();

            foreach (var item in ctx.Elements)
            {
                var ele = BuildCallDenpendency(item);
                if (ele == null)
                {
                    continue;
                }

                eles.Add(ele);
            }

            return new ServiceManyCallDependency(ctx, ctx.Constrcutors.FirstOrDefault(i => i.Paramters.Length == 0), eles.ToArray());
        }

        protected virtual CircularDependencyResolveResult ResolveParameterCircularDependency(
            ServiceContext parameter,
            ServiceContext instance
        )
        {
            throw new ServiceCallCircularException(parameter.ImplementionType, $"Circulard ependencies at type:{parameter.ImplementionType.FullName}");
        }

        protected virtual bool IsCircularDependency(ServiceContext ctx)
        {
            return ctx.ImplementionFactory == null && ctx.ImplementionInstance == null && CallDenpendencyScope.Constains(ctx.ImplementionType);
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
