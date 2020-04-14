using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Dependencies.Elements;

namespace Tinja.Core.Injection.Activations
{
    public class ActivatorBuilder : CallDependElementVisitor<Expression>
    {
        internal ServiceLifeScope ServiceRootScope { get; }

        internal static ParameterExpression ParameterScope = Expression.Parameter(typeof(ServiceLifeScope));

        internal static ParameterExpression ParameterResolver = Expression.Parameter(typeof(IServiceResolver));

        internal ActivatorBuilder(ServiceLifeScope serviceScope)
        {
            ServiceRootScope = serviceScope ?? throw new ArgumentNullException(nameof(serviceScope));
        }

        internal Func<IServiceResolver, ServiceLifeScope, object> Build(CallDependElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
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
                throw new ArgumentNullException(nameof(element));
            }

            var parameterInfos = element.ConstructorInfo.GetParameters();
            var parameterValues = new Expression[parameterInfos.Length];

            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var parameterType = parameterInfos[i].ParameterType;
                var parameterBinding = element.ParameterBindings[parameterInfos[i]];
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

            var newExpression = Expression.New(element.ConstructorInfo, parameterValues);
            var memberInit = SetProperties(newExpression, element);

            if (element.LifeStyle == ServiceLifeStyle.Transient && element.ImplementationType.IsNotType<IDisposable>())
            {
                return memberInit;
            }

            return CaptureServiceLife(memberInit, element);
        }

        protected override Expression VisitConstant(ConstantCallDependElement element)
        {
            return Expression.Constant(element.Constant);
        }

        protected override Expression VisitInstance(InstanceCallDependElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return CaptureServiceLife(Expression.Constant(element.Instance), element);
        }

        protected override Expression VisitDelegate(DelegateCallDependElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (element.Delegate == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return CaptureServiceLife((r, s) => element.Delegate(r), element);
        }

        protected override Expression VisitEnumerable(EnumerableCallDependElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
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

        protected override Expression VisitValueProvider(ValueProviderCallDependElement element)
        {
            return Expression.Invoke(Expression.Constant(element.Provider), ParameterResolver);
        }

        protected Expression SetProperties(NewExpression newExpression, TypeCallDependElement element)
        {
            if (newExpression == null)
            {
                throw new ArgumentNullException(nameof(newExpression));
            }

            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (element.PropertyBindings == null || element.PropertyBindings.Count == 0)
            {
                return newExpression;
            }

            var properties = new List<MemberBinding>();

            foreach (var item in element.PropertyBindings)
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
                throw new ArgumentNullException(nameof(serviceExpression));
            }

            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
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
            var compiledFunc = (Func<IServiceResolver, ServiceLifeScope, object>)lambda.Compile();

            return CaptureServiceLife(compiledFunc, element);
        }

        internal Expression CaptureServiceLife(Func<IServiceResolver, ServiceLifeScope, object> factory, CallDependElement element)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
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
                return Expression.Constant(ServiceRootScope.CreateCapturedService(element.ServiceId, factory));
            }

            if (element.LifeStyle == ServiceLifeStyle.Transient)
            {
                return Expression.Invoke(ActivatorUtil.CreateCapturedTransientServie, ParameterScope, Expression.Constant(factory));
            }

            return Expression.Invoke(ActivatorUtil.CreateCapturedScopedService, Expression.Constant(element.ServiceId), ParameterScope, Expression.Constant(factory));
        }

        internal Func<IServiceResolver, ServiceLifeScope, object> BuildExpressionFactory(CallDependElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var lambdaBody = Visit(element);
            if (lambdaBody == null)
            {
                throw new NullReferenceException(nameof(lambdaBody));
            }

            return (Func<IServiceResolver, ServiceLifeScope, object>)
                Expression.Lambda(lambdaBody, ParameterResolver, ParameterScope).Compile();
        }

        internal Func<IServiceResolver, ServiceLifeScope, object> BuildDelegateFactory(DelegateCallDependElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (element.ServiceType.IsType<IServiceLifeScope>())
            {
                return (r, s) => s;
            }

            if (element.ServiceType.IsType<IServiceResolver>())
            {
                return (r, s) => r;
            }

            if (element.LifeStyle == ServiceLifeStyle.Transient)
            {
                return (r, s) => s.CreateCapturedService((r1, s1) => element.Delegate(r1));
            }

            if (element.LifeStyle == ServiceLifeStyle.Scoped)
            {

                return (r, s) => s.CreateCapturedService(element.ServiceId, (r1, s1) => element.Delegate(r1));
            }

            var instance = ServiceRootScope.CreateCapturedService(element.ServiceId, (r, s) => element.Delegate(r));

            return (r, s) => instance;
        }

        internal Func<IServiceResolver, ServiceLifeScope, object> BuildInstanceFactory(InstanceCallDependElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            ServiceRootScope.CreateCapturedService(element.ServiceId, (r, s) => element.Instance);

            return (r, s) => element.Instance;
        }
    }
}
