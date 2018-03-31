using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Tinja.Registration
{
    public class ServiceRegistrar : IServiceRegistrar
    {
        private object _syncObject = new object();

        protected ConcurrentDictionary<Type, List<Component>> Components { get; }

        public ServiceRegistrar(ConcurrentDictionary<Type, List<Component>> components)
        {
            Components = components;
        }

        public void Register(Type serviceType, Type implType, LifeStyle lifeStyle)
        {
            Register(new Component()
            {
                LifeStyle = lifeStyle,
                ServiceType = serviceType,
                ImplementionType = implType
            });
        }

        public void Register(Type serviceType, Func<IContainer, object> implFacotry, LifeStyle lifeStyle)
        {
            Register(new Component()
            {
                LifeStyle = lifeStyle,
                ServiceType = serviceType,
                ImplementionFactory = implFacotry
            });
        }

        protected virtual void Register(Component component)
        {
            Components.AddOrUpdate(
                component.ServiceType,
                new List<Component>() { component },
                (k, v) =>
                {
                    if (v.Contains(component))
                    {
                        return v;
                    }

                    //may be not too much
                    lock (_syncObject)
                    {
                        if (v.Contains(component))
                        {
                            return v;
                        }

                        foreach (var item in v)
                        {
                            if (item.ServiceType == component.ServiceType &&
                                item.ImplementionType != null &&
                                item.ImplementionType == component.ImplementionType)
                            {
                                item.LifeStyle = component.LifeStyle;
                                return v;
                            }

                            if (item.ServiceType == component.ServiceType &&
                                item.ImplementionFactory != null &&
                                item.ImplementionFactory == component.ImplementionFactory)
                            {
                                item.LifeStyle = component.LifeStyle;
                                return v;
                            }
                        }

                        v.Add(component);
                        return v;
                    }
                }
            );
        }
    }
}
