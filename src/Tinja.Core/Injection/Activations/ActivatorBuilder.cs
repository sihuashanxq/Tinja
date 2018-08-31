using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Activations;
using Tinja.Abstractions.Injection.Dependencies.Elements;

namespace Tinja.Core.Injection.Activations
{
    public class ActivatorBuilder : CallDependElementVisitor<Expression>, IActivatorBuilder
    {
        internal IServiceLifeScope ServiceRootScope { get; }

        internal static ParameterExpression ParameterScope = Expression.Parameter(typeof(IServiceLifeScope));

        internal static ParameterExpression ParameterResolver = Expression.Parameter(typeof(IServiceResolver));

        public ActivatorBuilder(IServiceLifeScope serviceScope)
        {
            ServiceRootScope = serviceScope ?? throw new NullReferenceException(nameof(serviceScope));
        }

        public virtual Func<IServiceResolver, IServiceLifeScope, object> Build(CallDependElement element)
        {
            if (element == null)
            {
                throw new NullReferenceException(nameof(element));
            }

            if (element is DelegateCallDependElement delegateElement)
            {
                return BuildDelegateFactory(delegateElement);
            }

            if (element is InstanceCallDependElement instanceElement)
            {
                return BuildInstanceFactory(instanceElement);
            }

            return BuildExpressionFactory(element);
        }

        protected override Expression VisitType(TypeCallDependElement element)
        {
            if (element == null)
            {
                throw new NullReferenceException(nameof(element));
            }

            var parameterInfos = element.ConstructorInfo.GetParameters();
            var parameterValues = new Expression[parameterInfos.Length];

            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var parameterType = parameterInfos[i].ParameterType;
                var parameterElement = element.Parameters[parameterInfos[i]];
                if (parameterElement == null)
                {
                    throw new NullReferenceException(nameof(parameterElement));
                }

                var parameterValue = parameterElement.Accept(this);
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

            var newExpression = Expression.New(element.ConstructorInfo, parameterValues);
            var memberInit = SetProperties(newExpression, element);

            if (element.LifeStyle == ServiceLifeStyle.Transient && element.ImplementionType.IsNotType<IDisposable>())
            {
                return memberInit;
            }

            return CaptureServiceLife(memberInit, element);
        }

        protected override Expression VisitInstance(InstanceCallDependElement element)
        {
            if (element == null)
            {
                throw new NullReferenceException(nameof(element));
            }

            return CaptureServiceLife(Expression.Constant(element.Instance), element);
        }

        protected override Expression VisitDelegate(DelegateCallDependElement element)
        {
            if (element == null)
            {
                throw new NullReferenceException(nameof(element));
            }

            if (element.Delegate == null)
            {
                throw new NullReferenceException(nameof(element));
            }

            return CaptureServiceLife(element.Delegate, element);
        }

        protected override Expression VisitEnumerable(EnumerableCallDependElement element)
        {
            if (element == null)
            {
                throw new NullReferenceException(nameof(element));
            }

            var items = new Expression[element.Items.Length];

            for (var i = 0; i < items.Length; i++)
            {
                var elementValue = element.Items[i].Accept(this);
                if (elementValue.Type.IsNotType(element.Items[i].ServiceType))
                {
                    elementValue = Expression.Convert(elementValue, element.Items[i].ServiceType);
                }

                items[i] = elementValue;
            }

            return Expression.NewArrayInit(element.ItemType, items);
        }

        protected Expression SetProperties(NewExpression newExpression, TypeCallDependElement element)
        {
            if (newExpression == null)
            {
                throw new NullReferenceException(nameof(newExpression));
            }

            if (element == null)
            {
                throw new NullReferenceException(nameof(element));
            }

            if (element.Properties == null || element.Properties.Count == 0)
            {
                return newExpression;
            }

            var properties = new List<MemberBinding>();

            foreach (var item in element.Properties)
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

        protected Expression CaptureServiceLife(Expression serviceExpression, CallDependElement element)
        {
            if (serviceExpression == null)
            {
                throw new NullReferenceException(nameof(serviceExpression));
            }

            if (element == null)
            {
                throw new NullReferenceException(nameof(element));
            }

            if (element.ServiceType.IsType<IServiceResolver>())
            {
                return ParameterResolver;
            }

            if (element.ServiceType.IsType<IServiceLifeScope>())
            {
                return ParameterScope;
            }

            //optimization
            var lambda = Expression.Lambda(serviceExpression, ParameterResolver, ParameterScope);
            var compiledFunc = (Func<IServiceResolver, IServiceLifeScope, object>)lambda.Compile();
            var valueFactory = (Func<IServiceResolver, object>)(resolver => compiledFunc(resolver, resolver.Scope));

            return CaptureServiceLife(valueFactory, element);
        }

        protected Expression CaptureServiceLife(Func<IServiceResolver, object> factory, CallDependElement element)
        {
            if (factory == null)
            {
                throw new NullReferenceException(nameof(factory));
            }

            if (element == null)
            {
                throw new NullReferenceException(nameof(element));
            }

            if (element.ServiceType.IsType<IServiceLifeScope>())
            {
                return ParameterScope;
            }

            if (element.ServiceType.IsType<IServiceResolver>())
            {
                return ParameterResolver;
            }

            if (element.LifeStyle == ServiceLifeStyle.Singleton)
            {
                var service = ServiceRootScope.Factory.CreateCapturedService(element.ServiceCacheId, factory);

                return Expression.Constant(service);
            }

            if (element.LifeStyle == ServiceLifeStyle.Transient)
            {
                return Expression.Invoke(ActivatorUtil.CreateCapturedTransientServie, ParameterScope, Expression.Constant(factory));
            }

            return Expression.Invoke(ActivatorUtil.CreateCapturedScopedService, Expression.Constant(element.ServiceCacheId), ParameterScope, Expression.Constant(factory));
        }

        protected Func<IServiceResolver, IServiceLifeScope, object> BuildExpressionFactory(CallDependElement element)
        {
            if (element == null)
            {
                throw new NullReferenceException(nameof(element));
            }

            var lambdaBody = Visit(element);
            if (lambdaBody == null)
            {
                throw new NullReferenceException(nameof(lambdaBody));
            }

            return (Func<IServiceResolver, IServiceLifeScope, object>)
                Expression.Lambda(lambdaBody, ParameterResolver, ParameterScope).Compile();
        }

        protected Func<IServiceResolver, IServiceLifeScope, object> BuildDelegateFactory(DelegateCallDependElement element)
        {
            if (element == null)
            {
                throw new NullReferenceException(nameof(element));
            }

            if (element.ServiceType.IsType<IServiceLifeScope>())
            {
                return (r, s) => s;
            }

            if (element.ServiceType.IsType<IServiceResolver>())
            {
                return (r, s) => r;
            }

            if (element.LifeStyle == ServiceLifeStyle.Singleton)
            {
                var service = ServiceRootScope.Factory.CreateCapturedService(element.ServiceCacheId, element.Delegate);
                
                return (r, s) => service;
            }

            return (r, s) => s.Factory.CreateCapturedService(element.ServiceCacheId, element.Delegate);
        }

        protected Func<IServiceResolver, IServiceLifeScope, object> BuildInstanceFactory(InstanceCallDependElement element)
        {
            if (element == null)
            {
                throw new NullReferenceException(nameof(element));
            }

            ServiceRootScope.Factory.CreateCapturedService(element.ServiceCacheId, r => element.Instance);

            return (r, s) => element.Instance;
        }
    }
}
