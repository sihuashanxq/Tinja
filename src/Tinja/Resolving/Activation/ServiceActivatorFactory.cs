using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Tinja.ServiceLife;
using Tinja.Resolving.Dependency;
using Tinja.Extensions;

namespace Tinja.Resolving.Activation
{
    public class ServiceActivatorFactory : IServiceActivatorFactory
    {
        private delegate object ApplyLifeStyleDelegate(
            IServiceLifeScope lifeScope,
            Type serviceType,
            ServiceLifeStyle lifeStyle,
            Func<IServiceResolver, object> factory
        );

        private static ParameterExpression ScopeParameter { get; }

        private static ParameterExpression ResolverParameter { get; }

        private static ConstantExpression ApplyLifeStyleFuncConstant { get; }

        private static ApplyLifeStyleDelegate ApplyLifeStyleFunc { get; }

        static ServiceActivatorFactory()
        {
            ScopeParameter = Expression.Parameter(typeof(IServiceLifeScope));
            ResolverParameter = Expression.Parameter(typeof(IServiceResolver));

            ApplyLifeStyleFunc = (scope, serviceType, lifeStyle, factory) => scope.ApplyServiceLifeStyle(serviceType, lifeStyle, factory);
            ApplyLifeStyleFuncConstant = Expression.Constant(ApplyLifeStyleFunc, typeof(ApplyLifeStyleDelegate));
        }

        public Func<IServiceResolver, IServiceLifeScope, object> Create(ServiceCallDependency callDependency)
        {
            var factory = CreateActivatorCore(callDependency);
            if (factory == null)
            {
                return (resolver, scope) => null;
            }

            return factory;
        }

        public Expression BuildPropertyInfo(Expression instance, ServiceCallDependency callDependency)
        {
            if (instance.Type != callDependency.Constructor.ConstructorInfo.DeclaringType)
            {
                instance = Expression.Convert(instance, callDependency.Constructor.ConstructorInfo.DeclaringType);
            }

            var label = Expression.Label(instance.Type);
            var instanceVariable = Expression.Variable(instance.Type);
            var variables = new List<ParameterExpression>()
            {
                instanceVariable
            };

            var statements = new List<Expression>()
            {
                Expression.Assign(instanceVariable, instance),
                Expression.IfThen(
                    Expression.Equal(Expression.Constant(null), instanceVariable),
                    Expression.Return(label, instanceVariable)
                )
            };

            foreach (var item in callDependency.Properties)
            {
                var property = Expression.MakeMemberAccess(instanceVariable, item.Key);
                var propertyVariable = Expression.Variable(item.Key.PropertyType, item.Key.Name);

                var propertyValue = BuildExpression(item.Value);
                if (!propertyVariable.Type.IsAssignableFrom(propertyValue.Type))
                {
                    propertyValue = Expression.Convert(propertyValue, propertyVariable.Type);
                }

                var setPropertyVariableValue = Expression.Assign(propertyVariable, propertyValue);
                var setPropertyValue = Expression.IfThen(
                    Expression.NotEqual(Expression.Constant(null), propertyVariable),
                    Expression.Assign(property, propertyVariable)
                );

                variables.Add(propertyVariable);
                statements.Add(setPropertyVariableValue);
                statements.Add(setPropertyValue);
            }

            statements.Add(Expression.Return(label, instanceVariable));
            statements.Add(Expression.Label(label, instanceVariable));

            return Expression.Block(variables, statements);
        }

        protected virtual
            Func<IServiceResolver, IServiceLifeScope, object>
            CreateActivatorCore(ServiceCallDependency callDependency)
        {
            var lambdaBody = BuildExpression(callDependency);
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
                .Lambda(lambdaBody, ResolverParameter, ScopeParameter)
                .Compile();
        }

        protected Expression BuildExpression(ServiceCallDependency callDependency)
        {
            if (callDependency.Constructor == null)
            {
                return BuildWithImplFactory(callDependency);
            }

            Expression instance;

            if (callDependency is ServiceManyCallDependency enumerable)
            {
                instance = BuildWithEnumerable(enumerable);
            }
            else
            {
                instance = BuildWithConstructor(callDependency);
            }

            if (instance == null)
            {
                return null;
            }

            if (callDependency.Properties == null || callDependency.Properties.Count == 0)
            {
                return WrapperWithLifeStyle(instance, callDependency);
            }

            var wInstance = BuildPropertyInfo(instance, callDependency);
            if (wInstance == null)
            {
                return null;
            }

            return WrapperWithLifeStyle(wInstance, callDependency);
        }

        protected virtual Expression BuildWithImplFactory(ServiceCallDependency callDependency)
        {
            return
                Expression.Invoke(
                    ApplyLifeStyleFuncConstant,
                    ScopeParameter,
                    Expression.Constant(callDependency.Context.ServiceType),
                    Expression.Constant(callDependency.Context.LifeStyle),
                    Expression.Constant(callDependency.Context.GetImplementionFactory())
                );
        }

        protected virtual Expression BuildWithConstructor(ServiceCallDependency callDependency)
        {
            var parameterValues = new Expression[callDependency.Parameters?.Count ?? 0];

            for (var i = 0; i < parameterValues.Length; i++)
            {
                if (callDependency.Parameters != null)
                {
                    var parameterValue = BuildExpression(callDependency.Parameters[callDependency.Constructor.Paramters[i]]);
                    if (!callDependency.Constructor.Paramters[i].ParameterType.IsAssignableFrom(parameterValue.Type))
                    {
                        parameterValue = Expression.Convert(parameterValue, callDependency.Constructor.Paramters[i].ParameterType);
                    }

                    parameterValues[i] = parameterValue;
                }
            }

            return Expression.New(callDependency.Constructor.ConstructorInfo, parameterValues);
        }

        protected virtual Expression BuildWithEnumerable(ServiceManyCallDependency callDependency)
        {
            var elementInits = new ElementInit[callDependency.Elements.Length];
            var addElement = callDependency.Context.GetImplementionType().GetMethod("Add");

            for (var i = 0; i < elementInits.Length; i++)
            {
                elementInits[i] = Expression.ElementInit(
                    addElement,
                    Expression.Convert(
                        BuildExpression(callDependency.Elements[i]),
                        callDependency.Elements[i].Context.ServiceType
                    )
                );
            }

            return Expression.ListInit(Expression.New(callDependency.Constructor.ConstructorInfo), elementInits);
        }

        protected virtual Expression WrapperWithLifeStyle(Expression instance, ServiceCallDependency callDependency)
        {
            if (!callDependency.ShouldHoldServiceLife())
            {
                return instance;
            }

            //optimization
            var preCompiledFunc = (Func<IServiceResolver, IServiceLifeScope, object>)
                Expression.Lambda(instance, ResolverParameter, ScopeParameter).Compile();

            var factory = (Func<IServiceResolver, object>)
            (
                resolver => preCompiledFunc(resolver, resolver.ServiceLifeScope)
            );

            return
                Expression.Invoke(
                    ApplyLifeStyleFuncConstant,
                    ScopeParameter,
                    Expression.Constant(callDependency.Context.ServiceType),
                    Expression.Constant(callDependency.Context.LifeStyle),
                    Expression.Constant(factory)
                );
        }
    }
}
