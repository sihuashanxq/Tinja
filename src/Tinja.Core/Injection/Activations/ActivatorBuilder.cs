using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Graphs.Sites;

namespace Tinja.Core.Injection.Activations
{
    public class ActivatorBuilder : GraphSiteVisitor<Expression>
    {
        internal ServiceLifeScope ServiceRootScope { get; }

        internal static ParameterExpression ParameterScope = Expression.Parameter(typeof(ServiceLifeScope));

        internal static ParameterExpression ParameterResolver = Expression.Parameter(typeof(IServiceResolver));

        internal ActivatorBuilder(ServiceLifeScope serviceScope)
        {
            ServiceRootScope = serviceScope ?? throw new ArgumentNullException(nameof(serviceScope));
        }

        internal Func<IServiceResolver, ServiceLifeScope, object> Build(GraphSite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            if (site is GraphDelegateSite delegateSite)
            {
                return BuildDelegateFactory(delegateSite);
            }

            if (site is GraphInstanceSite instanceSite)
            {
                return BuildInstanceFactory(instanceSite);
            }

            return BuildExpressionFactory(site);
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
            var memberInit = BindInitProperties(newExpression, site);

            if (site.LifeStyle == ServiceLifeStyle.Transient && site.ImplementationType.IsNotType<IDisposable>())
            {
                return memberInit;
            }

            return CaptureServiceLife(memberInit, site);
        }

        protected override Expression VisitLazy(GraphLazySite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            var newExpression = site.Tag != null ?
                Expression.New(
                    site.ConstructorInfo,
                    Expression.Call(
                        ActivatorUtil.MakeTagLazyValueFactoryMethod(site.ValueType),
                        ParameterResolver,
                        Expression.Constant(site.Tag)
                    )
                ) :
                Expression.New(
                    site.ConstructorInfo,
                    Expression.Call(
                        ActivatorUtil.MakeLazyValueFactoryMethod(site.ValueType),
                        ParameterResolver
                    )
                );

            if (site.LifeStyle == ServiceLifeStyle.Transient)
            {
                return newExpression;
            }

            return CaptureServiceLife(newExpression, site);
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

            return CaptureServiceLife(Expression.Constant(site.Instance), site);
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

            return CaptureServiceLife((r, s) => site.Delegate(r), site);
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

        protected Expression CaptureServiceLife(Expression serviceExpression, GraphSite site)
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

            //optimization
            var lambda = Expression.Lambda(serviceExpression, ParameterResolver, ParameterScope);
            var compiledFunc = (Func<IServiceResolver, ServiceLifeScope, object>)lambda.Compile();

            return CaptureServiceLife(compiledFunc, site);
        }

        internal Expression CaptureServiceLife(Func<IServiceResolver, ServiceLifeScope, object> factory, GraphSite site)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
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
                return Expression.Constant(ServiceRootScope.CreateCapturedService(site.ServiceId, factory));
            }

            if (site.LifeStyle == ServiceLifeStyle.Transient)
            {
                return Expression.Invoke(ActivatorUtil.CreateCapturedTransientServie, ParameterScope, Expression.Constant(factory));
            }

            return Expression.Invoke(ActivatorUtil.CreateCapturedScopedService, Expression.Constant(site.ServiceId), ParameterScope, Expression.Constant(factory));
        }

        internal Func<IServiceResolver, ServiceLifeScope, object> BuildExpressionFactory(GraphSite site)
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

            return (Func<IServiceResolver, ServiceLifeScope, object>)
                Expression.Lambda(lambdaBody, ParameterResolver, ParameterScope).Compile();
        }

        internal Func<IServiceResolver, ServiceLifeScope, object> BuildDelegateFactory(GraphDelegateSite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            if (site.ServiceType.IsType<IServiceLifeScope>())
            {
                return (r, s) => s;
            }

            if (site.ServiceType.IsType<IServiceResolver>())
            {
                return (r, s) => r;
            }

            if (site.LifeStyle == ServiceLifeStyle.Transient)
            {
                return (r, s) => s.CreateCapturedService((r1, s1) => site.Delegate(r1));
            }

            if (site.LifeStyle == ServiceLifeStyle.Scoped)
            {

                return (r, s) => s.CreateCapturedService(site.ServiceId, (r1, s1) => site.Delegate(r1));
            }

            var instance = ServiceRootScope.CreateCapturedService(site.ServiceId, (r, s) => site.Delegate(r));

            return (r, s) => instance;
        }

        internal Func<IServiceResolver, ServiceLifeScope, object> BuildInstanceFactory(GraphInstanceSite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            ServiceRootScope.CreateCapturedService(site.ServiceId, (r, s) => site.Instance);

            return (r, s) => site.Instance;
        }
    }
}
