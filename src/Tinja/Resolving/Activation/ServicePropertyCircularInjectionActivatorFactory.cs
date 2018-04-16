using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Tinja.LifeStyle;
using Tinja.Resolving.Context;
using Tinja.Resolving.Dependency;
namespace Tinja.Resolving.Activation
{
    public class ServicePropertyCircularInjectionActivatorFactory : IServiceInjectionActivatorFactory
    {
        delegate object ApplyLifeStyleDelegate(
           IServiceLifeStyleScope scope,
           IResolvingContext context,
           Dictionary<ServiceDependChain, object> services,
           Func<IServiceLifeStyleScope, IServiceResolver, Dictionary<ServiceDependChain, object>, object> factory
        );

        static ParameterExpression ScopeParameter { get; }

        static ParameterExpression ResolverParameter { get; }

        static ConstantExpression ApplyLifeStyleFuncConstant { get; }

        static ParameterExpression ResolvedServicesParameter { get; }

        static MethodInfo AddResolvedServiceMethod { get; }

        static ApplyLifeStyleDelegate ApplyLifeStyleFunc { get; }

        static Action<ServicePropertyCircularInjectionActivatorFactory, Dictionary<ServiceDependChain, object>, IServiceResolver> SetPropertyValueFunc { get; }

        private static ConcurrentDictionary<Type, Delegate> _propertySetters;

        private Dictionary<IResolvingContext, HashSet<IResolvingContext>> _resolvedProperties;

        static ServicePropertyCircularInjectionActivatorFactory()
        {
            ScopeParameter = Expression.Parameter(typeof(IServiceLifeStyleScope));
            ResolverParameter = Expression.Parameter(typeof(IServiceResolver));
            ResolvedServicesParameter = Expression.Parameter(typeof(Dictionary<ServiceDependChain, object>));

            SetPropertyValueFunc = (factory, services, resolver) => factory.SetPropertyValue(services, resolver);

            ApplyLifeStyleFunc = (scope, context, services, factory) => scope.ApplyServiceLifeStyle(context, resolver => factory(resolver.Scope, resolver, services));
            ApplyLifeStyleFuncConstant = Expression.Constant(ApplyLifeStyleFunc, typeof(ApplyLifeStyleDelegate));

            AddResolvedServiceMethod = typeof(ServicePropertyCircularInjectionActivatorFactory).GetMethod(nameof(AddResolvedService));

            _propertySetters = new ConcurrentDictionary<Type, Delegate>();
        }

        public ServicePropertyCircularInjectionActivatorFactory()
        {
            _resolvedProperties = new Dictionary<IResolvingContext, HashSet<IResolvingContext>>();
        }

        public Func<IServiceResolver, IServiceLifeStyleScope, object> CreateActivator(ServiceDependChain chain)
        {
            var factory = CreateActivatorCore(chain);
            if (factory == null)
            {
                return (resolver, scope) => null;
            }

            return (resolver, scope) =>
            {
                return factory(resolver, scope, new Dictionary<ServiceDependChain, object>());
            };
        }

        protected virtual
            Func<IServiceResolver, IServiceLifeStyleScope, Dictionary<ServiceDependChain, object>, object>
            CreateActivatorCore(ServiceDependChain chain)
        {
            var lambdaBody = BuildExpression(chain);
            if (lambdaBody == null)
            {
                throw new NullReferenceException(nameof(lambdaBody));
            }

            var varible = Expression.Variable(chain.Context.ServiceType);
            var lable = Expression.Label(varible.Type);

            var statements = new List<Expression>
            {
                Expression.Assign(varible, Expression.Convert(lambdaBody,varible.Type)),
                Expression.Invoke(
                    Expression.Constant(SetPropertyValueFunc),
                    Expression.Constant(this),
                    ResolvedServicesParameter,
                    ResolverParameter
                ),
                Expression.Return(lable,varible),
                Expression.Label(lable, varible)
            };

            return (Func<IServiceResolver, IServiceLifeStyleScope, Dictionary<ServiceDependChain, object>, object>)
                Expression
                .Lambda(
                    Expression.Block(new[] { varible }, statements),
                    ResolverParameter, ScopeParameter,
                    ResolvedServicesParameter)
                .Compile();
        }

        protected Expression BuildExpression(ServiceDependChain chain)
        {
            var func = null as Expression;
            if (chain.Constructor == null)
            {
                func = BuildWithImplFactory(chain);
            }
            else if (chain is ServiceEnumerableDependChain enumerable)
            {
                func = BuildWithEnumerable(enumerable);
            }
            else
            {
                func = BuildWithConstructor(chain as ServiceDependChain);
            }

            if (func == null)
            {
                return null;
            }

            return WrapperWithLifeStyle(func, chain);
        }

        protected virtual Expression BuildWithImplFactory(ServiceDependChain chain)
        {
            var factory = (Func<IServiceLifeStyleScope, IServiceResolver, Dictionary<ServiceDependChain, object>, object>)
                Expression
                .Lambda(
                    Expression.Invoke(
                        Expression.Constant(chain.Context.Component.ImplementionFactory),
                        ResolverParameter
                    ),
                    ScopeParameter,
                    ResolverParameter,
                    ResolvedServicesParameter)
                .Compile();

            return Expression.Constant(factory);
        }

        protected virtual Expression BuildWithConstructor(ServiceDependChain node)
        {
            var parameterValues = new Expression[node.Parameters?.Count ?? 0];
            var varible = Expression.Variable(node.Constructor.ConstructorInfo.DeclaringType);
            var lable = Expression.Label(varible.Type);
            var statements = new List<Expression>();

            for (var i = 0; i < parameterValues.Length; i++)
            {
                var parameterValue = BuildExpression(node.Parameters[node.Constructor.Paramters[i]]);
                if (!node.Constructor.Paramters[i].ParameterType.IsAssignableFrom(parameterValue.Type))
                {
                    parameterValue = Expression.Convert(parameterValue, node.Constructor.Paramters[i].ParameterType);
                }

                parameterValues[i] = parameterValue;
            }

            statements.Add(
                Expression.Assign(
                    varible,
                    Expression.Convert(
                        Expression.New(node.Constructor.ConstructorInfo, parameterValues),
                        varible.Type
                    )
                )
            );

            if (node.Properties != null && node.Properties.Count != 0)
            {
                statements.Add(
                    Expression.Call(
                        AddResolvedServiceMethod,
                        ResolvedServicesParameter,
                        Expression.Constant(node),
                        varible
                    )
                );
            }

            statements.Add(Expression.Return(lable, varible));
            statements.Add(Expression.Label(lable, varible));

            var lambdaBody = Expression.Block(new[] { varible }, statements);
            var factory = (Func<IServiceLifeStyleScope, IServiceResolver, Dictionary<ServiceDependChain, object>, object>)
                Expression
                    .Lambda(lambdaBody, ScopeParameter, ResolverParameter, ResolvedServicesParameter)
                    .Compile();

            return Expression.Constant(factory);
        }

        protected virtual Expression BuildWithEnumerable(ServiceEnumerableDependChain node)
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

            var listInit = Expression.ListInit(Expression.New(node.Constructor.ConstructorInfo), elementInits);
            var factory = (Func<IServiceLifeStyleScope, IServiceResolver, Dictionary<ServiceDependChain, object>, object>)
                Expression
                    .Lambda(listInit, ScopeParameter, ResolverParameter, ResolvedServicesParameter)
                    .Compile();

            return Expression.Constant(factory);
        }

        protected virtual Delegate BuildPropertySetter(ServiceDependChain node)
        {
            var instance = Expression.Parameter(node.Constructor.ConstructorInfo.DeclaringType);
            var label = Expression.Label();

            var variables = new List<ParameterExpression>();
            var statements = new List<Expression>();

            foreach (var item in node.Properties)
            {
                if (IsPropertyCircularDependeny(node, item.Value))
                {
                    continue;
                };

                var property = Expression.MakeMemberAccess(instance, item.Key);
                var propertyVariable = Expression.Variable(item.Key.PropertyType);

                var propertyValue = CreatePropertyValue(item.Value);
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

            statements.Add(Expression.Return(label));
            statements.Add(Expression.Label(label));

            return
                Expression
                .Lambda(Expression.Block(variables, statements), instance, ResolverParameter, ScopeParameter)
                .Compile();
        }

        protected virtual Expression CreatePropertyValue(ServiceDependChain chain)
        {
            var factory = CreateActivatorCore(chain);
            if (factory == null)
            {
                return Expression.Constant(null);
            }

            return
                Expression.Invoke(
                    Expression.Constant(factory),
                    ResolverParameter,
                    ScopeParameter,
                    Expression.Constant(new Dictionary<ServiceDependChain, object>()
                )
            );
        }

        protected void SetPropertyValue(Dictionary<ServiceDependChain, object> services, IServiceResolver resolver)
        {
            foreach (var item in services)
            {
                var serviceType = item.Key.Constructor.ConstructorInfo.DeclaringType;
                if (!_propertySetters.ContainsKey(serviceType))
                {
                    lock (_propertySetters)
                    {
                        if (!_propertySetters.ContainsKey(serviceType))
                        {
                            _propertySetters[serviceType] = BuildPropertySetter(item.Key);
                        }
                    }
                }

                var setter = _propertySetters[serviceType];
                if (setter != null)
                {
                    setter.DynamicInvoke(item.Value, resolver, resolver.Scope);
                }
            }
        }

        protected bool IsPropertyCircularDependeny(ServiceDependChain instanceChain, ServiceDependChain propertyChain)
        {
            if (!_resolvedProperties.ContainsKey(instanceChain.Context))
            {
                _resolvedProperties[instanceChain.Context] = new HashSet<IResolvingContext>()
                {
                    propertyChain.Context
                };

                return false;
            }

            var properties = _resolvedProperties[instanceChain.Context];
            if (properties.Contains(propertyChain.Context))
            {
                return true;
            }

            properties.Add(propertyChain.Context);

            return false;
        }

        protected virtual Expression WrapperWithLifeStyle(Expression func, ServiceDependChain chain)
        {
            if (!chain.IsNeedWrappedLifeStyle())
            {
                return
                    Expression.Invoke(
                        func,
                        ScopeParameter,
                        ResolverParameter,
                        ResolvedServicesParameter
                    );
            }

            var wrappedContext = Expression.Constant(chain.Context);

            return
                Expression.Invoke(
                    ApplyLifeStyleFuncConstant,
                    ScopeParameter,
                    wrappedContext,
                    ResolvedServicesParameter,
                    func
                );
        }

        public static void AddResolvedService(Dictionary<ServiceDependChain, object> services, ServiceDependChain chain, object instance)
        {
            if (!services.ContainsKey(chain) && instance != null)
            {
                services.Add(chain, instance);
            }
        }
    }
}
