using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Activators;
using Tinja.Abstractions.Injection.Dependency.Elements;

namespace Tinja.Core.Injection.Activators
{
    public class ActivatorBuilder : CallDependElementVisitor<Expression>, IActivatorBuilder
    {
        internal IServiceLifeScope ServiceRootScope { get; }

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

            var lambdaBody = Visit(element);
            if (lambdaBody == null)
            {
                throw new NullReferenceException(nameof(lambdaBody));
            }

            return (Func<IServiceResolver, IServiceLifeScope, object>)
                Expression
                    .Lambda(lambdaBody, ActivatorUtil.ParameterResolver, ActivatorUtil.ParameterScope)
                    .Compile();
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

            if (element.LifeStyle != ServiceLifeStyle.Transient ||
                element.ImplementionType.IsType<IDisposable>())
            {
                return CaptureServiceLife(memberInit, element);
            }

            return memberInit;
        }

        protected virtual Expression SetProperties(NewExpression newExpression, TypeCallDependElement element)
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

        protected virtual Expression CaptureServiceLife(Expression serviceExpression, CallDependElement element)
        {
            if (serviceExpression == null)
            {
                throw new NullReferenceException(nameof(serviceExpression));
            }

            if (element == null)
            {
                throw new NullReferenceException(nameof(element));
            }

            //optimization
            var lambda = Expression.Lambda(serviceExpression, ActivatorUtil.ParameterResolver, ActivatorUtil.ParameterScope);
            var compiledFunc = (Func<IServiceResolver, IServiceLifeScope, object>)lambda.Compile();
            var valueFactory = (Func<IServiceResolver, object>)(resolver => compiledFunc(resolver, resolver.Scope));

            return CaptureServiceLife(valueFactory, element);
        }

        protected virtual Expression CaptureServiceLife(Func<IServiceResolver, object> factory, CallDependElement element)
        {
            if (factory == null)
            {
                throw new NullReferenceException(nameof(factory));
            }

            if (element == null)
            {
                throw new NullReferenceException(nameof(element));
            }

            if (element.LifeStyle == ServiceLifeStyle.Singleton)
            {
                return Expression.Constant(ServiceRootScope.Factory.CreateService(element.ServiceId, factory));
            }

            if (element.LifeStyle == ServiceLifeStyle.Transient)
            {
                return Expression.Invoke(ActivatorUtil.CreateTransientServieConstant, ActivatorUtil.ParameterScope, Expression.Constant(factory));
            }

            return Expression.Invoke(ActivatorUtil.CreateScopedServiceConstant, Expression.Constant(element.ServiceId), ActivatorUtil.ParameterScope, Expression.Constant(factory));
        }
    }
}
