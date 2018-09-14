using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using Tinja.Abstractions.Extensions;
using Tinja.Core;
using Tinja.Core.Extensions;
using Tinja.Core.Injection.Dependencies;
using Tinja.Test.Fakes.Consturctor;
using Tinja.Test.Fakes.Generic;
using Tinja.Test.Fakes.Property;

namespace Tinja.Test
{
    public class ServiceResolverTests
    {
        [Fact]
        public void ResolveTransientService()
        {
            var resolver = new Container()
                .AddTransient<ITransientServiceC, TransientServiceC>()
                .AddTransient<ITransientServiceB, TransientServiceB>()
                .AddTransient<ITransientServiceA, TransientServiceA>()
                .BuildServiceResolver();

            var serviceA = resolver.ResolveService<ITransientServiceA>();
            var serviceB = resolver.ResolveService<ITransientServiceB>();
            var serviceC = resolver.ResolveService<ITransientServiceC>();

            Assert.NotNull(serviceA);
            Assert.NotNull(serviceB);
            Assert.NotNull(serviceC);
            Assert.NotNull(serviceA.ServiceB);
            Assert.NotNull(serviceA.ServiceC);

            Assert.NotEqual(serviceA.ServiceB, serviceB);
            Assert.NotEqual(serviceA.ServiceC, serviceC);
        }

        [Fact]
        public void ResolveScopedService()
        {
            var resolver = new Container()
                .AddTransient<ITransientServiceC, TransientServiceC>()
                .AddScoped<IScopedServiceC, ScopedServiceC>()
                .AddScoped<IScopedServiceB, ScopedServiceB>()
                .AddScoped<IScopeServiceA, ScopedServiceA>()
                .BuildServiceResolver();

            var serviceA1 = resolver.ResolveService<IScopeServiceA>();
            var serviceA2 = resolver.ResolveService<IScopeServiceA>();

            var serviceB1 = resolver.ResolveService<IScopedServiceB>();
            var serviceB2 = resolver.ResolveService<IScopedServiceB>();

            var serviceC1 = resolver.ResolveService<IScopedServiceC>();
            var serviceC2 = resolver.ResolveService<IScopedServiceC>();

            Assert.NotNull(serviceA1);
            Assert.NotNull(serviceA2);
            Assert.NotNull(serviceB1);
            Assert.NotNull(serviceB2);
            Assert.NotNull(serviceC1);
            Assert.NotNull(serviceC2);

            Assert.NotNull(serviceA1.ServiceB);
            Assert.NotNull(serviceA1.ServiceC);
            Assert.NotNull(serviceA2.ServiceB);
            Assert.NotNull(serviceA2.ServiceC);
            Assert.NotNull(serviceB1.TransientServiceC);
            Assert.NotNull(serviceB2.TransientServiceC);

            Assert.Equal(serviceA1, serviceA2);
            Assert.Equal(serviceB1, serviceB2);
            Assert.Equal(serviceC1, serviceC2);

            Assert.Equal(serviceA1.ServiceB, serviceA2.ServiceB);
            Assert.Equal(serviceA1.ServiceC, serviceA2.ServiceC);

            Assert.Equal(serviceA1.ServiceB, serviceB1);
            Assert.Equal(serviceA1.ServiceC, serviceC1);

            Assert.Equal(serviceB1.TransientServiceC, serviceB2.TransientServiceC);
        }

        [Fact]
        public void ResolveDiffSopedService()
        {
            var resolver = new Container()
                .AddTransient<ITransientServiceC, TransientServiceC>()
                .AddScoped<IScopedServiceC, ScopedServiceC>()
                .AddScoped<IScopedServiceB, ScopedServiceB>()
                .AddScoped<IScopeServiceA, ScopedServiceA>()
                .BuildServiceResolver();

            var scoped1Resolver = resolver.CreateScope();
            var scoped2Resolver = resolver.CreateScope();

            var serviceA1 = scoped1Resolver.ServiceResolver.ResolveService<IScopeServiceA>();
            var serviceA2 = scoped2Resolver.ServiceResolver.ResolveService<IScopeServiceA>();

            Assert.NotNull(serviceA1);
            Assert.NotNull(serviceA2);

            Assert.NotNull(serviceA1.ServiceB);
            Assert.NotNull(serviceA2.ServiceB);
            Assert.NotNull(serviceA1.ServiceC);
            Assert.NotNull(serviceA2.ServiceC);

            Assert.NotEqual(serviceA1, serviceA2);
            Assert.NotEqual(serviceA1.ServiceB, serviceA2.ServiceB);
            Assert.NotEqual(serviceA1.ServiceC, serviceA2.ServiceC);
        }

        [Fact]
        public void ResolveSingletonService()
        {
            var resolver = new Container()
                .AddTransient<ITransientServiceC, TransientServiceC>()
                .AddSingleton<ISingletonServiceC, SingletonServiceC>()
                .AddSingleton<ISingletonServiceB, SingletonServiceB>()
                .AddSingleton<ISingletonServiceA, SingletonServiceA>()
                .BuildServiceResolver();

            var serviceA1 = resolver.ResolveService<ISingletonServiceA>();
            var serviceA2 = resolver.ResolveService<ISingletonServiceA>();

            var serviceB1 = resolver.ResolveService<ISingletonServiceB>();
            var serviceB2 = resolver.ResolveService<ISingletonServiceB>();

            var serviceC1 = resolver.ResolveService<ISingletonServiceC>();
            var serviceC2 = resolver.ResolveService<ISingletonServiceC>();

            Assert.NotNull(serviceA1);
            Assert.NotNull(serviceA2);
            Assert.NotNull(serviceB1);
            Assert.NotNull(serviceB2);
            Assert.NotNull(serviceC1);
            Assert.NotNull(serviceC2);

            Assert.NotNull(serviceA1.ServiceB);
            Assert.NotNull(serviceA1.ServiceC);
            Assert.NotNull(serviceA2.ServiceB);
            Assert.NotNull(serviceA2.ServiceC);
            Assert.NotNull(serviceC1.TransientServiceC);
            Assert.NotNull(serviceC2.TransientServiceC);

            Assert.Equal(serviceA1, serviceA2);
            Assert.Equal(serviceB1, serviceB2);
            Assert.Equal(serviceC1, serviceC2);

            Assert.Equal(serviceA1.ServiceB, serviceA2.ServiceB);
            Assert.Equal(serviceA1.ServiceC, serviceA2.ServiceC);

            Assert.Equal(serviceA1.ServiceB, serviceB1);
            Assert.Equal(serviceA1.ServiceC, serviceC1);

            Assert.Equal(serviceC1.TransientServiceC, serviceC2.TransientServiceC);
        }

        [Fact]
        public void ResolveDiffSopeSingletonService()
        {
            var resolver = new Container()
                .AddTransient<ITransientServiceC, TransientServiceC>()
                .AddSingleton<ISingletonServiceC, SingletonServiceC>()
                .AddSingleton<ISingletonServiceB, SingletonServiceB>()
                .AddSingleton<ISingletonServiceA, SingletonServiceA>()
                .BuildServiceResolver();

            var scoped1Resolver = resolver.CreateScope();
            var scoped2Resolver = resolver.CreateScope();

            var serviceA1 = scoped1Resolver.ServiceResolver.ResolveService<ISingletonServiceA>();
            var serviceA2 = scoped2Resolver.ServiceResolver.ResolveService<ISingletonServiceA>();

            Assert.NotNull(serviceA1);
            Assert.NotNull(serviceA2);

            Assert.NotNull(serviceA1.ServiceB);
            Assert.NotNull(serviceA2.ServiceB);
            Assert.NotNull(serviceA1.ServiceC);
            Assert.NotNull(serviceA2.ServiceC);

            Assert.Equal(serviceA1, serviceA2);
            Assert.Equal(serviceA1.ServiceB, serviceA2.ServiceB);
            Assert.Equal(serviceA1.ServiceC, serviceA2.ServiceC);
        }

        [Fact]
        public void ResolveFactoryService()
        {
            var resolver = new Container()
                .AddTransient<ITransientServiceC, TransientServiceC>()
                .AddTransient<ITransientServiceB, TransientServiceB>()
                .AddTransient<ITransientServiceA, TransientServiceA>()
                .AddTransient<IFactoryService, FactoryService>()
                .BuildServiceResolver();

            var service = resolver.ResolveService<IFactoryService>();

            Assert.NotNull(service);
            Assert.NotNull(service.Service);
        }

        [Fact]
        public void ResolveScopedFactoryService()
        {
            var resolver = new Container()
                .AddScoped<ITransientServiceA, TransientServiceA>()
                .AddScoped<ITransientServiceB, TransientServiceB>()
                .AddScoped<ITransientServiceC, TransientServiceC>()
                .AddScoped(typeof(IFactoryService), _ => new FactoryService(_.ResolveService<ITransientServiceA>()))
                .BuildServiceResolver();

            var scope1 = resolver.CreateScope();
            var scope2 = resolver.CreateScope();

            var service1 = scope1.ServiceResolver.ResolveService<IFactoryService>();
            var service2 = scope2.ServiceResolver.ResolveService<IFactoryService>();

            var service3 = scope1.ServiceResolver.ResolveService<IFactoryService>();
            var service4 = scope2.ServiceResolver.ResolveService<IFactoryService>();

            Assert.NotNull(service1);
            Assert.NotNull(service2);

            Assert.NotEqual(service1, service2);
            Assert.Equal(service1, service3);
            Assert.Equal(service2, service4);
        }

        [Fact]
        public void ResolvePropertyService()
        {
            for (var i = 0; i < 1000; i++)
            {
                var resolver = new Container()
                    .AddScoped<ITransientServiceB, TransientServiceB>()
                    .AddTransient<IPropertyInjectionService, PropertyInjectionService>()
                    .BuildServiceResolver();

                Action action = () =>
                {
                    var service = resolver.ResolveService<IPropertyInjectionService>();
                    var serviceB = resolver.ResolveService<ITransientServiceB>();

                    Assert.NotNull(service);
                    Assert.NotNull(service.ServiceB);
                    Assert.NotNull(serviceB);
                    Assert.Equal(serviceB, service.ServiceB);
                };

                var tasks = new List<Task>();

                for (var n = 0; n < 100; n++)
                {
                    tasks.Add(Task.Run(action));
                }

                Task.WaitAll(tasks.ToArray());
            }
        }

        [Fact]
        public void ResolveEnumerableService()
        {
            var resolver = new Container()
                .AddTransient<ITransientServiceC, TransientServiceC>()
                .AddTransient<ITransientServiceB, TransientServiceB>()
                .AddTransient<ITransientServiceA, TransientServiceA2>()
                .AddTransient<ITransientServiceA, TransientServiceA>()
                .BuildServiceResolver();

            var services = resolver.ResolveService<IEnumerable<ITransientServiceA>>();
            var serviceA = resolver.ResolveService<ITransientServiceA>();
            var service2 = resolver.ResolveService<IEnumerable<TransientServiceA>>();

            Assert.NotNull(serviceA);
            Assert.Equal(typeof(TransientServiceA), serviceA.GetType());

            Assert.NotNull(services);
            Assert.Equal(2, services.Count());

            Assert.NotNull(service2);
            Assert.Empty(service2);
        }

        [Fact]
        public void ResolveGenericService()
        {
            var resolver = new Container()
                .AddTransient(typeof(IGenericService<>), typeof(GenericService<>))
                .AddTransient(typeof(IGenericService2<>), typeof(GenericService2<>))
                .AddTransient<ITransientServiceB, TransientServiceB>()
                .AddTransient<ITransientServiceC, TransientServiceC>()
                .BuildServiceResolver();

            var genericService = resolver.ResolveService<IGenericService<ITransientServiceB>>();
            var genericService2 = resolver.ResolveService<IGenericService2<ITransientServiceB>>();

            Assert.NotNull(genericService);
            Assert.NotNull(genericService2);
            Assert.NotNull(genericService.Service);
            Assert.Throws<InvalidOperationException>(() => resolver.ResolveService<IGenericService<ITransientServiceA>>());
        }

        [Fact]
        public void ResolveConstructorCircularService()
        {
            var resolver = new Container()
                .AddTransient<IParamterService, ParamterServie>()
                .AddTransient<IConstructorCircularDepenencyService, ConstructorCircularDepenencyService>()
                .BuildServiceResolver();

            Assert.Throws<CallCircularException>(() => resolver.ResolveService<IConstructorCircularDepenencyService>());
            Assert.Throws<CallCircularException>(() => resolver.ResolveService<IParamterService>());
        }

        [Fact]
        public void ResolvePropertyCircularService()
        {
            var resolver = new Container()
                .AddTransient<IPropertyServiceA, PropertyServiceA>()
                .AddTransient<IPropertyServiceB, PropertyServiceB>()
                .AddTransient<IPropertyCircularInjectionService, PropertyCircularInjectionService>()
                .BuildServiceResolver();

            Assert.Throws<CallCircularException>(() => resolver.ResolveService<IPropertyServiceA>());
        }
    }
}
