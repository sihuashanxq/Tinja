using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Tinja.Interception;

namespace Tinja
{
    public class Container : IContainer
    {
        public IServiceConfiguration Configuration { get; }

        public ConcurrentDictionary<Type, List<ServiceComponent>> Components { get; }

        public Container()
        {
            Configuration = new ServiceCongfiguration();
            Components = new ConcurrentDictionary<Type, List<ServiceComponent>>();
        }
    }

    public interface IServiceConfiguration
    {
        IInterceptionConfiguration Interception { get; }
    }

    public interface IInterceptionConfiguration
    {
        List<IMemberInterceptionProvider> Providers { get; }
    }

    public class ServiceCongfiguration : IServiceConfiguration
    {
        public IInterceptionConfiguration Interception { get; }

        public ServiceCongfiguration()
        {
            Interception = new InterceptionConfiguration();
        }
    }

    public class InterceptionConfiguration : IInterceptionConfiguration
    {
        public List<IMemberInterceptionProvider> Providers { get; }

        public InterceptionConfiguration()
        {
            Providers = new List<IMemberInterceptionProvider>();
        }
    }
}
