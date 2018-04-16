using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Tinja.Test.Fakes;

namespace Tinja.Test
{
    public class ScopedServiceTests
    {
        [Fact]
        public void ResolveScopedService()
        {
            var resolver = ResolverFactory.CreateResolver();

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
            var resolver = ResolverFactory.CreateResolver();
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
    }
}
