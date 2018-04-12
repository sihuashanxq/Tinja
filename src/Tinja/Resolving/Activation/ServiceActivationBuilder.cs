using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using Tinja.LifeStyle;
using Tinja.Resolving.Context;
using Tinja.Resolving.Dependency;

namespace Tinja.Resolving.Activation
{
    public class ServiceActivationBuilder : IServiceActivationBuilder
    {
        public static ConcurrentDictionary<Type, Func<IServiceResolver, IServiceLifeStyleScope, object>> Cache { get; }

        static ServiceActivationBuilder()
        {
            Cache = new ConcurrentDictionary<Type, Func<IServiceResolver, IServiceLifeStyleScope, object>>();
        }

        public Func<IServiceResolver, IServiceLifeStyleScope, object> Build(ServiceDependChain chain)
        {
            return Cache.GetOrAdd(chain.Context.ServiceType, (k) => BuildFactory(chain, new HashSet<ServiceDependChain>()));
        }

        public Func<IServiceResolver, IServiceLifeStyleScope, object> Build(Type resolvingType)
        {
            if (Cache.TryGetValue(resolvingType, out var factory))
            {
                return factory;
            }

            return null;
        }

        public static Func<IServiceResolver, IServiceLifeStyleScope, object> BuildFactory(ServiceDependChain node, HashSet<ServiceDependChain> injectedProperties)
        {
            return new ServiceActivationFactory().CreateActivator(node);
        }
    }

    public class ServiceActivationFactory
    {
        delegate object ApplyLifeStyleDelegate(IServiceLifeStyleScope scope, IResolvingContext context, Func<IServiceResolver, object> factory);

        static ParameterExpression ScopeParameter { get; }

        static ParameterExpression ResolverParameter { get; }

        static ApplyLifeStyleDelegate ApplyLifeStyleFunc { get; }

        static ConstantExpression ApplyLifeStyleFuncConstant { get; }

        private Dictionary<IResolvingContext, HashSet<IResolvingContext>> _handledProperties;

        static ServiceActivationFactory()
        {
            ScopeParameter = Expression.Parameter(typeof(IServiceLifeStyleScope));
            ResolverParameter = Expression.Parameter(typeof(IServiceResolver));

            ApplyLifeStyleFunc = (scope, context, factory)
                => scope.ApplyServiceLifeStyle(context, factory);

            ApplyLifeStyleFuncConstant = Expression.Constant(ApplyLifeStyleFunc, typeof(ApplyLifeStyleDelegate));
        }

        public ServiceActivationFactory()
        {
            _handledProperties = new Dictionary<IResolvingContext, HashSet<IResolvingContext>>();
        }

        public Func<IServiceResolver, IServiceLifeStyleScope, object> CreateActivator(ServiceDependChain chain)
        {
            var lambdaBody = BuildExpression(chain);
            if (lambdaBody == null)
            {
                throw new NullReferenceException(nameof(lambdaBody));
            }

            return (Func<IServiceResolver, IServiceLifeStyleScope, object>)Expression
                .Lambda(lambdaBody, ResolverParameter, ScopeParameter)
                .Compile();
        }

        public Expression BuildExpression(ServiceDependChain chain)
        {
            if (chain.Constructor == null)
            {
                return BuildImplFactory(chain);
            }

            if (chain is ServiceEnumerableDependChain enumerable)
            {
                return BuildEnumerable(enumerable);
            }

            var instance = BuildConstructor(chain as ServiceDependChain);
            if (instance == null)
            {
                return instance;
            }

            instance = ApplyServiceLifeStyle(instance, chain);
            instance = Expression.Convert(
                instance,
                chain.Constructor?.ConstructorInfo?.DeclaringType ??
                chain.Context.ServiceType
            );

            if (chain.Properties == null || chain.Properties.Count == 0)
            {
                return instance;
            }

            return BuildPropertyInfo(instance, chain);
        }

        protected virtual Expression ApplyServiceLifeStyle(Expression instance, ServiceDependChain chain)
        {
            if (chain.Context.Component.LifeStyle == ServiceLifeStyle.Transient &&
                chain.Constructor != null &&
                !chain.Constructor.ConstructorInfo.DeclaringType.Is(typeof(IDisposable)))
            {
                return instance;
            }

            //optimization
            var preCompiledFunc = (Func<IServiceResolver, IServiceLifeStyleScope, object>)
                Expression.Lambda(instance, ResolverParameter, ScopeParameter).Compile();

            var factory = (Func<IServiceResolver, object>)
                (
                    resolver => preCompiledFunc(resolver, resolver.Scope)
                );

            return
                Expression.Invoke(
                    Expression.Constant(ApplyLifeStyleFunc),
                    ScopeParameter,
                    Expression.Constant(chain.Context),
                    Expression.Constant(factory)
                );
        }

        public Expression BuildImplFactory(ServiceDependChain chain)
        {
            return
                ApplyServiceLifeStyle(
                    Expression.Invoke(
                        Expression.Constant(chain.Context.Component.ImplementionFactory),
                        ResolverParameter
                    ),
                    chain);
        }

        public Expression BuildConstructor(ServiceDependChain node)
        {
            var parameterValues = new Expression[node.Parameters?.Count ?? 0];

            for (var i = 0; i < parameterValues.Length; i++)
            {
                parameterValues[i] = BuildExpression(node.Parameters[node.Constructor.Paramters[i]]);
            }

            return Expression.New(node.Constructor.ConstructorInfo, parameterValues);
        }

        public Expression BuildEnumerable(ServiceEnumerableDependChain node)
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

            return Expression.ListInit(
                Expression.New(node.Constructor.ConstructorInfo),
                elementInits);
        }

        public Expression BuildPropertyInfo(Expression instance, ServiceDependChain node)
        {
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
                if (IsPropertyCircularDependeny(node, item.Value))
                {
                    continue;
                };

                var property = Expression.MakeMemberAccess(instanceVariable, item.Key);
                var propertyVariable = Expression.Variable(item.Key.PropertyType, item.Key.Name);

                var propertyValue = BuildExpression(item.Value);

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

        public bool IsPropertyCircularDependeny(ServiceDependChain instance, ServiceDependChain propertyNode)
        {
            if (!_handledProperties.ContainsKey(instance.Context))
            {
                _handledProperties[instance.Context] = new HashSet<IResolvingContext>()
                {
                    propertyNode.Context
                };

                return false;
            }

            var properties = _handledProperties[instance.Context];
            if (properties.Contains(propertyNode.Context))
            {
                return true;
            }

            properties.Add(propertyNode.Context);

            return false;
        }
    }
}

