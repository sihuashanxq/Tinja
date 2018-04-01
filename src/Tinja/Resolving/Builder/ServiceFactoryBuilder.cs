using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Tinja.Resolving.Builder
{
    public class ServiceFactoryBuilder : IServiceFactoryBuilder
    {
        public static ConcurrentDictionary<Type, Func<IContainer, ILifeStyleScope, object>> FactoryCache { get; }

        static MethodInfo BuildMethod { get; }

        static MethodInfo BuildParamterMethod { get; }

        static ParameterExpression ParameterContainer { get; }

        static ParameterExpression ParameterLifeScope { get; }

        static ServiceFactoryBuilder()
        {
            BuildMethod = typeof(ServiceFactoryBuilder).GetMethod(nameof(BuildFactory));
            BuildParamterMethod = typeof(ServiceFactoryBuilder).GetMethod(nameof(BuildParamter));

            FactoryCache = new ConcurrentDictionary<Type, Func<IContainer, ILifeStyleScope, object>>();
            ParameterContainer = Expression.Parameter(typeof(IContainer));
            ParameterLifeScope = Expression.Parameter(typeof(ILifeStyleScope));
        }

        public Func<IContainer, ILifeStyleScope, object> Build(ServiceFactoryBuildContext context)
        {
            return GetOrAddFactory(context);
        }

        public Func<IContainer, ILifeStyleScope, object> Build(Type resolvingType)
        {
            if (FactoryCache.TryGetValue(resolvingType, out var factory))
            {
                return factory;
            }

            return null;
        }

        public static Func<IContainer, ILifeStyleScope, object> GetOrAddFactory(ServiceFactoryBuildContext context)
        {
            return FactoryCache.GetOrAdd(context.ResolvingContext.ReslovingType, (k) => BuildFactory(context));
        }

        public static Func<IContainer, ILifeStyleScope, object> BuildFactory(ServiceFactoryBuildContext context)
        {
            Func<IContainer, ILifeStyleScope, object> factory;

            if (context.ParamtersContext == null || context.ParamtersContext.Length == 0)
            {
                factory = (Func<IContainer, ILifeStyleScope, object>)Expression
                   .Lambda(Expression.New(context.Constructor.ConstructorInfo), ParameterContainer, ParameterLifeScope)
                   .Compile();

                return (o, scope) =>
                {
                    return scope.GetOrAddLifeScopeInstance(context.ResolvingContext, (_) => factory(o, scope));
                };
            }

            var parameters = new Expression[context.Constructor.Paramters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                parameters[i] = Expression.Convert(
                    Expression.Invoke(
                        Expression.Call(
                            BuildParamterMethod,
                            Expression.Constant(context.ParamtersContext[i])
                        ),
                        ParameterContainer,
                        ParameterLifeScope
                    ),
                    context.ParamtersContext[i].Parameter.ParameterType
                );
            }

            factory = (Func<IContainer, ILifeStyleScope, object>)Expression
                 .Lambda(Expression.New(context.Constructor.ConstructorInfo, parameters), ParameterContainer, ParameterLifeScope)
                 .Compile();

            return (o, scope) =>
            {
                return scope.GetOrAddLifeScopeInstance(context.ResolvingContext, (_) => factory(o, scope));
            };
        }

        public static Func<IContainer, ILifeStyleScope, object> BuildParamter(ServiceFactoryBuildParamterContext context)
        {
            if (context.ParameterTypeContext.Constructor == null)
            {
                return (o, scope) =>
                {
                    return scope.GetOrAddLifeScopeInstance(
                         context.ParameterTypeContext.ResolvingContext,
                         (_) => context.ParameterTypeContext.ResolvingContext.Component.ImplementionFactory(o)
                     );
                };
            }

            return (o, scope) =>
            {
                return scope.GetOrAddLifeScopeInstance(
                       context.ParameterTypeContext.ResolvingContext,
                       (_) => GetOrAddFactory(context.ParameterTypeContext)(o, scope)
                   );
            };
        }
    }
}
