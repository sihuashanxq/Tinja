using Tinja.Resolving;
using Tinja.Test.Fakes;

namespace Tinja.Test
{
    public static class ResolverFactory
    {
        public static IServiceResolver CreateResolver()
        {
            var container = new Container();

            container.AddScoped<IScopedServiceC, ScopedServiceC>();
            container.AddScoped<IScopedServiceB, ScopedServiceB>();
            container.AddScoped<IScopeServiceA, ScopedServiceA>();

            container.AddSingleton<ISingletonServiceC, SingletonServiceC>();
            container.AddSingleton<ISingletonServiceB, SingletonServiceB>();
            container.AddSingleton<ISingletonServiceA, SingletonServiceA>();

            container.AddTransient<ITransientServiceC, TransientServiceC>();
            container.AddTransient<ITransientServiceB, TransientServiceB>();
            container.AddTransient<ITransientServiceA, TransientServiceA>();

            container.AddTransient(typeof(IFactoryService), (resolver) => new FactoryService(resolver.Resolve<ITransientServiceA>()));

            return container.BuildResolver();
        }
    }
}
