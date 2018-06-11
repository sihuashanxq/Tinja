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
        delegate object ApplyLifeStyleDelegate(
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

        public Func<IServiceResolver, IServiceLifeScope, object> Create(ServiceCallDependency chain)
        {
            var factory = CreateActivatorCore(chain);
            if (factory == null)
            {
                return (resolver, scope) => null;
            }

            return factory;
        }

        public Expression BuildPropertyInfo(Expression instance, ServiceCallDependency node)
        {
            if (instance.Type != node.Constructor.ConstructorInfo.DeclaringType)
            {
                instance = Expression.Convert(instance, node.Constructor.ConstructorInfo.DeclaringType);
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

            foreach (var item in node.Properties)
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
            CreateActivatorCore(ServiceCallDependency chain)
        {
            var lambdaBody = BuildExpression(chain);
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

        protected Expression BuildExpression(ServiceCallDependency chain)
        {
            if (chain.Constructor == null)
            {
                return BuildWithImplFactory(chain);
            }

            Expression instance;

            if (chain is ServiceManyCallDependency enumerable)
            {
                instance = BuildWithEnumerable(enumerable);
            }
            else
            {
                instance = BuildWithConstructor(chain);
            }

            if (instance == null)
            {
                return null;
            }

            if (chain.Properties == null || chain.Properties.Count == 0)
            {
                return WrapperWithLifeStyle(instance, chain);
            }

            var wInstance = BuildPropertyInfo(instance, chain);
            if (wInstance == null)
            {
                return null;
            }

            return WrapperWithLifeStyle(wInstance, chain);
        }

        protected virtual Expression BuildWithImplFactory(ServiceCallDependency chain)
        {
            return
                Expression.Invoke(
                    ApplyLifeStyleFuncConstant,
                    ScopeParameter,
                    Expression.Constant(chain.Context.ServiceType),
                    Expression.Constant(chain.Context.LifeStyle),
                    Expression.Constant(chain.Context.GetImplementionFactory())
                );
        }

        protected virtual Expression BuildWithConstructor(ServiceCallDependency node)
        {
            var parameterValues = new Expression[node.Parameters?.Count ?? 0];

            for (var i = 0; i < parameterValues.Length; i++)
            {
                if (node.Parameters != null)
                {
                    var parameterValue = BuildExpression(node.Parameters[node.Constructor.Paramters[i]]);
                    if (!node.Constructor.Paramters[i].ParameterType.IsAssignableFrom(parameterValue.Type))
                    {
                        parameterValue = Expression.Convert(parameterValue, node.Constructor.Paramters[i].ParameterType);
                    }

                    parameterValues[i] = parameterValue;
                }
            }

            return Expression.New(node.Constructor.ConstructorInfo, parameterValues);
        }

        protected virtual Expression BuildWithEnumerable(ServiceManyCallDependency node)
        {
            var elementInits = new ElementInit[node.Elements.Length];
            var addElement = node.Context.GetImplementionType().GetMethod("Add");

            for (var i = 0; i < elementInits.Length; i++)
            {
                elementInits[i] = Expression.ElementInit(
                    addElement,
                    Expression.Convert(
                        BuildExpression(node.Elements[i]),
                        node.Elements[i].Context.ServiceType
                    )
                );
            }

            return Expression.ListInit(Expression.New(node.Constructor.ConstructorInfo), elementInits);
        }

        protected virtual Expression WrapperWithLifeStyle(Expression instance, ServiceCallDependency chain)
        {
            if (!chain.ShouldHoldServiceLife())
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
                    Expression.Constant(chain.Context.ServiceType),
                    Expression.Constant(chain.Context.LifeStyle),
                    Expression.Constant(factory)
                );
        }
    }
}
