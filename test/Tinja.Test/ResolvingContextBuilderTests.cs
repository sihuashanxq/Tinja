using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Tinja.LifeStyle;
using Tinja.Resolving.Context;
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
            var ioc = new Container();
            var resolvingContextBuilder = new ResolvingContextBuilder();

            var serviceType = typeof(IService<>);
            var implType = typeof(Service<>);

            ioc.AddService(serviceType, implType, ServiceLifeStyle.Transient);

            var context = resolvingContextBuilder.BuildResolvingContext(typeof(IService<int>));

            Assert.NotNull(context);
        }

        [Fact]
        public void BuildGenericContext()
        {
            var ioc = new Container();
            var resolvingContextBuilder = new ResolvingContextBuilder();

            ioc.AddService(typeof(IService<>), typeof(Service<>), ServiceLifeStyle.Transient);
            ioc.AddService(typeof(IService<int>), typeof(Service<int>), ServiceLifeStyle.Singleton);

            var context = resolvingContextBuilder.BuildResolvingContext(typeof(IService<string>));
            Assert.NotNull(context);
            Assert.Equal(ServiceLifeStyle.Transient, context.Component.LifeStyle);

            context = resolvingContextBuilder.BuildResolvingContext(typeof(IService<int>));

            Assert.NotNull(context);
            Assert.Equal(ServiceLifeStyle.Singleton, context.Component.LifeStyle);

            context = resolvingContextBuilder.BuildResolvingContext(typeof(IEnumerable<IService<int>>));

            Assert.NotNull(context);
            Assert.Equal(2, (context as ResolvingEnumerableContext).ElementsResolvingContext.Count);
        }
    }
}
