using System;
using System.Collections.Generic;
using Tinja.Configuration;
using Tinja.Interception;
using Tinja.Interception.Executors;
using Tinja.Interception.Executors.Internal;
using Tinja.Interception.Members;
using Tinja.Resolving;
using Tinja.Resolving.Activation;
using Tinja.Resolving.Context;
using Tinja.Resolving.Dependency;
using Tinja.Resolving.Metadata;
using Tinja.ServiceLife;

namespace Tinja.Extensions
{
    public static class ContainerExtensions
    {
        public static IServiceResolver BuildResolver(this IContainer ioc)
        {
            if (ioc == null)
            {
                throw new NullReferenceException(nameof(ioc));
            }

            var configuration = ioc.BuildConfiguration();
            var serviceLifeScopeFactory = new ServiceLifeScopeFactory();
            var memberInterceptionCollector = new MemberInterceptionCollector(
                configuration.Interception,
                MemberCollectorFactory.Default
            );

            var serviceContextFactory = new ServiceContextFactory(
                new TypeMetadataFactory(),
                memberInterceptionCollector
            );

            var callDependencyBuilderFactory = new ServiceCallDependencyBuilderFactory(
                serviceContextFactory,
                configuration
            );

            var activatorFacotry = new ActivatorFactory(callDependencyBuilderFactory);
            var activatorProvider = new ActivatorProvider(activatorFacotry);

            ioc.AddScoped(typeof(IServiceResolver), resolver => resolver);
            ioc.AddScoped(typeof(IServiceLifeScope), resolver => resolver.ServiceLifeScope);

            ioc.AddSingleton(typeof(IActivatorBuilder), _ => activatorFacotry);
            ioc.AddSingleton(typeof(IServiceCallDependencyBuilderFactory), _ => callDependencyBuilderFactory);
            ioc.AddSingleton(typeof(IServiceConfiguration), _ => configuration);
            ioc.AddSingleton(typeof(IServiceContextFactory), _ => serviceContextFactory);
            ioc.AddSingleton(typeof(IServiceLifeScopeFactory), _ => serviceLifeScopeFactory);
            ioc.AddSingleton(typeof(IActivatorProvider), _ => activatorProvider);
            ioc.AddSingleton<IMethodInvocationExecutor, MethodInvocationExecutor>();
            ioc.AddSingleton<IMethodInvokerBuilder, MethodInvokerBuilder>();
            ioc.AddSingleton<IInterceptorCollector, InterceptorCollector>();
            ioc.AddSingleton<IObjectMethodExecutorProvider, ObjectMethodExecutorProvider>();
            ioc.AddSingleton(typeof(IMemberInterceptionCollector), _ => memberInterceptionCollector);
            ioc.AddSingleton(typeof(IMemberCollectorFactory), _ => MemberCollectorFactory.Default);

            serviceContextFactory.Initialize(ioc.Components);

            return new ServiceResolver(activatorProvider, serviceLifeScopeFactory);
        }

        public static IContainer Configure(this IContainer ioc, Action<IServiceConfiguration> configurator)
        {
            if (ioc == null)
            {
                throw new NullReferenceException(nameof(ioc));
            }

            if (configurator == null)
            {
                throw new NullReferenceException(nameof(configurator));
            }

            ioc.Configurators.Add(configurator);

            return ioc;
        }

        internal static IServiceConfiguration BuildConfiguration(this IContainer ioc)
        {
            var configuration = new ServiceCongfiguration();

            foreach (var configurator in ioc.Configurators)
            {
                if (configuration != null)
                {
                    configurator(configuration);
                }
            }

            return configuration;
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
            if (component == null)
            {
                throw new NullReferenceException(nameof(component));
            }

            if (component.ServiceType == null)
            {
                throw new InvalidOperationException("ServiceType is null!");
            }

            if (component.ImplementionFactory == null && component.ImplementionType == null)
            {
                throw new InvalidOperationException($"Type:{component.ServiceType.FullName} ImplementionType and ImplementionFactory is null!");
            }

            ioc.Components.AddOrUpdate(
                component.ServiceType,
                 new List<Component>() { component },
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
