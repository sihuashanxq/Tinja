using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Tinja.Registration;
using Tinja.Resolving;
using Tinja.Resolving.ReslovingContext;
using Xunit;

namespace Tinja.Test
{
    public interface IService<T>
    {

    }

    public class Service<T> : IService<T>
    {

    }

    public class ResolvingContextBuilderTests
    {
        [Fact]
        public void BuildContext()
        {
            var components = new ConcurrentDictionary<Type, List<Component>>();
            var serviceRegistrar = new ServiceRegistrar(components);
            var resolvingContextBuilder = new ResolvingContextBuilder(components);

            var serviceType = typeof(IService<>);
            var implType = typeof(Service<>);

            serviceRegistrar.Register(serviceType, implType, LifeStyle.Transient);

            var context = resolvingContextBuilder.BuildResolvingContext(typeof(IService<int>));

            Assert.NotNull(context);
        }

        [Fact]
        public void BuildGenericContext()
        {
            var components = new ConcurrentDictionary<Type, List<Component>>();
            var serviceRegistrar = new ServiceRegistrar(components);
            var resolvingContextBuilder = new ResolvingContextBuilder(components);

            serviceRegistrar.Register(typeof(IService<>), typeof(Service<>), LifeStyle.Transient);
            serviceRegistrar.Register(typeof(IService<int>), typeof(Service<int>), LifeStyle.Singleton);

            var context = resolvingContextBuilder.BuildResolvingContext(typeof(IService<string>));
            Assert.NotNull(context);
            Assert.Equal(LifeStyle.Transient, context.Component.LifeStyle);

            context = resolvingContextBuilder.BuildResolvingContext(typeof(IService<int>));

            Assert.NotNull(context);
            Assert.Equal(LifeStyle.Singleton, context.Component.LifeStyle);

            context = resolvingContextBuilder.BuildResolvingContext(typeof(IEnumerable<IService<int>>));

            Assert.NotNull(context);
            Assert.Equal(2, (context as ResolvingEnumerableContext).ElementsResolvingContext.Count);
        }
    }
}
