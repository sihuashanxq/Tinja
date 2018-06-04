using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Tinja.Test.Fakes;
using System.Linq;
using System.Threading.Tasks;
using Tinja.Resolving.Dependency;

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
                .BuildResolver();

            var serviceA = resolver.Resolve<ITransientServiceA>();
            var serviceB = resolver.Resolve<ITransientServiceB>();
            var serviceC = resolver.Resolve<ITransientServiceC>();

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
                .BuildResolver();

            var serviceA1 = resolver.Resolve<IScopeServiceA>();
            var serviceA2 = resolver.Resolve<IScopeServiceA>();

            var serviceB1 = resolver.Resolve<IScopedServiceB>();
            var serviceB2 = resolver.Resolve<IScopedServiceB>();

            var serviceC1 = resolver.Resolve<IScopedServiceC>();
            var serviceC2 = resolver.Resolve<IScopedServiceC>();

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
                .BuildResolver();

            var scoped1Resolver = resolver.CreateScope();
            var scoped2Resolver = resolver.CreateScope();

            var serviceA1 = scoped1Resolver.Resolve<IScopeServiceA>();
            var serviceA2 = scoped2Resolver.Resolve<IScopeServiceA>();

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
                .BuildResolver();

            var serviceA1 = resolver.Resolve<ISingletonServiceA>();
            var serviceA2 = resolver.Resolve<ISingletonServiceA>();

            var serviceB1 = resolver.Resolve<ISingletonServiceB>();
            var serviceB2 = resolver.Resolve<ISingletonServiceB>();

            var serviceC1 = resolver.Resolve<ISingletonServiceC>();
            var serviceC2 = resolver.Resolve<ISingletonServiceC>();

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
                .BuildResolver();

            var scoped1Resolver = resolver.CreateScope();
            var scoped2Resolver = resolver.CreateScope();

            var serviceA1 = scoped1Resolver.Resolve<ISingletonServiceA>();
            var serviceA2 = scoped2Resolver.Resolve<ISingletonServiceA>();

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
                .BuildResolver();

            var service = resolver.Resolve<IFactoryService>();

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
                .AddScoped(typeof(IFactoryService), _ => new FactoryService(_.Resolve<ITransientServiceA>()))
                .BuildResolver();

            var scope1 = resolver.CreateScope();
            var scope2 = resolver.CreateScope();

            var service1 = scope1.Resolve<IFactoryService>();
            var service2 = scope2.Resolve<IFactoryService>();

            var service3 = scope1.Resolve<IFactoryService>();
            var service4 = scope2.Resolve<IFactoryService>();

            Assert.NotNull(service1);
            Assert.NotNull(service2);

            Assert.NotEqual(service1, service2);
            Assert.Equal(service1, service3);
            Assert.Equal(service2, service4);
        }

        [Fact]
        public void ResolvePropertyService()
        {
            var resolver = new Container()
                .AddScoped<ITransientServiceB, TransientServiceB>()
                .AddTransient<IPropertyInjectionService, PropertyInjectionService>()
                .BuildResolver();

            for (var i = 0; i < 10000; i++)
            {
                var t1 = Task.Run(() =>
                {
                    var service = resolver.Resolve<IPropertyInjectionService>();
                    var serviceB = resolver.Resolve<ITransientServiceB>();

                    Assert.NotNull(service);
                    Assert.NotNull(service.ServiceB);
                    Assert.NotNull(serviceB);

                    Assert.Equal(serviceB, service.ServiceB);
                });

                var t2 = Task.Run(() =>
                {
                    var service = resolver.Resolve<IPropertyInjectionService>();
                    var serviceB = resolver.Resolve<ITransientServiceB>();

                    Assert.NotNull(service);
                    Assert.NotNull(service.ServiceB);
                    Assert.NotNull(serviceB);

                    Assert.Equal(serviceB, service.ServiceB);
                });

                Task.WaitAll(t1, t2);
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
                .BuildResolver();

            var services = resolver.Resolve<IEnumerable<ITransientServiceA>>();
            var serviceA = resolver.Resolve<ITransientServiceA>();
            var service2 = resolver.Resolve<IEnumerable<TransientServiceA>>();

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
                .BuildResolver();

            var genericService = resolver.Resolve<IGenericService<ITransientServiceB>>();
            var genericService2 = resolver.Resolve<IGenericService2<ITransientServiceB>>();
            var genericService3 = resolver.Resolve<IGenericService<ITransientServiceA>>();

            Assert.NotNull(genericService);
            Assert.NotNull(genericService2);
            Assert.NotNull(genericService.Service);

            Assert.Null(genericService3);
        }

        [Fact]
        public void ResolveConstructorCircularService()
        {
            var resolver = new Container()
                .AddTransient<IParamterService, ParamterServie>()
                .AddTransient<IConstructorCircularDepenencyService, ConstructorCircularDepenencyService>()
                .BuildResolver();

            Assert.Throws<ServiceCallCircularExpcetion>(() => resolver.Resolve<IConstructorCircularDepenencyService>());
            Assert.Throws<ServiceCallCircularExpcetion>(() => resolver.Resolve<IParamterService>());
        }

        [Fact]
        public void ResolvePropertyTransientCircularService()
        {
            var resolver = new Container()
                .AddTransient<IPropertyServiceA, PropertyServiceA>()
                .AddTransient<IPropertyServiceB, PropertyServiceB>()
                .AddTransient<IPropertyCircularInjectionService, PropertyCircularInjectionService>()
                .BuildResolver();

            var propertyServiceA = resolver.Resolve<IPropertyServiceA>();
            var propertyServiceB = resolver.Resolve<IPropertyServiceB>();
            var propertyServiceC = resolver.Resolve<IPropertyCircularInjectionService>();

            Assert.NotNull(propertyServiceA);
            Assert.NotNull(propertyServiceB);
            Assert.NotNull(propertyServiceC);

            Assert.NotNull(propertyServiceA.Service);
            Assert.NotNull(propertyServiceB.Service);
            Assert.NotNull(propertyServiceC.Service);

            Assert.NotNull(propertyServiceA.Service.Service);
            Assert.NotNull(propertyServiceB.Service.Service);
            Assert.NotNull(propertyServiceC.Service.Service);

            Assert.Null(propertyServiceA.Service.Service.Service);
            Assert.Null(propertyServiceB.Service.Service.Service);
            Assert.Null(propertyServiceC.Service.Service.Service);
        }

        [Fact]
        public void ResolvePropertyOneScopedCircularService()
        {
            //just one scoped/singleton service in circular property dependency chain,injection success
            var resolver = new Container()
               .AddScoped<IPropertyServiceA, PropertyServiceA>()
               .AddTransient<IPropertyServiceB, PropertyServiceB>()
               .AddTransient<IPropertyCircularInjectionService, PropertyCircularInjectionService>()
               .BuildResolver();

            var propertyServiceA = resolver.Resolve<IPropertyServiceA>();
            var propertyServiceB = resolver.Resolve<IPropertyServiceB>();
            var propertyServiceC = resolver.Resolve<IPropertyCircularInjectionService>();

            Assert.NotNull(propertyServiceA);
            Assert.NotNull(propertyServiceB);
            Assert.NotNull(propertyServiceC);

            Assert.NotNull(propertyServiceA.Service);
            Assert.NotNull(propertyServiceB.Service);
            Assert.NotNull(propertyServiceC.Service);

            Assert.NotNull(propertyServiceA.Service.Service);
            Assert.NotNull(propertyServiceB.Service.Service);
            Assert.NotNull(propertyServiceC.Service.Service);

            Assert.NotNull(propertyServiceA.Service.Service.Service);
            Assert.NotNull(propertyServiceB.Service.Service.Service);
            Assert.NotNull(propertyServiceC.Service.Service.Service);
        }

        [Fact]
        public void ResolvePropertyAllScopedCircularService()
        {
            //just one scoped/singleton service in circular property dependency chain,injection success
            var resolver = new Container()
                   .AddScoped<IPropertyServiceA, PropertyServiceA>()
                   .AddScoped<IPropertyServiceB, PropertyServiceB>()
                   .AddScoped<IPropertyCircularInjectionService, PropertyCircularInjectionService>()
                   .BuildResolver();

            for (var i = 0; i < 1000; i++)
            {

                var t1 = Task.Run(() =>
                {
                    var propertyServiceA = resolver.Resolve<IPropertyServiceA>();
                    var propertyServiceB = resolver.Resolve<IPropertyServiceB>();
                    var propertyServiceC = resolver.Resolve<IPropertyCircularInjectionService>();

                    Assert.NotNull(propertyServiceA);
                    Assert.NotNull(propertyServiceB);
                    Assert.NotNull(propertyServiceC);

                    Assert.NotNull(propertyServiceA.Service);
                    Assert.NotNull(propertyServiceB.Service);
                    Assert.NotNull(propertyServiceC.Service);

                    Assert.NotNull(propertyServiceA.Service.Service);
                    Assert.NotNull(propertyServiceB.Service.Service);
                    Assert.NotNull(propertyServiceC.Service.Service);

                    Assert.NotNull(propertyServiceA.Service.Service.Service);
                    Assert.NotNull(propertyServiceB.Service.Service.Service);
                    Assert.NotNull(propertyServiceC.Service.Service.Service);

                    Assert.Equal(propertyServiceA.Service, propertyServiceC);
                    Assert.Equal(propertyServiceC.Service, propertyServiceB);
                    Assert.Equal(propertyServiceB.Service, propertyServiceA);
                });

                var t2 = Task.Run(() =>
                {
                    var propertyServiceA = resolver.Resolve<IPropertyServiceA>();
                    var propertyServiceB = resolver.Resolve<IPropertyServiceB>();
                    var propertyServiceC = resolver.Resolve<IPropertyCircularInjectionService>();

                    Assert.NotNull(propertyServiceA);
                    Assert.NotNull(propertyServiceB);
                    Assert.NotNull(propertyServiceC);

                    Assert.NotNull(propertyServiceA.Service);
                    Assert.NotNull(propertyServiceB.Service);
                    Assert.NotNull(propertyServiceC.Service);

                    Assert.NotNull(propertyServiceA.Service.Service);
                    Assert.NotNull(propertyServiceB.Service.Service);
                    Assert.NotNull(propertyServiceC.Service.Service);

                    Assert.NotNull(propertyServiceA.Service.Service.Service);
                    Assert.NotNull(propertyServiceB.Service.Service.Service);
                    Assert.NotNull(propertyServiceC.Service.Service.Service);

                    Assert.Equal(propertyServiceA.Service, propertyServiceC);
                    Assert.Equal(propertyServiceC.Service, propertyServiceB);
                    Assert.Equal(propertyServiceB.Service, propertyServiceA);
                });

                var t3 = Task.Run(() =>
                {
                    var propertyServiceA = resolver.Resolve<IPropertyServiceA>();
                    var propertyServiceB = resolver.Resolve<IPropertyServiceB>();
                    var propertyServiceC = resolver.Resolve<IPropertyCircularInjectionService>();

                    Assert.NotNull(propertyServiceA);
                    Assert.NotNull(propertyServiceB);
                    Assert.NotNull(propertyServiceC);

                    Assert.NotNull(propertyServiceA.Service);
                    Assert.NotNull(propertyServiceB.Service);
                    Assert.NotNull(propertyServiceC.Service);

                    Assert.NotNull(propertyServiceA.Service.Service);
                    Assert.NotNull(propertyServiceB.Service.Service);
                    Assert.NotNull(propertyServiceC.Service.Service);

                    Assert.NotNull(propertyServiceA.Service.Service.Service);
                    Assert.NotNull(propertyServiceB.Service.Service.Service);
                    Assert.NotNull(propertyServiceC.Service.Service.Service);

                    Assert.Equal(propertyServiceA.Service, propertyServiceC);
                    Assert.Equal(propertyServiceC.Service, propertyServiceB);
                    Assert.Equal(propertyServiceB.Service, propertyServiceA);
                });

                Task.WaitAll(t1, t2, t3);
            }
        }
    }
}
