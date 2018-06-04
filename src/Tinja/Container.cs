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
        List<IMemberAddtionalInterceptionProvider> Providers { get; }
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
        public List<IMemberAddtionalInterceptionProvider> Providers { get; }

        public InterceptionConfiguration()
        {
            Providers = new List<IMemberAddtionalInterceptionProvider>();
        }
    }
}
