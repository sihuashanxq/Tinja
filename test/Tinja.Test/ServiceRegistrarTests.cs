using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Tinja;
using Tinja.Registration;
using Xunit;

namespace TinjaTest
{
    public class ServiceRegistrarTests
    {
        [Fact]
        public void RegisterAsImplType()
        {
            var components = new ConcurrentDictionary<Type, List<Component>>();
            var serviceRegistrar = new ServiceRegistrar(components);

            var serviceType = typeof(IList<>);
            var implType = typeof(List<>);

            serviceRegistrar.Register(serviceType, implType, LifeStyle.Transient);

            Assert.Equal(1, components.GetValueOrDefault(serviceType)?.Count);

            //replace lifestyle
            serviceRegistrar.Register(serviceType, implType, LifeStyle.Singleton);

            Assert.Equal(1, components.GetValueOrDefault(serviceType)?.Count);

            Assert.Equal(
                LifeStyle.Singleton,
                components.GetValueOrDefault(serviceType)?.FirstOrDefault().LifeStyle);
        }

        [Fact]
        public void RegisterAsImplFacotry()
        {
            var components = new ConcurrentDictionary<Type, List<Component>>();
            var serviceRegistrar = new ServiceRegistrar(components);

            var serviceType = typeof(IList<>);

            object implFacotry(IContainer o) => new List<int>();

            serviceRegistrar.Register(serviceType, implFacotry, LifeStyle.Transient);

            Assert.Equal(1, components.GetValueOrDefault(serviceType)?.Count);

            //add new
            serviceRegistrar.Register(serviceType, (o) => new List<string>(), LifeStyle.Singleton);

            Assert.Equal(2, components.GetValueOrDefault(serviceType)?.Count);
        }
    }
}
