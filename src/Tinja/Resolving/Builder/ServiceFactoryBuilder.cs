using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

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
            var lambdaBody = BuildExpression(serviceNode);
            if (lambdaBody == null)
            {
                throw new NullReferenceException(nameof(lambdaBody));
            }

            var factory = (Func<IContainer, ILifeStyleScope, object>)Expression
                .Lambda(lambdaBody, ParameterContainer, ParameterLifeScope)
                .Compile();

            if (serviceNode.ResolvingContext.Component.LifeStyle != LifeStyle.Transient ||
                serviceNode.ResolvingContext.Component.ImplementionType.Is(typeof(IDisposable)))
            {
                return (o, scope) =>
                scope.GetOrAddLifeScopeInstance(serviceNode.ResolvingContext, (_) => factory(o, scope));
            }

            return factory;
        }

        public Expression BuildExpression(IServiceNode serviceNode)
        {
            Expression instance;

            if (serviceNode.Constructor == null)
            {
                instance = BuildImplFactory(serviceNode);
            }
            else if (serviceNode is ServiceEnumerableNode enumerable)
            {
                instance = BuildEnumerable(enumerable);
            }
            else
            {
                instance = BuildConstructor(serviceNode as ServiceConstrutorNode);
            }

            if (serviceNode.Properties == null || serviceNode.Properties.Count == 0)
            {
                return instance;
            }

            return BuildPropertyInfo(instance, serviceNode);
        }

        public Expression BuildImplFactory(IServiceNode node)
        {
            return
                Expression.Lambda(
                    Expression.Call(
                        typeof(ILifeStyleScope).GetMethod("GetOrAddLifeScopeInstance"),
                        ParameterLifeScope,
                        Expression.Constant(node.ResolvingContext),
                        Expression.Lambda(
                            Expression.Invoke(
                                Expression.Constant(node.ResolvingContext.Component.ImplementionFactory),
                                ParameterContainer
                            ),
                            Expression.Parameter(typeof(IContainer))
                    )
                ),
                ParameterContainer,
                ParameterLifeScope
            );
        }

        public NewExpression BuildConstructor(ServiceConstrutorNode node)
        {
            var parameterValues = new Expression[node.Paramters?.Count ?? 0];

            for (var i = 0; i < parameterValues.Length; i++)
            {
                var parameterValue = BuildExpression(node.Paramters[node.Constructor.Paramters[i]]);
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

        public ListInitExpression BuildEnumerable(ServiceEnumerableNode node)
        {
            var newExpression = BuildConstructor(node);
            var elementInits = new ElementInit[node.Elements.Length];
            var addElement = node.ResolvingContext.Component.ImplementionType.GetMethod("Add");

            for (var i = 0; i < elementInits.Length; i++)
            {
                elementInits[i] = Expression.ElementInit(
                    addElement,
                    Expression.Convert(
                        BuildExpression(node.Elements[i]),
                        node.Elements[i].ResolvingContext.ReslovingType
                    )
                );
            }

            return Expression.ListInit(newExpression, elementInits);
        }

        public Expression BuildPropertyInfo(Expression instance, IServiceNode node)
        {
            var vars = new List<ParameterExpression>();
            var statements = new List<Expression>();
            var instanceVar = Expression.Variable(instance.Type);
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
                var property = Expression.MakeMemberAccess(instanceVar, item.Key);
                var propertyVar = Expression.Variable(item.Key.PropertyType, item.Key.Name);

                var propertyValue = BuildExpression(item.Value);
                if (propertyValue is LambdaExpression)
                {
                    propertyValue = Expression.Invoke(propertyValue, ParameterContainer, ParameterLifeScope);
                }

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

