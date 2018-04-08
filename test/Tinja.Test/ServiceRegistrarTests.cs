using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Tinja;
using Tinja.LifeStyle;
using Tinja.Resolving;
using Xunit;

namespace TinjaTest
{
    public class ServiceRegistrarTests
    {
        [Fact]
        public void RegisterAsImplType()
        {
            var ioc = new Container();

            var serviceType = typeof(IList<>);
            var implType = typeof(List<>);

            ioc.AddService(serviceType, implType, ServiceLifeStyle.Transient);

            Assert.Equal(1, ioc.Components.GetValueOrDefault(serviceType)?.Count);

            //replace lifestyle
            ioc.AddService(serviceType, implType, ServiceLifeStyle.Singleton);

            Assert.Equal(1, ioc.Components.GetValueOrDefault(serviceType)?.Count);

            Assert.Equal(
                ServiceLifeStyle.Singleton,
                ioc.Components.GetValueOrDefault(serviceType)?.FirstOrDefault().LifeStyle);
        }

        [Fact]
        public void RegisterAsImplFacotry()
        {
            var ioc = new Container();

            var serviceType = typeof(IList<>);

            object implFacotry(IServiceResolver o) => new List<int>();

            ioc.AddService(serviceType, implFacotry, ServiceLifeStyle.Transient);

            Assert.Equal(1, ioc.Components.GetValueOrDefault(serviceType)?.Count);

            //add new
            ioc.AddService(serviceType, (o) => new List<string>(), ServiceLifeStyle.Singleton);

            Assert.Equal(2, ioc.Components.GetValueOrDefault(serviceType)?.Count);
        }
    }
}
