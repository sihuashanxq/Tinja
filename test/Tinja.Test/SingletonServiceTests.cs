using Tinja.Test.Fakes;
using Xunit;

namespace Tinja.Test
{
    public class SingletonServiceTests
    {
        [Fact]
        public void ResolveSingletonService()
        {
            var resolver = ResolverFactory.CreateResolver();

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
            var resolver = ResolverFactory.CreateResolver();
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
    }
}
