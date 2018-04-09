using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using Tinja.LifeStyle;
using Tinja.Resolving.Chain.Node;
using Tinja.Resolving.Context;

namespace Tinja.Resolving.Activation
{
    public class ServiceActivationBuilder : IServiceActivationBuilder
    {
        public static ConcurrentDictionary<Type, Func<IServiceResolver, IServiceLifeStyleScope, object>> Cache { get; }

        static ServiceActivationBuilder()
        {
            Cache = new ConcurrentDictionary<Type, Func<IServiceResolver, IServiceLifeStyleScope, object>>();
        }

        public Func<IServiceResolver, IServiceLifeStyleScope, object> Build(IServiceChainNode chain)
        {
            return Cache.GetOrAdd(chain.ResolvingContext.ReslovingType, (k) => BuildFactory(chain, new HashSet<IServiceChainNode>()));
        }

        public Func<IServiceResolver, IServiceLifeStyleScope, object> Build(Type resolvingType)
        {
            if (Cache.TryGetValue(resolvingType, out var factory))
            {
                return factory;
            }

            return null;
        }

        public static Func<IServiceResolver, IServiceLifeStyleScope, object> BuildFactory(IServiceChainNode node, HashSet<IServiceChainNode> injectedProperties)
        {
            return new ServiceActivatorFacotry().CreateActivator(node);
        }

        private class ServiceActivatorFacotry
        {
            static ParameterExpression ParameterContainer { get; }

            static ParameterExpression ParameterLifeScope { get; }

            private Dictionary<IResolvingContext, HashSet<IResolvingContext>> _resolvedPropertyTypes;

            static ServiceActivatorFacotry()
            {
                ParameterContainer = Expression.Parameter(typeof(IServiceResolver));
                ParameterLifeScope = Expression.Parameter(typeof(IServiceLifeStyleScope));
            }

            public ServiceActivatorFacotry()
            {
                _resolvedPropertyTypes = new Dictionary<IResolvingContext, HashSet<IResolvingContext>>();
            }

            public Func<IServiceResolver, IServiceLifeStyleScope, object> CreateActivator(IServiceChainNode node)
            {
                var lambdaBody = BuildExpression(node);
                if (lambdaBody == null)
                {
                    throw new NullReferenceException(nameof(lambdaBody));
                }

                var factory = (Func<IServiceResolver, IServiceLifeStyleScope, object>)Expression
                       .Lambda(lambdaBody, ParameterContainer, ParameterLifeScope)
                       .Compile();

                if (node.ResolvingContext.Component.LifeStyle != ServiceLifeStyle.Transient ||
                    node.ResolvingContext.Component.ImplementionType.Is(typeof(IDisposable)))
                {
                    return BuildProperty(
                        (o, scoped) =>
                            scoped.ApplyInstanceLifeStyle(node.ResolvingContext, (_) => factory(o, scoped)),
                        node
                    );
                }

                return BuildProperty(factory, node);
            }

            public Expression BuildExpression(IServiceChainNode serviceNode)
            {
                if (serviceNode.Constructor == null)
                {
                    return BuildImplFactory(serviceNode);
                }

                if (serviceNode is ServiceEnumerableChainNode enumerable)
                {
                    return BuildEnumerable(enumerable);
                }
                else
                {
                    return BuildConstructor(serviceNode as ServiceConstrutorChainNode);
                }
            }

            public Expression BuildImplFactory(IServiceChainNode node)
            {
                return
                    Expression.Invoke(
                        Expression.Constant(node.ResolvingContext.Component.ImplementionFactory),
                        ParameterContainer
                    );
            }

            public NewExpression BuildConstructor(ServiceConstrutorChainNode node)
            {
                var parameterValues = new Expression[node.Paramters?.Count ?? 0];

                for (var i = 0; i < parameterValues.Length; i++)
                {
                    var parameterValueFactory = CreateActivator(node.Paramters[node.Constructor.Paramters[i]]);
                    if (parameterValueFactory == null)
                    {
                        parameterValues[i] = Expression.Constant(null, node.Constructor.Paramters[i].ParameterType);
                    }
                    else
                    {
                        parameterValues[i] = Expression.Convert(
                            Expression.Invoke(
                                Expression.Constant(parameterValueFactory),
                                ParameterContainer,
                                ParameterLifeScope
                            ),
                            node.Constructor.Paramters[i].ParameterType
                        );
                    }
                }

                return Expression.New(node.Constructor.ConstructorInfo, parameterValues);
            }

            public ListInitExpression BuildEnumerable(ServiceEnumerableChainNode node)
            {
                var newExpression = BuildConstructor(node);
                var elementInits = new List<ElementInit>();
                var addElement = node.ResolvingContext.Component.ImplementionType.GetMethod("Add");

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
                                    ParameterContainer,
                                    ParameterLifeScope
                                ),
                                node.Elements[i].ResolvingContext.ReslovingType
                            )
                        )
                    );
                }

                return Expression.ListInit(newExpression, elementInits.ToArray());
            }

            public Expression BuildPropertyInfo(Expression instance, IServiceChainNode node)
            {
                instance = Expression.Convert(instance, node.Constructor.ConstructorInfo.DeclaringType);

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
                                ParameterContainer,
                                ParameterLifeScope
                            ),
                            item.Value.ResolvingContext.ReslovingType
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

            public Func<IServiceResolver, IServiceLifeStyleScope, object> BuildProperty(Func<IServiceResolver, IServiceLifeStyleScope, object> factory, IServiceChainNode node)
            {
                if (node.Properties != null && node.Properties.Count != 0)
                {
                    var lambdaBody = BuildPropertyInfo(
                        Expression.Invoke(
                            Expression.Constant(factory),
                            ParameterContainer,
                            ParameterLifeScope
                        ),
                        node);

                    return (Func<IServiceResolver, IServiceLifeStyleScope, object>)Expression
                       .Lambda(lambdaBody, ParameterContainer, ParameterLifeScope)
                       .Compile();
                }

                return factory;
            }

            public bool IsPropertyCircularDependeny(IServiceChainNode instance, IServiceChainNode propertyNode)
            {
                if (!_resolvedPropertyTypes.ContainsKey(instance.ResolvingContext))
                {
                    _resolvedPropertyTypes[instance.ResolvingContext] = new HashSet<IResolvingContext>()
                {
                   propertyNode.ResolvingContext
                };

                    return false;
                }

                var properties = _resolvedPropertyTypes[instance.ResolvingContext];
                if (properties.Contains(propertyNode.ResolvingContext))
                {
                    return true;
                }

                properties.Add(propertyNode.ResolvingContext);

                return false;
            }
        }
    }
}

