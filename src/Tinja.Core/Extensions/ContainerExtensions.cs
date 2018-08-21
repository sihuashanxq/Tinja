using System;
using System.Collections.Generic;
using Tinja.Abstractions;
using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Configurations;
using Tinja.Abstractions.Injection.Dependencies;
using Tinja.Core.Injection;
using Tinja.Core.Injection.Configurations;
using Tinja.Core.Injection.Dependencies;

namespace Tinja.Core.Extensions
{
    /// <summary>
    /// IContainer Extension Methods
    /// </summary>
    public static class ContainerExtensions
    {
        /// <summary>
        /// Build IServiceResolver
        /// </summary>
        /// <param name="container"></param>
        /// <param name="configurator"></param>
        /// <returns></returns>
        public static IServiceResolver BuildServiceResolver(this IContainer container, Action<IInjectionConfiguration> configurator = null)
        {
            if (container == null)
            {
                throw new NullReferenceException(nameof(container));
            }

            var serviceEntryFactory = new ServiceEntryFactory();
            var configuration = container.BuildConfiguration(configurator);

            var serviceResolver = container.BuildServiceResolver(new CallDependElementBuilderFactory(serviceEntryFactory, configuration));
            if (serviceResolver == null)
            {
                throw new NullReferenceException(nameof(serviceResolver));
            }

            serviceEntryFactory.Populate(container.Components, serviceResolver);

            return serviceResolver;
        }

        internal static IServiceResolver BuildServiceResolver(this IContainer container, ICallDependencyElementBuilderFactory factory)
        {
            if (container == null)
            {
                throw new NullReferenceException(nameof(container));
            }

            if (factory == null)
            {
                throw new NullReferenceException(nameof(factory));
            }

            container.AddScoped<IServiceResolver>(resolver => resolver);
            container.AddScoped<IServiceLifeScope>(resolver => resolver.Scope);

            return new ServiceResolver(factory);
        }

        /// <summary>
        /// Build IInjectionConfiguration
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="configurator"></param>
        /// <returns></returns>
        internal static IInjectionConfiguration BuildConfiguration(this IContainer container, Action<IInjectionConfiguration> configurator)
        {
            var configuration = new InjectionConfiguration();

            configurator?.Invoke(configuration);
            container.AddSingleton<IInjectionConfiguration>(configuration);

            return configuration;
        }

        public static IContainer AddService(this IContainer container, Type serviceType, Type implementationType, ServiceLifeStyle lifeStyle = ServiceLifeStyle.Transient)
        {
            if (!implementationType.IsGenericTypeDefinition && !implementationType.IsType(serviceType))
            {
                throw new InvalidCastException($"type:{implementationType.FullName} can not cast to {serviceType.FullName}");
            }

            container.AddService(new Component()
            {
                LifeStyle = lifeStyle,
                ServiceType = serviceType,
                ImplementationType = implementationType
            });

            return container;
        }

        /// <summary>
        /// Add a service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImpl">ImplementationType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="lifeStyle"><see cref="ServiceLifeStyle"/></param>
        /// <returns></returns>
        public static IContainer AddService<TType, TImpl>(this IContainer container, ServiceLifeStyle lifeStyle)
            where TImpl : class, TType
        {
            return container.AddService(typeof(TType), typeof(TImpl), lifeStyle);
        }

        /// <summary>
        /// Add a service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType</param>
        /// <param name="factory">ImplementationFactory</param>
        /// <param name="lifeStyle"><see cref="ServiceLifeStyle"/></param>
        /// <returns></returns>
        public static IContainer AddService(this IContainer container, Type serviceType, Func<IServiceResolver, object> factory, ServiceLifeStyle lifeStyle = ServiceLifeStyle.Transient)
        {
            container.AddService(new Component()
            {
                LifeStyle = lifeStyle,
                ServiceType = serviceType,
                ImplementationFactory = factory
            });

            return container;
        }

        /// <summary>
        /// Add a service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImple">ImplementationType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="factory">ImplementationFactory</param>
        /// <param name="lifeStyle"><see cref="ServiceLifeStyle"/></param>
        /// <returns></returns>
        public static IContainer AddService<TType, TImple>(this IContainer container, Func<IServiceResolver, TImple> factory, ServiceLifeStyle lifeStyle = ServiceLifeStyle.Transient)
            where TImple : class, TType
        {
            return container.AddService(typeof(TType), factory, lifeStyle);
        }

        /// <summary>
        /// Add a service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType</param>
        /// <param name="instance">ImplementationInstance</param>
        /// <param name="lifeStyle"><see cref="ServiceLifeStyle"/></param>
        /// <returns></returns>
        private static IContainer AddService(this IContainer container, Type serviceType, object instance, ServiceLifeStyle lifeStyle = ServiceLifeStyle.Transient)
        {
            var implementationType = instance.GetType();
            if (!implementationType.IsType(serviceType))
            {
                throw new InvalidCastException($"type:{implementationType.FullName} can not cast to {serviceType.FullName}");
            }

            container.AddService(new Component()
            {
                LifeStyle = lifeStyle,
                ServiceType = serviceType,
                ImplementationInstance = instance
            });

            return container;
        }

        /// <summary>
        /// Add a service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="instance">ImplementationType</param>
        /// <param name="lifeStyle"><see cref="ServiceLifeStyle"/></param>
        /// <returns></returns>
        private static IContainer AddService<TType>(this IContainer container, object instance, ServiceLifeStyle lifeStyle = ServiceLifeStyle.Transient)
        {
            return container.AddService(typeof(TType), instance, lifeStyle);
        }

        /// <summary>
        /// Add a singleton service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType</param>
        /// <param name="implementationType">ImplementationType</param>
        /// <returns></returns>
        public static IContainer AddSingleton(this IContainer container, Type serviceType, Type implementationType)
        {
            return container.AddService(serviceType, implementationType, ServiceLifeStyle.Singleton);
        }

        /// <summary>
        /// Add a singleton service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType and ImplementationType</param>
        /// <returns></returns>
        public static IContainer AddSingleton(this IContainer container, Type serviceType)
        {
            return container.AddSingleton(serviceType, serviceType);
        }

        /// <summary>
        /// Add a singleton service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImpl">ImplementationType</typeparam>
        /// <param name="container">Container</param>
        /// <returns></returns>
        public static IContainer AddSingleton<TType, TImpl>(this IContainer container)
            where TImpl : class, TType
        {
            return container.AddSingleton(typeof(TType), typeof(TImpl));
        }

        /// <summary>
        /// Add a singleton service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType and ImplementationType</typeparam>
        /// <param name="container">Container</param>
        /// <returns></returns>
        public static IContainer AddSingleton<TType>(this IContainer container)
        {
            return container.AddSingleton(typeof(TType), typeof(TType));
        }

        /// <summary>
        /// Add a singleton service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType</param>
        /// <param name="factory">ImplementationFactory</param>
        /// <returns></returns>
        public static IContainer AddSingleton(this IContainer container, Type serviceType, Func<IServiceResolver, object> factory)
        {
            return container.AddService(serviceType, factory, ServiceLifeStyle.Singleton);
        }

        /// <summary>
        /// Add a singleton service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="factory">ImplementationFactory</param>
        /// <returns></returns>
        public static IContainer AddSingleton<TType>(this IContainer container, Func<IServiceResolver, object> factory)
        {
            return container.AddSingleton(typeof(TType), factory);
        }

        /// <summary>
        /// Add a singleton service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImple">ImplementationType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="factory">ImplementationFactory</param>
        /// <returns></returns>
        public static IContainer AddSingleton<TType, TImple>(this IContainer container, Func<IServiceResolver, TImple> factory)
            where TImple : class, TType
        {
            return container.AddSingleton(typeof(TType), factory);
        }

        /// <summary>
        /// Add a singleton service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType</param>
        /// <param name="instance">ImplementationInstance</param>
        /// <returns></returns>
        public static IContainer AddSingleton(this IContainer container, Type serviceType, object instance)
        {
            return container.AddService(serviceType, instance, ServiceLifeStyle.Singleton);
        }

        /// <summary>
        /// Add a singleton service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="instance">ImplementationInstance</param>
        /// <returns></returns>
        public static IContainer AddSingleton<TType>(this IContainer container, object instance)
        {
            return container.AddSingleton(typeof(TType), instance);
        }

        /// <summary>
        /// Add a transient service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType</param>
        /// <param name="implementationType">ImplementationType</param>
        /// <returns></returns>
        public static IContainer AddTransient(this IContainer container, Type serviceType, Type implementationType)
        {
            return container.AddService(serviceType, implementationType);
        }

        /// <summary>
        /// Add a transient service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType and ImplementationType</param>
        /// <returns></returns>
        public static IContainer AddTransient(this IContainer container, Type serviceType)
        {
            return container.AddTransient(serviceType, serviceType);
        }

        /// <summary>
        /// Add a transient service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImpl">ImplementationType</typeparam>
        /// <param name="container">Container</param>
        /// <returns></returns>
        public static IContainer AddTransient<TType, TImpl>(this IContainer container)
            where TImpl : class, TType
        {
            return container.AddTransient(typeof(TType), typeof(TImpl));
        }

        /// <summary>
        /// Add a transient service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType and ImplementationType</typeparam>
        /// <param name="container">Container</param>
        /// <returns></returns>
        public static IContainer AddTransient<TType>(this IContainer container)
        {
            return container.AddTransient(typeof(TType), typeof(TType));
        }

        /// <summary>
        /// Add a transient service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType</param>
        /// <param name="factory">ImplementationFactory</param>
        /// <returns></returns>
        public static IContainer AddTransient(this IContainer container, Type serviceType, Func<IServiceResolver, object> factory)
        {
            return container.AddService(serviceType, factory);
        }

        /// <summary>
        /// Add a transient service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="factory">ImplementationFactory</param>
        /// <returns></returns>
        public static IContainer AddTransient<TType>(this IContainer container, Func<IServiceResolver, object> factory)
        {
            return container.AddTransient(typeof(TType), factory);
        }

        /// <summary>
        /// Add a transient service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImple">ImplementationType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="factory">ImplementationFactory</param>
        /// <returns></returns>
        public static IContainer AddTransient<TType, TImple>(this IContainer container, Func<IServiceResolver, TImple> factory)
            where TImple : class, TType
        {
            return container.AddTransient(typeof(TType), factory);
        }

        /// <summary>
        /// Add a scoped service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType</param>
        /// <param name="implementationType">ImplementationType</param>
        /// <returns></returns>
        public static IContainer AddScoped(this IContainer container, Type serviceType, Type implementationType)
        {
            return container.AddService(serviceType, implementationType, ServiceLifeStyle.Scoped);
        }

        /// <summary>
        /// Add a scoped service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType and ImplementationType</param>
        /// <returns></returns>
        public static IContainer AddScoped(this IContainer container, Type serviceType)
        {
            return container.AddScoped(serviceType, serviceType);
        }

        /// <summary>
        /// Add a scoped service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImpl">ImplementationType</typeparam>
        /// <param name="container">Container</param>
        /// <returns></returns>
        public static IContainer AddScoped<TType, TImpl>(this IContainer container)
        {
            return container.AddScoped(typeof(TType), typeof(TImpl));
        }

        /// <summary>
        /// Add a scoped service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType and ImplementationType</typeparam>
        /// <param name="container">Container</param>
        /// <returns></returns>
        public static IContainer AddScoped<TType>(this IContainer container)
        {
            return container.AddScoped(typeof(TType), typeof(TType));
        }

        /// <summary>
        /// Add a scoped service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType</param>
        /// <param name="factory">ImplementationFactory</param>
        /// <returns></returns>
        public static IContainer AddScoped(this IContainer container, Type serviceType, Func<IServiceResolver, object> factory)
        {
            return container.AddService(serviceType, factory, ServiceLifeStyle.Scoped);
        }

        /// <summary>
        /// Add a scoped service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="factory">ImplementationFactory</param>
        /// <returns></returns>
        public static IContainer AddScoped<TType>(this IContainer container, Func<IServiceResolver, object> factory)
        {
            return container.AddScoped(typeof(TType), factory);
        }

        /// <summary>
        /// Add a scoped service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImple">ImplementationType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="factory">ImplementationFactory</param>
        /// <returns></returns>
        public static IContainer AddScoped<TType, TImple>(this IContainer container, Func<IServiceResolver, TImple> factory)
            where TImple : class, TType
        {
            return container.AddScoped(typeof(TType), factory);
        }

        private static void AddService(this IContainer container, Component component)
        {
            if (component == null)
            {
                throw new NullReferenceException(nameof(component));
            }

            if (component.ServiceType == null)
            {
                throw new InvalidOperationException(nameof(component.ServiceType));
            }

            if (component.LifeStyle != ServiceLifeStyle.Singleton &&
                component.ImplementationInstance != null)
            {
                throw new InvalidOperationException($"ServiceType:{component.ServiceType.FullName} ServiceLifeStyle must be Singleton when registered with and Implementation instance");
            }

            if (component.ImplementationFactory == null &&
                component.ImplementationType == null &&
                component.ImplementationInstance == null)
            {
                throw new InvalidOperationException($"ServiceType:{component.ServiceType.FullName} have not an Implementation!");
            }

            container.Components.AddOrUpdate(component.ServiceType, new List<Component>() { component }, (key, components) =>
            {
                if (components.Contains(component))
                {
                    return components;
                }

                lock (container.Components)
                {
                    if (components.Contains(component))
                    {
                        return components;
                    }

                    foreach (var item in components)
                    {
                        if (item.ServiceType == component.ServiceType &&
                            item.ImplementationType != null &&
                            item.ImplementationType == component.ImplementationType)
                        {
                            item.LifeStyle = component.LifeStyle;
                            return components;
                        }

                        if (item.ServiceType == component.ServiceType &&
                            item.ImplementationFactory != null &&
                            item.ImplementationFactory == component.ImplementationFactory)
                        {
                            item.LifeStyle = component.LifeStyle;
                            return components;
                        }
                    }

                    components.Add(component);
                    return components;
                }
            });
        }
    }
}
