using Xunit;
using Tinja.Test.Fakes;

namespace Tinja.Test
{
    public class FactoryServiceTests
    {
        [Fact]
        public void ResolveFactoryService()
        {
            var resolver = ResolverFactory.CreateResolver();

            var service = resolver.Resolve<IFactoryService>();

            Assert.NotNull(service);
            Assert.NotNull(service.Service);
        }

        [Fact]
        public void ResolveScopedFactoryService()
        {
            var container = new Container();

            container.AddScoped<ITransientServiceA, TransientServiceA>();
            container.AddScoped<ITransientServiceB, TransientServiceB>();
            container.AddScoped<ITransientServiceC, TransientServiceC>();
            container.AddScoped(typeof(IFactoryService), _ => new FactoryService(_.Resolve<ITransientServiceA>()));

            var resolver = container.BuildResolver();

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
    }
}
