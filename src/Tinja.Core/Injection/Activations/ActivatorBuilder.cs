using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Graphs.Sites;

namespace Tinja.Core.Injection.Activations
{
    internal class ActivatorBuilder : GraphSiteVisitor<Expression>
    {
        internal ServiceLifeScope ServiceRootScope { get; }

        internal static ParameterExpression ParameterScope = Expression.Parameter(typeof(ServiceLifeScope));

        internal static ParameterExpression ParameterResolver = Expression.Parameter(typeof(IServiceResolver));

        internal ActivatorBuilder(ServiceLifeScope serviceScope)
        {
            ServiceRootScope = serviceScope ?? throw new ArgumentNullException(nameof(serviceScope));
        }

        internal ActivatorDelegate Build(GraphSite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            if (site is GraphDelegateSite delegateSite)
            {
                return BuildStaticDelegate(delegateSite);
            }

            if (site is GraphInstanceSite instanceSite)
            {
                return BuildStaticInstance(instanceSite);
            }

            if (site is GraphConstantSite constantSite)
            {
                return BuildStaticConstant(constantSite);
            }

            if (site is GraphValueProviderSite valueProviderSite)
            {
                return BuildStaticValueProvider(valueProviderSite);
            }

            return BuildDynamicExpression(site);
        }

        protected override Expression VisitType(GraphTypeSite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            var parameterInfos = site.ConstructorInfo.GetParameters();
            var parameterValues = new Expression[parameterInfos.Length];

            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var parameterType = parameterInfos[i].ParameterType;
                var parameterBinding = site.ParameterSites[parameterInfos[i]];
                if (parameterBinding == null)
                {
                    throw new NullReferenceException(nameof(parameterBinding));
                }

                var parameterValue = parameterBinding.Accept(this);
                if (parameterValue == null)
                {
                    throw new NullReferenceException(nameof(parameterValue));
                }

                if (parameterValue.Type.IsNotType(parameterType))
                {
                    parameterValue = Expression.Convert(parameterValue, parameterType);
                }

                parameterValues[i] = parameterValue;
            }

            var newExpression = Expression.New(site.ConstructorInfo, parameterValues);
            var memberInitExpression = BindInitProperties(newExpression, site);

            if (site.LifeStyle != ServiceLifeStyle.Transient ||
                site.ImplementationType.IsType<IDisposable>())
            {
                memberInitExpression = CaptureServiceLifeExpression(memberInitExpression, site);
            }

            return memberInitExpression;
        }

        protected override Expression VisitLazy(GraphLazySite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            var newExpression = (Expression)Expression.New(
                site.ConstructorInfo,
                Expression.Call(
                    ActivatorUtil.GetLazyValueFactoryMethod.MakeGenericMethod(site.ValueType),
                    ParameterScope,
                    Expression.Constant(site.Tag, typeof(string)),
                    Expression.Constant(site.TagOptional)
                )
            );

            if (site.LifeStyle != ServiceLifeStyle.Transient)
            {
                newExpression = CaptureServiceLifeExpression(newExpression, site);
            }

            return newExpression;
        }

        protected override Expression VisitConstant(GraphConstantSite site)
        {
            return Expression.Constant(site.Constant);
        }

        protected override Expression VisitInstance(GraphInstanceSite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            return CaptureServiceLifeExpression(Expression.Constant(site.Instance), site);
        }

        protected override Expression VisitDelegate(GraphDelegateSite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            if (site.Delegate == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            var facotry = site.Delegate;
            return CaptureServiceLifeExpression((r, s) => facotry(r), site);
        }

        protected override Expression VisitEnumerable(GraphEnumerableSite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            var items = new Expression[site.Elements.Length];

            for (var i = 0; i < items.Length; i++)
            {
                var elementValue = site.Elements[i].Accept(this);
                if (elementValue.Type.IsNotType(site.Elements[i].ServiceType))
                {
                    elementValue = Expression.Convert(elementValue, site.Elements[i].ServiceType);
                }

                items[i] = elementValue;
            }

            return Expression.NewArrayInit(site.ElementType, items);
        }

        protected override Expression VisitValueProvider(GraphValueProviderSite site)
        {
            return Expression.Invoke(Expression.Constant(site.Provider), ParameterResolver);
        }

        protected Expression BindInitProperties(NewExpression newExpression, GraphTypeSite site)
        {
            if (newExpression == null)
            {
                throw new ArgumentNullException(nameof(newExpression));
            }

            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            if (site.PropertySites == null || site.PropertySites.Count == 0)
            {
                return newExpression;
            }

            var properties = new List<MemberBinding>();

            foreach (var item in site.PropertySites)
            {
                var propertyValue = item.Value.Accept(this);
                if (propertyValue.Type.IsNotType(item.Key.PropertyType))
                {
                    propertyValue = Expression.Convert(item.Value.Accept(this), item.Key.PropertyType);
                }

                properties.Add(Expression.Bind(item.Key, propertyValue));
            }

            return Expression.MemberInit(newExpression, properties);
        }

        internal ActivatorDelegate BuildDynamicExpression(GraphSite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            var lambdaBody = Visit(site);
            if (lambdaBody == null)
            {
                throw new NullReferenceException(nameof(lambdaBody));
            }

            return Expression
                .Lambda<ActivatorDelegate>(lambdaBody, ParameterResolver, ParameterScope)
                .Compile();
        }

        internal ActivatorDelegate BuildStaticDelegate(GraphDelegateSite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            var factory = site.Delegate;
            return CaptureServiceLife((r, s) => factory(r), site);
        }

        internal ActivatorDelegate BuildStaticInstance(GraphInstanceSite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            var instance = site.Instance;
            var serviceId = site.ServiceId;

            ServiceRootScope.CreateCapturedScopedService(serviceId, (r, s) => instance);
          
            return (r, s) => instance;
        }

        internal ActivatorDelegate BuildStaticConstant(GraphConstantSite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            //primitive
            var instance = site.Constant;
            return (r, s) => instance;
        }

        internal ActivatorDelegate BuildStaticValueProvider(GraphValueProviderSite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            var factory = site.Provider;
            return CaptureServiceLife((r, s) => factory(r), site);
        }

        internal virtual ActivatorDelegate CaptureServiceLife(ActivatorDelegate activatorDelegate, GraphSite site)
        {
            if (activatorDelegate == null)
            {
                throw new ArgumentNullException(nameof(activatorDelegate));
            }

            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            if (site.ServiceType.IsType<IServiceResolver>())
            {
                return (r, s) => r;
            }

            if (site.ServiceType.IsType<IServiceLifeScope>())
            {
                return (r, s) => s;
            }

            switch (site.LifeStyle)
            {
                case ServiceLifeStyle.Scoped:
                    return CaptureScopedServiceFactory(activatorDelegate, site);
                case ServiceLifeStyle.Singleton:
                    return CaptureSingletonServiceFactory(activatorDelegate, site);
                default:
                    return CaptureTransientServiceFactory(activatorDelegate, site);
            }
        }

        internal virtual Expression CaptureServiceLifeExpression(Expression serviceExpression, GraphSite site)
        {
            if (serviceExpression == null)
            {
                throw new ArgumentNullException(nameof(serviceExpression));
            }

            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            if (site.ServiceType.IsType<IServiceResolver>())
            {
                return ParameterResolver;
            }

            if (site.ServiceType.IsType<IServiceLifeScope>())
            {
                return ParameterScope;
            }

            var activatorDelegate = Expression
                .Lambda<ActivatorDelegate>(serviceExpression, ParameterResolver, ParameterScope)
                .Compile();

            return CaptureServiceLifeExpression(activatorDelegate, site);
        }

        internal virtual Expression CaptureServiceLifeExpression(ActivatorDelegate activatorDelegate, GraphSite site)
        {
            if (activatorDelegate == null)
            {
                throw new ArgumentNullException(nameof(activatorDelegate));
            }

            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            if (site.ServiceType.IsType<IServiceLifeScope>())
            {
                return ParameterScope;
            }

            if (site.ServiceType.IsType<IServiceResolver>())
            {
                return ParameterResolver;
            }

            if (site.LifeStyle == ServiceLifeStyle.Singleton)
            {
                var serviveId = site.ServiceId;
                var instance = ServiceRootScope.CreateCapturedScopedService(serviveId, activatorDelegate);
                return Expression.Constant(instance);
            }

            return Expression.Invoke(
                Expression.Constant(CaptureServiceLife(activatorDelegate, site)),
                ParameterResolver,
                ParameterScope
            );
        }

        internal virtual ActivatorDelegate CaptureScopedServiceFactory(ActivatorDelegate activatorDelegate, GraphSite site)
        {
            var serviecId = site.ServiceId;
            return (r, s) => s.CreateCapturedScopedService(serviecId, activatorDelegate);
        }

        internal virtual ActivatorDelegate CaptureSingletonServiceFactory(ActivatorDelegate activatorDelegate, GraphSite site)
        {
            var serviceId = site.ServiceId;
            var instance = ServiceRootScope.CreateCapturedScopedService(serviceId, activatorDelegate);
            return (r, s) => instance;
        }

        internal virtual ActivatorDelegate CaptureTransientServiceFactory(ActivatorDelegate activatorDelegate, GraphSite site)
        {
            return (r, s) => s.CreateCapturedService(activatorDelegate);
        }
    }
}
