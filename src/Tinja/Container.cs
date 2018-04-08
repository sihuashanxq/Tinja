using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Tinja.LifeStyle;
using Tinja.Resolving.Activation;
using Tinja.Resolving.Chain;
using Tinja.Resolving.Context;
using Tinja.Resolving.Service;

namespace Tinja
{
    public class Container : IContainer
    {
        public ConcurrentDictionary<Type, List<Component>> Components { get; }

        public Container()
        {
            Components = new ConcurrentDictionary<Type, List<Component>>();
            Initialize();
        }

        protected void Initialize()
        {
            this.AddSingleton(typeof(IServiceInfoFactory), (resolver) => new ServiceInfoFactory());
            this.AddSingleton(typeof(IServiceActivationBuilder), (resolver) => new ServiceActivationBuilder());
            this.AddSingleton(typeof(ServiceChainBuilder), (resolver) =>
            {
                return new ServiceConstructorChainFactory(
                    resolver.Resolve<IServiceInfoFactory>(),
                    resolver.Resolve<IResolvingContextBuilder>()
                );
            });

            //just for definition,not call
            this.AddSingleton(typeof(IResolvingContextBuilder), (resolver) => new ResolvingContextBuilder(Components));
        }
    }
}
