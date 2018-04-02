using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Tinja.Resolving.ReslovingContext;

namespace Tinja.Resolving.Builder
{
    public class ServiceFactoryBuilder : IServiceFactoryBuilder
    {
        public ConcurrentDictionary<Type, Func<IContainer, ILifeStyleScope, object>> FactoryCache { get; }

        static ParameterExpression ParameterContainer { get; }

        static ParameterExpression ParameterLifeScope { get; }

        static ServiceFactoryBuilder()
        {
            ParameterContainer = Expression.Parameter(typeof(IContainer));
            ParameterLifeScope = Expression.Parameter(typeof(ILifeStyleScope));
        }

        public ServiceFactoryBuilder()
        {
            FactoryCache = new ConcurrentDictionary<Type, Func<IContainer, ILifeStyleScope, object>>();
        }

        public Func<IContainer, ILifeStyleScope, object> Build(IServiceNode serviceNode)
        {
            return FactoryCache.GetOrAdd(serviceNode.ResolvingContext.ReslovingType, (k) => BuildFactory(serviceNode));
        }

        public Func<IContainer, ILifeStyleScope, object> Build(Type resolvingType)
        {
            if (FactoryCache.TryGetValue(resolvingType, out var factory))
            {
                return factory;
            }

            return null;
        }

        public Func<IContainer, ILifeStyleScope, object> BuildFactory(IServiceNode serviceNode)
        {
            var lambdaBody = BuildExpression(serviceNode, new Dictionary<IServiceNode, Expression>());
            if (lambdaBody == null)
            {
                throw new NullReferenceException(nameof(lambdaBody));
            }

            return (Func<IContainer, ILifeStyleScope, object>)Expression
                   .Lambda(lambdaBody, ParameterContainer, ParameterLifeScope)
                   .Compile();
        }

        public Expression BuildExpression(IServiceNode serviceNode, Dictionary<IServiceNode, Expression> propertyInjectedNodes)
        {
            if (serviceNode.Constructor == null)
            {
                return BuildLifeScope(BuildImplFactory(serviceNode), serviceNode);
            }

            Expression instance;

            if (serviceNode is ServiceEnumerableNode enumerable)
            {
                instance = BuildEnumerable(enumerable, propertyInjectedNodes);
                instance = BuildLifeScope(instance, serviceNode);
            }
            else
            {
                instance = BuildConstructor(serviceNode as ServiceConstrutorNode, propertyInjectedNodes);
                instance = BuildLifeScope(instance, serviceNode);
            }

            if (serviceNode.Properties == null || serviceNode.Properties.Count == 0)
            {
                return instance;
            }

            if (!propertyInjectedNodes.ContainsKey(serviceNode))
            {
                instance = BuildPropertyInfo(instance, serviceNode, propertyInjectedNodes);
            }

            return instance;
        }

        public Expression BuildLifeScope(Expression instance, IServiceNode node)
        {
            if (node.ResolvingContext.Component.LifeStyle != LifeStyle.Transient ||
                node.ResolvingContext.Component.ImplementionType.Is(typeof(IDisposable)))
            {
                var lambda =
                  Expression.Lambda(
                      Expression.Call(
                          ParameterLifeScope,
                          typeof(ILifeStyleScope).GetMethod("GetOrAddLifeScopeInstance"),
                          Expression.Constant(node.ResolvingContext),
                          Expression.Lambda(
                              instance is LambdaExpression
                              ? Expression.Invoke(
                                    Expression.Constant(node.ResolvingContext.Component.ImplementionFactory),
                                    ParameterContainer
                                )
                              : instance,
                              Expression.Parameter(typeof(IResolvingContext))
                        )
                    ),
                    ParameterContainer,
                    ParameterLifeScope
                );

                return Expression.Invoke(lambda, ParameterContainer, ParameterLifeScope);
            }

            return instance;
        }

        public Expression BuildImplFactory(IServiceNode node)
        {
            return
                Expression.Lambda(
                    Expression.Invoke(
                        Expression.Constant(node.ResolvingContext.Component.ImplementionFactory),
                        ParameterContainer
                    ),
                    Expression.Parameter(typeof(IContainer))
                );
        }

        public NewExpression BuildConstructor(ServiceConstrutorNode node, Dictionary<IServiceNode, Expression> propertyInjectedNodes)
        {
            var parameterValues = new Expression[node.Paramters?.Count ?? 0];

            for (var i = 0; i < parameterValues.Length; i++)
            {
                var parameterValue = BuildExpression(node.Paramters[node.Constructor.Paramters[i]], propertyInjectedNodes);
                if (parameterValue is LambdaExpression)
                {
                    parameterValues[i] = Expression.Convert(
                        Expression.Invoke(parameterValue, ParameterContainer, ParameterLifeScope),
                        node.Constructor.Paramters[i].ParameterType
                    );
                }
                else
                {
                    parameterValues[i] = parameterValue;
                }
            }

            return Expression.New(node.Constructor.ConstructorInfo, parameterValues);
        }

        public ListInitExpression BuildEnumerable(ServiceEnumerableNode node, Dictionary<IServiceNode, Expression> propertyInjectedNodes)
        {
            var newExpression = BuildConstructor(node, propertyInjectedNodes);
            var elementInits = new ElementInit[node.Elements.Length];
            var addElement = node.ResolvingContext.Component.ImplementionType.GetMethod("Add");

            for (var i = 0; i < elementInits.Length; i++)
            {
                elementInits[i] = Expression.ElementInit(
                    addElement,
                    Expression.Convert(
                        BuildExpression(node.Elements[i], propertyInjectedNodes),
                        node.Elements[i].ResolvingContext.ReslovingType
                    )
                );
            }

            return Expression.ListInit(newExpression, elementInits);
        }

        public Expression BuildPropertyInfo(Expression instance, IServiceNode node, Dictionary<IServiceNode, Expression> propertyInjectedNodes)
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
                if (propertyInjectedNodes.ContainsKey(item.Value))
                {
                    continue;
                }

                propertyInjectedNodes[item.Value] = null;

                var property = Expression.MakeMemberAccess(instanceVar, item.Key);
                var propertyVar = Expression.Variable(item.Key.PropertyType, item.Key.Name);

                var propertyValue = Expression.Convert(
                    BuildExpression(item.Value, propertyInjectedNodes),
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
    }
}

