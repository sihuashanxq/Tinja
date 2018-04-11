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
            return new ServiceActivatorFacotry().CreateActivator(node);
        }

        private class ServiceActivatorFacotry
        {
            static ParameterExpression ParameterResolver { get; }

            static ParameterExpression ParameterLifeScope { get; }

            private Dictionary<IResolvingContext, HashSet<IResolvingContext>> _handledProperties;

            static ServiceActivatorFacotry()
            {
                ParameterResolver = Expression.Parameter(typeof(IServiceResolver));
                ParameterLifeScope = Expression.Parameter(typeof(IServiceLifeStyleScope));
            }

            public ServiceActivatorFacotry()
            {
                _handledProperties = new Dictionary<IResolvingContext, HashSet<IResolvingContext>>();
            }

            public Func<IServiceResolver, IServiceLifeStyleScope, object> CreateActivator(ServiceDependChain node)
            {
                var lambdaBody = BuildExpression(node);
                if (lambdaBody == null)
                {
                    throw new NullReferenceException(nameof(lambdaBody));
                }

                var factory = (Func<IServiceResolver, IServiceLifeStyleScope, object>)Expression
                       .Lambda(lambdaBody, ParameterResolver, ParameterLifeScope)
                       .Compile();

                if (node.Context.Component.LifeStyle != ServiceLifeStyle.Transient ||
                    node.Context.Component.ImplementionType.Is(typeof(IDisposable)))
                {
                    //return (o, scoped) =>
                    //        scoped.ApplyInstanceLifeStyle(node.Context, (_) => factory(_, scoped));
                    return BuildProperty(
                        (o, scoped) =>
                            scoped.ApplyInstanceLifeStyle(node.Context, (_) => factory(_, scoped)),
                        node
                    );
                }

                return BuildProperty(factory, node);
            }

            public Expression BuildExpression(ServiceDependChain serviceNode)
            {
                if (serviceNode.Constructor == null)
                {
                    return BuildImplFactory(serviceNode);
                }

                if (serviceNode is ServiceEnumerableDependChain enumerable)
                {
                    return BuildEnumerable(enumerable);
                }
                else
                {
                    return BuildConstructor(serviceNode as ServiceDependChain);
                }
            }

            public Expression BuildImplFactory(ServiceDependChain node)
            {
                return
                    Expression.Invoke(
                        Expression.Constant(node.Context.Component.ImplementionFactory),
                        ParameterResolver
                    );
            }

            public Expression BuildConstructor(ServiceDependChain node)
            {
                var parameterValues = new Expression[node.Parameters?.Count ?? 0];

                for (var i = 0; i < parameterValues.Length; i++)
                {
                    var parameterValueFactory = CreateActivator(node.Parameters[node.Constructor.Paramters[i]]);
                    if (parameterValueFactory == null)
                    {
                        parameterValues[i] = Expression.Constant(null, node.Constructor.Paramters[i].ParameterType);
                    }
                    else
                    {
                        parameterValues[i] = Expression.Convert(
                            Expression.Invoke(
                                Expression.Constant(parameterValueFactory),
                                ParameterResolver,
                                ParameterLifeScope
                            ),
                            node.Constructor.Paramters[i].ParameterType
                        );
                    }
                }

                return Expression.New(node.Constructor.ConstructorInfo, parameterValues);
            }

            public Expression BuildEnumerable(ServiceEnumerableDependChain node)
            {
                var elementInits = new List<ElementInit>();
                var addElement = node.Context.Component.ImplementionType.GetMethod("Add");

                for (var i = 0; i < node.Elements.Length; i++)
                {
                    if (node.Elements[i] == null)
                    {
                        continue;
                    }

                    var elementValueFactory = CreateActivator(node.Elements[i]);
                    if (elementValueFactory == null)
                    {
                        continue;
                    }

                    elementInits.Add(
                        Expression.ElementInit(
                            addElement,
                            Expression.Convert(
                                Expression.Invoke(
                                    Expression.Constant(elementValueFactory),
                                    ParameterResolver,
                                    ParameterLifeScope
                                ),
                                node.Elements[i].Context.ServiceType
                            )
                        )
                    );
                }

                return Expression.ListInit(
                    Expression.New(node.Constructor.ConstructorInfo),
                    elementInits.ToArray()
                );
            }

            public Expression BuildPropertyInfo(Expression instance, ServiceDependChain node)
            {
                if (instance.Type != node.Constructor.ConstructorInfo.DeclaringType)
                {
                    instance = Expression.Convert(instance, node.Constructor.ConstructorInfo.DeclaringType);
                }

                var vars = new List<ParameterExpression>();
                var statements = new List<Expression>();
                var instanceVar = Expression.Parameter(instance.Type);
                var assignInstance = Expression.Assign(instanceVar, instance);

                var label = Expression.Label(instanceVar.Type);

                vars.Add(instanceVar);
                statements.Add(assignInstance);
                statements.Add(
                    Expression.IfThen(
                        Expression.Equal(Expression.Constant(null), instanceVar),
                        Expression.Return(label, instanceVar)
                    )
                );

                foreach (var item in node.Properties)
                {
                    if (IsPropertyCircularDependeny(node, item.Value))
                    {
                        continue;
                    }

                    var property = Expression.MakeMemberAccess(instanceVar, item.Key);
                    var propertyVar = Expression.Variable(item.Key.PropertyType, item.Key.Name);

                    var propertyValue = Expression.Convert(
                            Expression.Invoke(
                                Expression.Constant(CreateActivator(item.Value)),
                                ParameterResolver,
                                ParameterLifeScope
                            ),
                            item.Value.Context.ServiceType
                        );

                    var setPropertyVarValue = Expression.Assign(propertyVar, propertyValue);
                    var setPropertyValue = Expression.IfThen(
                        Expression.NotEqual(Expression.Constant(null), propertyVar),
                        Expression.Assign(property, propertyVar)
                    );

                    vars.Add(propertyVar);
                    statements.Add(setPropertyVarValue);
                    statements.Add(setPropertyValue);
                }

                statements.Add(Expression.Return(label, instanceVar));
                statements.Add(Expression.Label(label, instanceVar));

                return Expression.Block(vars, statements);
            }

            public Func<IServiceResolver, IServiceLifeStyleScope, object> BuildProperty(Func<IServiceResolver, IServiceLifeStyleScope, object> factory, ServiceDependChain node)
            {
                if (node.Properties != null && node.Properties.Count != 0)
                {
                    var lambdaBody = BuildPropertyInfo(
                        Expression.Invoke(
                            Expression.Constant(factory),
                            ParameterResolver,
                            ParameterLifeScope
                        ),
                        node);

                    return (Func<IServiceResolver, IServiceLifeStyleScope, object>)Expression
                       .Lambda(lambdaBody, ParameterResolver, ParameterLifeScope)
                       .Compile();
                }

                return factory;
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
}

