using System;
using System.Collections.Generic;
using Tinja.Abstractions;
using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Configurations;
using Tinja.Abstractions.Injection.Graphs;
using Tinja.Core.Injection;
using Tinja.Core.Injection.Configurations;
using Tinja.Core.Injection.Dependencies;

namespace Tinja.Core
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
                throw new ArgumentNullException(nameof(container));
            }

            var serviceEntryFactory = new ServiceEntryFactory();
            var configuration = container.BuildConfiguration(configurator);
            var serviceResolver = container.BuildServiceResolver(new GraphSiteBuilderFactory(serviceEntryFactory, configuration));
            if (serviceResolver == null)
            {
                throw new NullReferenceException(nameof(serviceResolver));
            }

            serviceEntryFactory.Initialize(container.ServiceDescriptors, serviceResolver);

            return serviceResolver;
        }

        internal static ServiceResolver BuildServiceResolver(this IContainer container, IGraphSiteBuilderFactory factory)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var serviceResolver = new ServiceResolver(factory);

            container.AddScoped<IServiceResolver>(resolver => null);
            container.AddScoped<IServiceLifeScope>(resolver => null);
            container.AddSingleton<IServiceLifeScopeFactory>(new ServiceLifeScopeFactory(serviceResolver));

            return serviceResolver;
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

        public static IContainer AddService(this IContainer container, Type serviceType, Type implementationType, ServiceLifeStyle lifeStyle, params string[] tags)
        {
            if (!implementationType.IsGenericTypeDefinition &&
                !implementationType.IsType(serviceType))
            {
                throw new ArgumentException($"type:{implementationType.FullName} can not cast to {serviceType.FullName}");
            }

            container.AddService(new ServiceDescriptor()
            {
                Tags = tags,
                LifeStyle = lifeStyle,
                ServiceType = serviceType,
                ImplementationType = implementationType
            });

            return container;
        }

        public static IContainer AddService<TType, TImpl>(this IContainer container, ServiceLifeStyle lifeStyle, params string[] tags)
          where TImpl : class, TType
        {
            return container.AddService(typeof(TType), typeof(TImpl), lifeStyle, tags);
        }

        public static IContainer AddService(this IContainer container, Type serviceType, Func<IServiceResolver, object> factory, ServiceLifeStyle lifeStyle, params string[] tags)
        {
            return container.AddService(new ServiceDescriptor()
            {
                Tags = tags,
                LifeStyle = lifeStyle,
                ServiceType = serviceType,
                ImplementationFactory = factory
            });
        }

        /// <summary>
        /// add a service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImple">ImplementationType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="factory">ImplementationFactory</param>
        /// <param name="lifeStyle"><see cref="ServiceLifeStyle"/></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static IContainer AddService<TType, TImple>(this IContainer container, Func<IServiceResolver, TImple> factory, ServiceLifeStyle lifeStyle, params string[] tags)
            where TImple : class, TType
        {
            return container.AddService(typeof(TType), factory, lifeStyle, tags);
        }

        private static IContainer AddService(this IContainer container, Type serviceType, object instance, ServiceLifeStyle lifeStyle, params string[] tags)
        {
            var implementationType = instance.GetType();
            if (!implementationType.IsType(serviceType))
            {
                throw new InvalidCastException($"type:{implementationType.FullName} can not cast to {serviceType.FullName}");
            }

            return container.AddService(new ServiceDescriptor()
            {
                Tags = tags,
                LifeStyle = lifeStyle,
                ServiceType = serviceType,
                ImplementationInstance = instance
            });
        }

        /// <summary>
        /// Add a service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="instance">ImplementationType</param>
        /// <param name="lifeStyle"><see cref="ServiceLifeStyle"/></param>
        /// <returns></returns>
        private static IContainer AddService<TType>(this IContainer container, object instance, ServiceLifeStyle lifeStyle, params string[] tags)
        {
            return container.AddService(typeof(TType), instance, lifeStyle, tags);
        }

        /// <summary>
        /// Add a singleton service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType</param>
        /// <param name="implementationType">ImplementationType</param>
        /// <returns></returns>
        public static IContainer AddSingleton(this IContainer container, Type serviceType, Type implementationType, params string[] tags)
        {
            return container.AddService(serviceType, implementationType, ServiceLifeStyle.Singleton, tags);
        }

        /// <summary>
        /// Add a singleton service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType and ImplementationType</param>
        /// <returns></returns>
        public static IContainer AddSingleton(this IContainer container, Type serviceType, params string[] tags)
        {
            return container.AddSingleton(serviceType, serviceType, tags);
        }

        /// <summary>
        /// Add a singleton service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImpl">ImplementationType</typeparam>
        /// <param name="container">Container</param>
        /// <returns></returns>
        public static IContainer AddSingleton<TType, TImpl>(this IContainer container, params string[] tags)
            where TImpl : class, TType
        {
            return container.AddSingleton(typeof(TType), typeof(TImpl), tags);
        }

        /// <summary>
        /// Add a singleton service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType and ImplementationType</typeparam>
        /// <param name="container">Container</param>
        /// <returns></returns>
        public static IContainer AddSingleton<TType>(this IContainer container, params string[] tags)
        {
            return container.AddSingleton(typeof(TType), typeof(TType), tags);
        }

        /// <summary>
        /// Add a singleton service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType</param>
        /// <param name="factory">ImplementationFactory</param>
        /// <returns></returns>
        public static IContainer AddSingleton(this IContainer container, Type serviceType, Func<IServiceResolver, object> factory, params string[] tags)
        {
            return container.AddService(serviceType, factory, ServiceLifeStyle.Singleton, tags);
        }

        /// <summary>
        /// Add a singleton service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="factory">ImplementationFactory</param>
        /// <returns></returns>
        public static IContainer AddSingleton<TType>(this IContainer container, Func<IServiceResolver, object> factory, params string[] tags)
        {
            return container.AddSingleton(typeof(TType), factory, tags);
        }

        /// <summary>
        /// Add a singleton service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImple">ImplementationType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="factory">ImplementationFactory</param>
        /// <returns></returns>
        public static IContainer AddSingleton<TType, TImple>(this IContainer container, Func<IServiceResolver, TImple> factory, params string[] tags)
            where TImple : class, TType
        {
            return container.AddSingleton(typeof(TType), factory, tags);
        }

        /// <summary>
        /// Add a singleton service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType</param>
        /// <param name="instance">ImplementationInstance</param>
        /// <returns></returns>
        public static IContainer AddSingleton(this IContainer container, Type serviceType, object instance, params string[] tags)
        {
            return container.AddService(serviceType, instance, ServiceLifeStyle.Singleton, tags);
        }

        /// <summary>
        /// Add a singleton service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="instance">ImplementationInstance</param>
        /// <returns></returns>
        public static IContainer AddSingleton<TType>(this IContainer container, object instance, params string[] tags)
        {
            return container.AddSingleton(typeof(TType), instance, tags);
        }

        /// <summary>
        /// Add a transient service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType</param>
        /// <param name="implementationType">ImplementationType</param>
        /// <returns></returns>
        public static IContainer AddTransient(this IContainer container, Type serviceType, Type implementationType, params string[] tags)
        {
            return container.AddService(serviceType, implementationType, ServiceLifeStyle.Transient, tags);
        }

        /// <summary>
        /// Add a transient service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType and ImplementationType</param>
        /// <returns></returns>
        public static IContainer AddTransient(this IContainer container, Type serviceType, params string[] tags)
        {
            return container.AddTransient(serviceType, serviceType, tags);
        }

        /// <summary>
        /// Add a transient service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImpl">ImplementationType</typeparam>
        /// <param name="container">Container</param>
        /// <returns></returns>
        public static IContainer AddTransient<TType, TImpl>(this IContainer container, params string[] tags)
            where TImpl : class, TType
        {
            return container.AddTransient(typeof(TType), typeof(TImpl), tags);
        }

        /// <summary>
        /// Add a transient service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType and ImplementationType</typeparam>
        /// <param name="container">Container</param>
        /// <returns></returns>
        public static IContainer AddTransient<TType>(this IContainer container, params string[] tags)
        {
            return container.AddTransient(typeof(TType), typeof(TType), tags);
        }

        /// <summary>
        /// Add a transient service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType</param>
        /// <param name="factory">ImplementationFactory</param>
        /// <returns></returns>
        public static IContainer AddTransient(this IContainer container, Type serviceType, Func<IServiceResolver, object> factory, params string[] tags)
        {
            return container.AddService(serviceType, factory, ServiceLifeStyle.Transient, tags);
        }

        /// <summary>
        /// Add a transient service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="factory">ImplementationFactory</param>
        /// <returns></returns>
        public static IContainer AddTransient<TType>(this IContainer container, Func<IServiceResolver, object> factory, params string[] tags)
        {
            return container.AddTransient(typeof(TType), factory, tags);
        }

        /// <summary>
        /// Add a transient service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImple">ImplementationType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="factory">ImplementationFactory</param>
        /// <returns></returns>
        public static IContainer AddTransient<TType, TImple>(this IContainer container, Func<IServiceResolver, TImple> factory, params string[] tags)
            where TImple : class, TType
        {
            return container.AddTransient(typeof(TType), factory, tags);
        }

        /// <summary>
        /// Add a scoped service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType</param>
        /// <param name="implementationType">ImplementationType</param>
        /// <returns></returns>
        public static IContainer AddScoped(this IContainer container, Type serviceType, Type implementationType, params string[] tags)
        {
            return container.AddService(serviceType, implementationType, ServiceLifeStyle.Scoped, tags);
        }

        /// <summary>
        /// Add a scoped service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType and ImplementationType</param>
        /// <returns></returns>
        public static IContainer AddScoped(this IContainer container, Type serviceType, params string[] tags)
        {
            return container.AddScoped(serviceType, serviceType, tags);
        }

        /// <summary>
        /// Add a scoped service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImpl">ImplementationType</typeparam>
        /// <param name="container">Container</param>
        /// <returns></returns>
        public static IContainer AddScoped<TType, TImpl>(this IContainer container, params string[] tags)
        {
            return container.AddScoped(typeof(TType), typeof(TImpl), tags);
        }

        /// <summary>
        /// Add a scoped service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType and ImplementationType</typeparam>
        /// <param name="container">Container</param>
        /// <returns></returns>
        public static IContainer AddScoped<TType>(this IContainer container, params string[] tags)
        {
            return container.AddScoped(typeof(TType), typeof(TType), tags);
        }

        /// <summary>
        /// Add a scoped service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType</param>
        /// <param name="factory">ImplementationFactory</param>
        /// <returns></returns>
        public static IContainer AddScoped(this IContainer container, Type serviceType, Func<IServiceResolver, object> factory, params string[] tags)
        {
            return container.AddService(serviceType, factory, ServiceLifeStyle.Scoped, tags);
        }

        /// <summary>
        /// Add a scoped service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="factory">ImplementationFactory</param>
        /// <returns></returns>
        public static IContainer AddScoped<TType>(this IContainer container, Func<IServiceResolver, object> factory, params string[] tags)
        {
            return container.AddScoped(typeof(TType), factory, tags);
        }

        /// <summary>
        /// Add a scoped service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImple">ImplementationType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="factory">ImplementationFactory</param>
        /// <returns></returns>
        public static IContainer AddScoped<TType, TImple>(this IContainer container, Func<IServiceResolver, TImple> factory, params string[] tags)
            where TImple : class, TType
        {
            return container.AddScoped(typeof(TType), factory, tags);
        }

        private static IContainer AddService(this IContainer container, ServiceDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            if (descriptor.ServiceType == null)
            {
                throw new ArgumentNullException(nameof(descriptor.ServiceType));
            }

            if (descriptor.LifeStyle != ServiceLifeStyle.Singleton &&
                descriptor.ImplementationInstance != null)
            {
                throw new ArgumentException($"ServiceType:{descriptor.ServiceType.FullName} ServiceLifeStyle must be Singleton when registered with and Implementation instance");
            }

            if (descriptor.ImplementationFactory == null &&
                descriptor.ImplementationType == null &&
                descriptor.ImplementationInstance == null)
            {
                throw new ArgumentException($"ServiceType:{descriptor.ServiceType.FullName} have not an Implementation!");
            }

            container.ServiceDescriptors.AddOrUpdate(descriptor.ServiceType, new List<ServiceDescriptor>() { descriptor }, (key, descriptors) =>
            {
                if (descriptors.Contains(descriptor))
                {
                    return descriptors;
                }

                lock (container.ServiceDescriptors)
                {
                    if (descriptors.Contains(descriptor))
                    {
                        return descriptors;
                    }

                    var found = false;

                    foreach (var item in descriptors)
                    {
                        if (item.ServiceType == descriptor.ServiceType)
                        {
                            if (item.ImplementationType == descriptor.ImplementationType &&
                                item.ImplementationType != null)
                            {
                                item.Tags = item.Tags;
                                item.LifeStyle = descriptor.LifeStyle;
                                found = true;
                                continue;
                            }

                            if (item.ImplementationFactory == descriptor.ImplementationFactory &&
                                item.ImplementationFactory != null)
                            {
                                item.Tags = item.Tags;
                                item.LifeStyle = descriptor.LifeStyle;
                                found = true;
                                continue;
                            }

                            if (item.ImplementationInstance == descriptor.ImplementationInstance &&
                                item.ImplementationInstance != null)
                            {
                                item.Tags = item.Tags;
                                item.LifeStyle = descriptor.LifeStyle;
                                found = true;
                                continue;
                            }
                        }
                    }

                    if (!found)
                    {
                        descriptors.Add(descriptor);
                    }

                    return descriptors;
                }
            });

            return container;
        }
    }
}
