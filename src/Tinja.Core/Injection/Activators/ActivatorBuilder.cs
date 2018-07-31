using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Activators;
using Tinja.Abstractions.Injection.Dependency.Elements;

namespace Tinja.Core.Injection.Activators
{
    public class ActivatorBuilder : CallDependencyElementVisitor<Expression>, IActivatorBuilder
    {
        public static readonly IActivatorBuilder Default = new ActivatorBuilder();

        public virtual Func<IServiceResolver, IServiceLifeScope, object> Build(CallDepenencyElement element)
        {
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

        protected override Expression VisitMany(ManyCallDepenencyElement element)
        {
            var elements = new Expression[element.Elements.Length];

            for (var i = 0; i < elements.Length; i++)
            {
                var elementValue = element.Elements[i].Accept(this);
                if (elementValue.Type.IsNotType(element.Elements[i].ServiceType))
                {
                    elementValue = Expression.Convert(elementValue, element.Elements[i].ServiceType);
                }

                elements[i] = elementValue;
            }

            return Expression.NewArrayInit(element.ElementType, elements);
        }

        protected override Expression VisitInstance(InstanceCallDependencyElement element)
        {
            return Expression.Constant(element.Instance);
        }

        protected override Expression VisitDelegate(DelegateCallDepenencyElement element)
        {
            var constant = Expression.Constant(element.Delegate);
            var invocation = Expression.Invoke(constant, ActivatorUtil.ParameterResolver);

            if (element.Delegate.Method.ReturnType == typeof(object) ||
                element.Delegate.Method.ReturnType.IsType(typeof(IDisposable)) ||
                element.LifeStyle != ServiceLifeStyle.Transient)
            {
                return ResolveService(invocation, element);
            }

            return invocation;
        }

        protected override Expression VisitConstrcutor(ConstructorCallDependencyElement element)
        {
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

                if (parameterValue.Type.IsType(parameterType))
                {
                    parameterValues[i] = parameterValue;
                }
                else
                {
                    parameterValues[i] = Expression.Convert(parameterValue, parameterType);
                }
            }

            var newExpression = Expression.New(element.ConstructorInfo, parameterValues);
            var memberInit = InitializeProperty(newExpression, element);

            if (element.LifeStyle != ServiceLifeStyle.Transient || element.ImplementionType.IsType<IDisposable>())
            {
                return ResolveService(memberInit, element);
            }

            return memberInit;
        }

        protected virtual Expression InitializeProperty(NewExpression newExpression, ConstructorCallDependencyElement element)
        {
            if (element?.Properties == null || element.Properties.Count == 0)
            {
                return newExpression;
            }

            var propertyBindings = new List<MemberBinding>();

            foreach (var item in element.Properties)
            {
                var propertyValue = item.Value.Accept(this);
                if (propertyValue.Type.IsType(item.Key.PropertyType))
                {
                    propertyBindings.Add(Expression.Bind(item.Key, item.Value.Accept(this)));
                    continue;
                }

                propertyBindings.Add(Expression.Bind(item.Key, Expression.Convert(item.Value.Accept(this), item.Key.PropertyType)));
            }

            return Expression.MemberInit(newExpression, propertyBindings);
        }

        protected virtual Expression ResolveService(Expression serviceExpression, CallDepenencyElement element)
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
            var preCompiledFunc = (Func<IServiceResolver, IServiceLifeScope, object>)
                Expression
                    .Lambda(serviceExpression, ActivatorUtil.ParameterResolver, ActivatorUtil.ParameterScope)
                    .Compile();

            var factory = (Func<IServiceResolver, object>)(resolver => preCompiledFunc(resolver, resolver.Scope));

            switch (element.LifeStyle)
            {
                case ServiceLifeStyle.Transient:
                    return Expression.Invoke(ActivatorUtil.ResolveSingletonServiceConstant, ActivatorUtil.ParameterScope, Expression.Constant(factory));
                case ServiceLifeStyle.Scoped:
                    return Expression.Invoke(ActivatorUtil.ResolveScopedServiceConstant, Expression.Constant(element.ServiceId), ActivatorUtil.ParameterScope, Expression.Constant(factory));
                case ServiceLifeStyle.Singleton:
                    return Expression.Invoke(ActivatorUtil.ResolveSingletonServiceConstant, Expression.Constant(element.ServiceId), ActivatorUtil.ParameterScope, Expression.Constant(factory));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
