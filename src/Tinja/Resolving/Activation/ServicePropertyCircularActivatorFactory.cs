using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Tinja.ServiceLife;
using Tinja.Resolving.Dependency;
using Tinja.Extensions;
namespace Tinja.Resolving.Activation
{
    public class ServicePropertyCircularActivatorFactory : IServiceActivatorFactory
    {
        delegate object ApplyLifeStyleDelegate(
            IServiceLifeScope scope,
            Type serviceType,
            ServiceLifeStyle lifeStyle,
            PropertyCircularInjectionContext injectionContext,
            Func<IServiceLifeScope, IServiceResolver, PropertyCircularInjectionContext, object> factory
        );

        static ParameterExpression ScopeParameter { get; }

        static ParameterExpression ResolverParameter { get; }

        static ConstantExpression ApplyLifeStyleFuncConstant { get; }

        static ParameterExpression InjectionContextParameter { get; }

        static ApplyLifeStyleDelegate ApplyLifeStyleFunc { get; }

        static Action<IServiceResolver, PropertyCircularInjectionContext> SetPropertyValueFunc { get; }

        static ServicePropertyCircularActivatorFactory()
        {
            ScopeParameter = Expression.Parameter(typeof(IServiceLifeScope));
            ResolverParameter = Expression.Parameter(typeof(IServiceResolver));
            InjectionContextParameter = Expression.Parameter(typeof(PropertyCircularInjectionContext));

            SetPropertyValueFunc = (resolver, context) => SetPropertyValue(resolver, context);

            ApplyLifeStyleFunc = (scope, serviceType, lifeStyle, injectionContext, factory) => scope.ApplyServiceLifeStyle(serviceType, lifeStyle, resolver => factory(resolver.ServiceLifeScope, resolver, injectionContext));
            ApplyLifeStyleFuncConstant = Expression.Constant(ApplyLifeStyleFunc, typeof(ApplyLifeStyleDelegate));
        }

        public virtual Func<IServiceResolver, IServiceLifeScope, object> Create(ServiceCallDependency chain)
        {
            var factory = CreateActivatorCore(chain);
            if (factory == null)
            {
                return (resolver, scope) => null;
            }

            return (resolver, scope) =>
            {
                lock (this)
                {
                    return factory(resolver, scope, new PropertyCircularInjectionContext());
                }
            };
        }

        protected static
            Func<IServiceResolver, IServiceLifeScope, PropertyCircularInjectionContext, object>
            CreateActivatorCore(ServiceCallDependency chain)
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
                    ResolverParameter,
                    InjectionContextParameter
                ),
                Expression.Return(lable,varible),
                Expression.Label(lable, varible)
            };

            return (Func<IServiceResolver, IServiceLifeScope, PropertyCircularInjectionContext, object>)
                Expression
                .Lambda(
                    Expression.Block(new[] { varible }, statements),
                    ResolverParameter,
                    ScopeParameter,
                    InjectionContextParameter
                )
                .Compile();
        }

        protected static Expression BuildExpression(ServiceCallDependency chain)
        {
            var func = null as Expression;
            if (chain.Constructor == null)
            {
                func = BuildWithImplFactory(chain);
            }
            else if (chain is ServiceManyCallDependency enumerable)
            {
                func = BuildWithEnumerable(enumerable);
            }
            else
            {
                func = BuildWithConstructor(chain as ServiceCallDependency);
            }

            if (func == null)
            {
                return null;
            }

            return WrapperWithLifeStyle(func, chain);
        }

        protected static Expression BuildWithImplFactory(ServiceCallDependency chain)
        {
            var factory = (Func<IServiceLifeScope, IServiceResolver, PropertyCircularInjectionContext, object>)
                Expression
                .Lambda(
                    Expression.Invoke(
                        Expression.Constant(chain.Context.GetImplementionFactory()),
                        ResolverParameter
                    ),
                    ScopeParameter,
                    ResolverParameter,
                    InjectionContextParameter)
                .Compile();

            return Expression.Constant(factory);
        }

        protected static Expression BuildWithConstructor(ServiceCallDependency node)
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
                        InjectionContextParameter,
                        PropertyCircularInjectionContext.AddResolvedServiceMethod,
                        Expression.Constant(node),
                        varible
                    )
                );
            }

            statements.Add(Expression.Return(lable, varible));
            statements.Add(Expression.Label(lable, varible));

            var lambdaBody = Expression.Block(new[] { varible }, statements);
            var factory = (Func<IServiceLifeScope, IServiceResolver, PropertyCircularInjectionContext, object>)
                Expression
                    .Lambda(lambdaBody, ScopeParameter, ResolverParameter, InjectionContextParameter)
                    .Compile();

            return Expression.Constant(factory);
        }

        protected static Expression BuildWithEnumerable(ServiceManyCallDependency node)
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

            var listInit = Expression.ListInit(Expression.New(node.Constructor.ConstructorInfo), elementInits);
            var factory = (Func<IServiceLifeScope, IServiceResolver, PropertyCircularInjectionContext, object>)
                Expression
                    .Lambda(listInit, ScopeParameter, ResolverParameter, InjectionContextParameter)
                    .Compile();

            return Expression.Constant(factory);
        }

        protected static Delegate BuildPropertySetter(ServiceCallDependency node, PropertyCircularInjectionContext context)
        {
            var instance = Expression.Parameter(node.Constructor.ConstructorInfo.DeclaringType);
            var label = Expression.Label();

            var variables = new List<ParameterExpression>();
            var statements = new List<Expression>();

            foreach (var item in node.Properties)
            {
                if (node.Context.LifeStyle != ServiceLifeStyle.Transient &&
                    context.IsPropertyHandled(node.Constructor.ConstructorInfo.DeclaringType, item.Key))
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
                .Lambda(Expression.Block(variables, statements), instance, ResolverParameter, ScopeParameter, InjectionContextParameter)
                .Compile();
        }

        protected static Expression CreatePropertyValue(ServiceCallDependency chain)
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
                    Expression.Call(InjectionContextParameter, PropertyCircularInjectionContext.CreateScopeMethod)
                );
        }

        protected static void SetPropertyValue(IServiceResolver resolver, PropertyCircularInjectionContext context)
        {
            foreach (var item in context.ResolvedServices)
            {
                var setter = context.PropertySetters.GetOrAdd(
                    item.Key.Constructor.ConstructorInfo.DeclaringType,
                    _ => BuildPropertySetter(item.Key, context));

                if (setter != null)
                {
                    setter.DynamicInvoke(item.Value, resolver, resolver.ServiceLifeScope, context);
                }
            }
        }

        protected static Expression WrapperWithLifeStyle(Expression func, ServiceCallDependency chain)
        {
            if (!chain.ShouldHoldServiceLife())
            {
                return
                    Expression.Invoke(
                        func,
                        ScopeParameter,
                        ResolverParameter,
                        InjectionContextParameter
                    );
            }

            return
                Expression.Invoke(
                    ApplyLifeStyleFuncConstant,
                    ScopeParameter,
                    Expression.Constant(chain.Context.ServiceType),
                    Expression.Constant(chain.Context.LifeStyle),
                    InjectionContextParameter,
                    func
                );
        }

        protected internal class PropertyCircularInjectionContext
        {
            /// <summary>
            /// need property inject instance
            /// </summary>
            public Dictionary<ServiceCallDependency, object> ResolvedServices { get; }

            public Dictionary<Type, HashSet<PropertyInfo>> HandledProperties { get; }

            public static MethodInfo AddResolvedServiceMethod { get; }

            public static MethodInfo CreateScopeMethod { get; }

            public ConcurrentDictionary<Type, Delegate> PropertySetters { get; private set; }

            static PropertyCircularInjectionContext()
            {
                AddResolvedServiceMethod = typeof(PropertyCircularInjectionContext)
                    .GetMethod(nameof(AddResolvedService));

                CreateScopeMethod = typeof(PropertyCircularInjectionContext)
                    .GetMethod(nameof(CreateScope));
            }

            public PropertyCircularInjectionContext()
            {
                HandledProperties = new Dictionary<Type, HashSet<PropertyInfo>>();
                ResolvedServices = new Dictionary<ServiceCallDependency, object>();
                PropertySetters = new ConcurrentDictionary<Type, Delegate>();
            }

            private PropertyCircularInjectionContext(Dictionary<Type, HashSet<PropertyInfo>> handledProperties)
            {
                HandledProperties = handledProperties;
                ResolvedServices = new Dictionary<ServiceCallDependency, object>();
            }

            public bool IsPropertyHandled(Type serviceType, PropertyInfo propertyInfo)
            {
                if (!HandledProperties.ContainsKey(serviceType))
                {
                    HandledProperties[serviceType] = new HashSet<PropertyInfo>() { propertyInfo };
                    return false;
                }

                var properties = HandledProperties[serviceType];
                if (properties.Contains(propertyInfo))
                {
                    return true;
                }

                properties.Add(propertyInfo);
                return false;
            }

            public void AddResolvedService(ServiceCallDependency chain, object instance)
            {
                if (!ResolvedServices.ContainsKey(chain) && instance != null)
                {
                    ResolvedServices.Add(chain, instance);
                }
            }

            public PropertyCircularInjectionContext CreateScope()
            {
                return new PropertyCircularInjectionContext(HandledProperties)
                {
                    PropertySetters = PropertySetters
                };
            }
        }
    }
}
