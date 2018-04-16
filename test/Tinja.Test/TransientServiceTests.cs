using Tinja.Test.Fakes;
using Xunit;

namespace Tinja.Test
{
    public class TransientServiceTests
    {
        [Fact]
        public void ResolveTransient()
        {
            var resolver = ResolverFactory.CreateResolver();

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
    }
}
