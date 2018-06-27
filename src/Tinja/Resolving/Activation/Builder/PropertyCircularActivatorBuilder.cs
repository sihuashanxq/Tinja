using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Tinja.Resolving.Dependency;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Activation.Builder
{
    public class PropertyCircularActivatorBuilder : IActivatorBuilder
    {
        public static IActivatorBuilder Default = new PropertyCircularActivatorBuilder();

        private delegate object ApplyLifeStyleDelegate(
            IServiceLifeScope scope,
            Type serviceType,
            ServiceLifeStyle lifeStyle,
            PropertyCircularInjectionContext injectionContext,
            Func<IServiceLifeScope, IServiceResolver, PropertyCircularInjectionContext, object> factory
        );

        private static ParameterExpression ScopeParameter { get; }

        private static ParameterExpression ResolverParameter { get; }

        private static ConstantExpression ApplyLifeStyleFuncConstant { get; }

        private static ParameterExpression InjectionContextParameter { get; }

        private static ApplyLifeStyleDelegate ApplyLifeStyleFunc { get; }

        private static Action<IServiceResolver, PropertyCircularInjectionContext> SetPropertyValueFunc { get; }

        static PropertyCircularActivatorBuilder()
        {
            ScopeParameter = Expression.Parameter(typeof(IServiceLifeScope));
            ResolverParameter = Expression.Parameter(typeof(IServiceResolver));
            InjectionContextParameter = Expression.Parameter(typeof(PropertyCircularInjectionContext));
            SetPropertyValueFunc = SetPropertyValue;
            ApplyLifeStyleFunc = (scope, serviceType, lifeStyle, injectionContext, factory) => scope.ApplyServiceLifeStyle(serviceType, lifeStyle, resolver => factory(resolver.ServiceLifeScope, resolver, injectionContext));
            ApplyLifeStyleFuncConstant = Expression.Constant(ApplyLifeStyleFunc, typeof(ApplyLifeStyleDelegate));
        }

        public virtual Func<IServiceResolver, IServiceLifeScope, object> Build(ServiceCallDependency callDependency)
        {
            var factory = CreateActivatorCore(callDependency);
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
            CreateActivatorCore(ServiceCallDependency callDependency)
        {
            var lambdaBody = BuildExpression(callDependency);
            if (lambdaBody == null)
            {
                throw new NullReferenceException(nameof(lambdaBody));
            }

            var varible = Expression.Variable(callDependency.Context.ServiceType);
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

        protected static Expression BuildExpression(ServiceCallDependency callDependency)
        {
            Expression func;

            if (callDependency.Context.ImplementionFactory != null)
            {
                func = BuildDelegateImplemention(callDependency);
            }
            else if (callDependency.Context.ImplementionInstance != null)
            {
                func = BuildInstanceImplemention(callDependency);
            }
            else if (callDependency is ServiceManyCallDependency enumerable)
            {
                func = BuildManyImplemention(enumerable);
            }
            else
            {
                func = BuildTypeImplemention(callDependency);
            }

            return func == null ? null : WrapperWithLifeStyle(func, callDependency);
        }

        protected static Expression BuildDelegateImplemention(ServiceCallDependency callDependency)
        {
            var factory = (Func<IServiceLifeScope, IServiceResolver, PropertyCircularInjectionContext, object>)
                Expression
                .Lambda(
                    Expression.Invoke(
                        Expression.Constant(callDependency.Context.ImplementionFactory),
                        ResolverParameter
                    ),
                    ScopeParameter,
                    ResolverParameter,
                    InjectionContextParameter)
                .Compile();

            return Expression.Constant(factory);
        }

        protected static Expression BuildInstanceImplemention(ServiceCallDependency callDependency)
        {
            var factory = (Func<IServiceLifeScope, IServiceResolver, PropertyCircularInjectionContext, object>)
                Expression
                    .Lambda(
                        Expression.Constant(callDependency.Context.ImplementionInstance),
                        ScopeParameter,
                        ResolverParameter,
                        InjectionContextParameter)
                    .Compile();

            return Expression.Constant(factory);
        }

        protected static Expression BuildTypeImplemention(ServiceCallDependency callDependency)
        {
            var parameterValues = new Expression[callDependency.Parameters?.Count ?? 0];
            var varible = Expression.Variable(callDependency.Constructor.ConstructorInfo.DeclaringType);
            var lable = Expression.Label(varible.Type);
            var statements = new List<Expression>();

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

            statements.Add(
                Expression.Assign(
                    varible,
                    Expression.Convert(
                        Expression.New(callDependency.Constructor.ConstructorInfo, parameterValues),
                        varible.Type
                    )
                )
            );

            if (callDependency.Properties != null && callDependency.Properties.Count != 0)
            {
                statements.Add(
                    Expression.Call(
                        InjectionContextParameter,
                        PropertyCircularInjectionContext.AddResolvedServiceMethod,
                        Expression.Constant(callDependency),
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

        protected static Expression BuildManyImplemention(ServiceManyCallDependency callDependency)
        {
            var elementInits = new ElementInit[callDependency.Elements.Length];
            var addElement = callDependency.Context.ImplementionType.GetMethod("Add");

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

            var listInit = Expression.ListInit(Expression.New(callDependency.Constructor.ConstructorInfo), elementInits);
            var factory = (Func<IServiceLifeScope, IServiceResolver, PropertyCircularInjectionContext, object>)
                Expression
                    .Lambda(listInit, ScopeParameter, ResolverParameter, InjectionContextParameter)
                    .Compile();

            return Expression.Constant(factory);
        }

        protected static Delegate BuildPropertySetter(ServiceCallDependency callDependency, PropertyCircularInjectionContext context)
        {
            var instance = Expression.Parameter(callDependency.Constructor.ConstructorInfo.DeclaringType);
            var label = Expression.Label();

            var variables = new List<ParameterExpression>();
            var statements = new List<Expression>();

            foreach (var item in callDependency.Properties)
            {
                if (callDependency.Context.LifeStyle != ServiceLifeStyle.Transient &&
                    context.IsPropertyHandled(callDependency.Constructor.ConstructorInfo.DeclaringType, item.Key))
                {
                    continue;
                }

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

        protected static Expression CreatePropertyValue(ServiceCallDependency callDependency)
        {
            var factory = CreateActivatorCore(callDependency);
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

                setter?.DynamicInvoke(item.Value, resolver, resolver.ServiceLifeScope, context);
            }
        }

        protected static Expression WrapperWithLifeStyle(Expression func, ServiceCallDependency callDependency)
        {
            if (!callDependency.ShouldHoldServiceLife())
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
                    Expression.Constant(callDependency.Context.ServiceType),
                    Expression.Constant(callDependency.Context.LifeStyle),
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

            public void AddResolvedService(ServiceCallDependency callDependency, object instance)
            {
                if (!ResolvedServices.ContainsKey(callDependency) && instance != null)
                {
                    ResolvedServices.Add(callDependency, instance);
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
