using System;
using System.Linq.Expressions;
using Tinja.Extensions;
using Tinja.Resolving.Dependency;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Activation.Builder
{
    public class ExpressionActivatorBuilder : CallDependencyElementVisitor<Expression>
    {
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

            return Expression.ListInit(Expression.New(element.ConstructorInfo), elementInits);
        }

        protected internal override Expression VisitInstance(InstanceCallDependencyElement element)
        {
            return
                Expression.Invoke(
                    ActivatorUtil.ApplyLifeConstant,
                    ActivatorUtil.ParameterScope,
                    Expression.Constant(element.ServiceType),
                    Expression.Constant(element.LifeStyle),
                    Expression.Constant((Func<IServiceResolver, object>)(_ => element.Instance))
                );
        }

        protected internal override Expression VisitDelegate(DelegateCallDepenencyElement element)
        {
            return
                Expression.Invoke(
                    ActivatorUtil.ApplyLifeConstant,
                    ActivatorUtil.ParameterScope,
                    Expression.Constant(element.ServiceType),
                    Expression.Constant(element.LifeStyle),
                    Expression.Constant(element.Delegate)
                );
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

                if (parameterType.IsAssignableFrom(parameterElement.ServiceType))
                {
                    parameterValues[i] = parameterValue;
                }
                else
                {
                    parameterValues[i] = Expression.Convert(parameterValue, parameterType);
                }
            }

            return ApplyServiceLifeStyle(Expression.New(element.ConstructorInfo, parameterValues), element);
        }

        protected internal virtual Expression ApplyServiceLifeStyle(Expression serviceExpression, ConstructorCallDependencyElement element)
        {
            if (element.LifeStyle == ServiceLifeStyle.Transient && !element.ImplementionType.Is(typeof(IDisposable)))
            {
                return serviceExpression;
            }

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
