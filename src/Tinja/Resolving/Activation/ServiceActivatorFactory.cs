using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Tinja.ServiceLife;
using Tinja.Resolving;
using Tinja.Resolving.Dependency;

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

        static ParameterExpression ScopeParameter { get; }

        static ParameterExpression ResolverParameter { get; }

        static ConstantExpression ApplyLifeStyleFuncConstant { get; }

        static ApplyLifeStyleDelegate ApplyLifeStyleFunc { get; }

        static ServiceActivatorFactory()
        {
            ScopeParameter = Expression.Parameter(typeof(IServiceLifeScope));
            ResolverParameter = Expression.Parameter(typeof(IServiceResolver));

            ApplyLifeStyleFunc = (scope, serviceType, lifeStyle, factory) => scope.ApplyServiceLifeStyle(serviceType, lifeStyle, factory);
            ApplyLifeStyleFuncConstant = Expression.Constant(ApplyLifeStyleFunc, typeof(ApplyLifeStyleDelegate));
        }

        public ServiceActivatorFactory()
        {
            _resolvedProperties = new Dictionary<IServiceResolvingContext, HashSet<IServiceResolvingContext>>();
        }

        public Func<IServiceResolver, IServiceLifeScope, object> Create(ServiceDependencyChain chain)
        {
            var factory = CreateActivatorCore(chain);
            if (factory == null)
            {
                return (resolver, scope) => null;
            }

            return factory;
        }

        public Expression BuildPropertyInfo(Expression instance, ServiceDependencyChain node)
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
            CreateActivatorCore(ServiceDependencyChain chain)
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

        protected Expression BuildExpression(ServiceDependencyChain chain)
        {
            if (chain.Constructor == null)
            {
                return BuildWithImplFactory(chain);
            }

            var instance = null as Expression;
            if (chain is ServiceDependencyEnumerableChain enumerable)
            {
                instance = BuildWithEnumerable(enumerable);
            }
            else
            {
                instance = BuildWithConstructor(chain as ServiceDependencyChain);
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
                return wInstance;
            }

            return WrapperWithLifeStyle(wInstance, chain);
        }

        protected virtual Expression BuildWithImplFactory(ServiceDependencyChain chain)
        {
            return
                Expression.Invoke(
                    ApplyLifeStyleFuncConstant,
                    ScopeParameter,
                    Expression.Constant(chain.Context.ServiceType),
                    Expression.Constant(chain.Context.Component.LifeStyle),
                    Expression.Constant(chain.Context.Component.ImplementionFactory)
                );
        }

        protected virtual Expression BuildWithConstructor(ServiceDependencyChain node)
        {
            var parameterValues = new Expression[node.Parameters?.Count ?? 0];
            var varible = Expression.Variable(node.Constructor.ConstructorInfo.DeclaringType);
            var statements = new List<Expression>();
            var lable = Expression.Label(varible.Type);

            for (var i = 0; i < parameterValues.Length; i++)
            {
                var parameterValue = BuildExpression(node.Parameters[node.Constructor.Paramters[i]]);
                if (!node.Constructor.Paramters[i].ParameterType.IsAssignableFrom(parameterValue.Type))
                {
                    parameterValue = Expression.Convert(parameterValue, node.Constructor.Paramters[i].ParameterType);
                }

                parameterValues[i] = parameterValue;
            }

            return Expression.New(node.Constructor.ConstructorInfo, parameterValues);
        }

        protected virtual Expression BuildWithEnumerable(ServiceDependencyEnumerableChain node)
        {
            var elementInits = new ElementInit[node.Elements.Length];
            var addElement = node.Context.Component.ImplementionType.GetMethod("Add");

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

        protected virtual Expression WrapperWithLifeStyle(Expression instance, ServiceDependencyChain chain)
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
                    Expression.Constant(chain.Context.Component.LifeStyle),
                    Expression.Constant(factory)
                );
        }

        private Dictionary<IServiceResolvingContext, HashSet<IServiceResolvingContext>> _resolvedProperties;
    }
}
