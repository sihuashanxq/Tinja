using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Tinja.Extensions;
using Tinja.Resolving.Dependency.Elements;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Activation.Builder
{
    public class ExpressionActivatorBuilder : CallDependencyElementVisitor<Expression>, IActivatorBuilder
    {
        public static readonly IActivatorBuilder Default = new ExpressionActivatorBuilder();

        public virtual Func<IServiceResolver, IServiceLifeScope, object> Build(CallDepenencyElement element)
        {
            var lambdaBody = Visit(element);
            if (lambdaBody == null)
            {
                throw new NullReferenceException(nameof(lambdaBody));
            }

            if (lambdaBody.Type != typeof(object))
            {
                lambdaBody = Expression.Convert(lambdaBody, typeof(object));
            }

            return (Func<IServiceResolver, IServiceLifeScope, object>)
                Expression
                    .Lambda(lambdaBody, ActivatorUtil.ParameterResolver, ActivatorUtil.ParameterScope)
                    .Compile();
        }

        protected internal override Expression VisitMany(ManyCallDepenencyElement element)
        {
            var elementInits = new ElementInit[element.Elements.Length];
            var addElement = element.ImplementionType.GetMethod("Add");

            for (var i = 0; i < elementInits.Length; i++)
            {
                elementInits[i] = Expression.ElementInit(
                    addElement,
                    Expression.Convert(element.Elements[i].Accept(this), element.Elements[i].ServiceType)
                );
            }

            var listInit = Expression.ListInit(Expression.New(element.ConstructorInfo), elementInits);

            if (element.LifeStyle != ServiceLifeStyle.Transient ||
                element.ImplementionType.Is(typeof(IDisposable)))
            {
                return ResolveServiceLifeStyle(listInit, element);
            }

            return listInit;
        }

        protected internal override Expression VisitInstance(InstanceCallDependencyElement element)
        {
            return Expression.Constant(element.Instance);
        }

        protected internal override Expression VisitDelegate(DelegateCallDepenencyElement element)
        {
            var constant = Expression.Constant(element.Delegate);
            var invocation = Expression.Invoke(constant, ActivatorUtil.ParameterResolver);

            if (element.Delegate.Method.ReturnType == typeof(object) ||
                element.Delegate.Method.ReturnType.Is(typeof(IDisposable)) ||
                element.LifeStyle != ServiceLifeStyle.Transient)
            {
                return ResolveServiceLifeStyle(invocation, element);
            }

            return invocation;
        }

        protected internal override Expression VisitConstrcutor(ConstructorCallDependencyElement element)
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

                parameterValues[i] = Expression.Convert(parameterValue, parameterType);
            }

            var newExpression = Expression.New(element.ConstructorInfo, parameterValues);
            var memberInit = InitializeProperty(newExpression, element);

            if (element.LifeStyle != ServiceLifeStyle.Transient ||
                element.ImplementionType.Is(typeof(IDisposable)))
            {
                return ResolveServiceLifeStyle(memberInit, element);
            }

            return memberInit;
        }

        protected internal virtual Expression InitializeProperty(NewExpression newExpression, ConstructorCallDependencyElement element)
        {
            if (element?.Properties == null || element.Properties.Count == 0)
            {
                return newExpression;
            }

            var propertyBindings = new List<MemberBinding>();

            foreach (var item in element.Properties)
            {
                propertyBindings.Add(Expression.Bind(item.Key, Expression.Convert(item.Value.Accept(this), item.Key.PropertyType)));
            }

            return Expression.MemberInit(newExpression, propertyBindings);
        }

        protected internal virtual Expression ResolveServiceLifeStyle(Expression serviceExpression, CallDepenencyElement element)
        {
            //optimization
            var preCompiledFunc = (Func<IServiceResolver, IServiceLifeScope, object>)
                Expression
                    .Lambda(
                        serviceExpression,
                        ActivatorUtil.ParameterResolver,
                        ActivatorUtil.ParameterScope)
                    .Compile();

            var factory = (Func<IServiceResolver, object>)(resolver => preCompiledFunc(resolver, resolver.ServiceLifeScope));

            return
                Expression.Invoke(
                    ActivatorUtil.ApplyLifeConstant,
                    Expression.Constant(element.ServiceType),
                    Expression.Constant(element.LifeStyle),
                    ActivatorUtil.ParameterScope,
                    Expression.Constant(factory)
                );
        }
    }
}
