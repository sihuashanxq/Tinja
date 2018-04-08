﻿using System;
using System.Collections.Generic;
using Tinja.LifeStyle;
using Tinja.Resolving;
using Tinja.Resolving.Context;

namespace Tinja
{
    public static class ContainerExtensions
    {
        public static IServiceResolver BuildResolver(this IContainer ioc)
        {
            return new ServiceResolver(new ResolvingContextBuilder(ioc.Components));
        }

        public static IContainer AddService(this IContainer ioc, Type serviceType, Type implementionType, ServiceLifeStyle lifeStyle)
        {
            ioc.AddComponent(new Component()
            {
                LifeStyle = lifeStyle,
                ServiceType = serviceType,
                ImplementionType = implementionType
            });

            return ioc;
        }

        public static IContainer AddService(this IContainer ioc, Type serviceType, Func<IServiceResolver, object> factory, ServiceLifeStyle lifeStyle)
        {
            ioc.AddComponent(new Component()
            {
                LifeStyle = lifeStyle,
                ServiceType = serviceType,
                ImplementionFactory = factory
            });

            return ioc;
        }

        public static IContainer AddService<TType, TImpl>(this IContainer ioc, ServiceLifeStyle lifeStyle)
        {
            return ioc.AddService(typeof(TType), typeof(TImpl), lifeStyle);
        }

        public static IContainer AddSingleton(this IContainer ioc, Type serviceType, Type implementionType)
        {
            return ioc.AddService(serviceType, implementionType, ServiceLifeStyle.Singleton);
        }

        public static IContainer AddSingleton<TType, TImpl>(this IContainer ioc)
        {
            return ioc.AddService(typeof(TType), typeof(TImpl), ServiceLifeStyle.Singleton);
        }

        public static IContainer AddSingleton(this IContainer ioc, Type serviceType, Func<IServiceResolver, object> factory)
        {
            return ioc.AddService(serviceType, factory, ServiceLifeStyle.Singleton);
        }

        public static IContainer AddTransient(this IContainer ioc, Type serviceType, Type implementionType)
        {
            return ioc.AddService(serviceType, implementionType, ServiceLifeStyle.Transient);
        }

        public static IContainer AddTransient<TType, TImpl>(this IContainer ioc)
        {
            return ioc.AddService(typeof(TType), typeof(TImpl), ServiceLifeStyle.Transient);
        }

        public static IContainer AddTransient(this IContainer ioc, Type serviceType, Func<IServiceResolver, object> factory)
        {
            return ioc.AddService(serviceType, factory, ServiceLifeStyle.Transient);
        }


        public static IContainer AddScoped(this IContainer ioc, Type serviceType, Type implementionType)
        {
            return ioc.AddService(serviceType, implementionType, ServiceLifeStyle.Scoped);
        }

        public static IContainer AddScoped<TType, TImpl>(this IContainer ioc)
        {
            return ioc.AddService(typeof(TType), typeof(TImpl), ServiceLifeStyle.Scoped);
        }

        public static IContainer AddScoped(this IContainer ioc, Type serviceType, Func<IServiceResolver, object> factory)
        {
            return ioc.AddService(serviceType, factory, ServiceLifeStyle.Scoped);
        }

        internal static void AddComponent(this IContainer ioc, Component component)
        {
            var components = new List<Component>() { component };

            ioc.Components.AddOrUpdate(
                component.ServiceType,
                components,
                (k, v) =>
                {
                    if (v.Contains(component))
                    {
                        return v;
                    }

                    lock (ioc.Components)
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