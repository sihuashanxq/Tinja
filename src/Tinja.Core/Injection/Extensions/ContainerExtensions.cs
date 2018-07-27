﻿using System;
using System.Collections.Generic;
using Tinja.Abstractions;
using Tinja.Abstractions.Configuration;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Abstractions.DynamicProxy.Executors;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Activators;
using Tinja.Abstractions.Injection.Dependency;
using Tinja.Abstractions.Injection.Extensions;
using Tinja.Core.DynamicProxy;
using Tinja.Core.DynamicProxy.Executors;
using Tinja.Core.DynamicProxy.Executors.Internal;
using Tinja.Core.DynamicProxy.Metadatas;
using Tinja.Core.Injection.Activators;
using Tinja.Core.Injection.Dependency;
using Tinja.Core.Injection.Internals;

namespace Tinja.Core.Injection.Extensions
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
        /// <returns></returns>
        public static IServiceResolver BuildResolver(this IContainer container)
        {
            if (container == null)
            {
                throw new NullReferenceException(nameof(container));
            }

            var configuration = container.BuildConfiguration();

            var serviceLifeScopeFactory = new ServiceLifeScopeFactory();
            var serviceDescriptorFactory = new ServiceDescriptorFactory();
            var dependencyElementBuilderFactory = new CallDependencyElementBuilderFactory(serviceDescriptorFactory, configuration);

            var serviceActivatorFacotry = new ActivatorFactory(dependencyElementBuilderFactory);
            var serviceActivatorProvider = new ActivatorProvider(serviceActivatorFacotry);
            var serviceResolver = new ServiceResolver(serviceActivatorProvider, serviceLifeScopeFactory);

            container.AddSingleton<IServiceConfiguration>(configuration);
            container.AddSingleton<IActivatorFactory>(serviceActivatorFacotry);
            container.AddSingleton<IActivatorProvider>(serviceActivatorProvider);
            container.AddSingleton<IServiceLifeScopeFactory>(serviceLifeScopeFactory);
            container.AddSingleton<IProxyTypeFactory, ProxyTypeFactory>();
            container.AddSingleton<IMethodInvokerBuilder, MethodInvokerBuilder>();
            container.AddSingleton<IMemberMetadataProvider, MemberMetadataProvider>();
            container.AddSingleton<IMethodInvocationExecutor, MethodInvocationExecutor>();
            container.AddSingleton<IProxyTypeGenerationReferee, ProxyTypeGenerationReferee>();
            container.AddSingleton<IInterceptorSelectorProvider, InterceptorSelectorProvider>();
            container.AddSingleton<IObjectMethodExecutorProvider, ObjectMethodExecutorProvider>();
            container.AddSingleton<IInterceptorMetadataProvider, InterceptorMetadataProvider>();
            container.AddSingleton<IInterceptorMetadataCollector, InterceptorMetadataCollector>();

            container.AddScoped<IServiceResolver>(resolver => resolver);
            container.AddScoped<IServiceLifeScope>(resolver => resolver.ServiceLifeScope);
            container.AddScoped<IInterceptorFactory, InterceptorFactory>();

            container.AddTransient<IInterceptorAccessor, InterceptorAccessor>();

            serviceDescriptorFactory.Populate(container.Components, serviceResolver);

            return serviceResolver;
        }

        /// <summary>
        /// Configure Service
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="configurator">Service Configurator</param>
        /// <returns></returns>
        public static IContainer Configure(this IContainer container, Action<IServiceConfiguration> configurator)
        {
            if (container == null)
            {
                throw new NullReferenceException(nameof(container));
            }

            if (configurator == null)
            {
                throw new NullReferenceException(nameof(configurator));
            }

            container.Configurators.Add(configurator);

            return container;
        }

        /// <summary>
        /// Build IServiceConfiguration
        /// </summary>
        /// <param name="container">Container</param>
        /// <returns></returns>
        internal static IServiceConfiguration BuildConfiguration(this IContainer container)
        {
            var configuration = new ServiceCongfiguration();

            foreach (var configurator in container.Configurators)
            {
                configurator?.Invoke(configuration);
            }

            return configuration;
        }

        public static IContainer AddService(this IContainer container, Type serviceType, Type implementionType, ServiceLifeStyle lifeStyle = ServiceLifeStyle.Transient)
        {
            if (!implementionType.IsGenericTypeDefinition && !implementionType.Is(serviceType))
            {
                throw new InvalidCastException($"type:{implementionType.FullName} can not casted to {serviceType.FullName}");
            }

            container.AddService(new Component()
            {
                LifeStyle = lifeStyle,
                ServiceType = serviceType,
                ImplementionType = implementionType
            });

            return container;
        }

        /// <summary>
        /// Add a service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImpl">ImplementionType</typeparam>
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
        /// <param name="factory">ImplementionFactory</param>
        /// <param name="lifeStyle"><see cref="ServiceLifeStyle"/></param>
        /// <returns></returns>
        public static IContainer AddService(this IContainer container, Type serviceType, Func<IServiceResolver, object> factory, ServiceLifeStyle lifeStyle = ServiceLifeStyle.Transient)
        {
            container.AddService(new Component()
            {
                LifeStyle = lifeStyle,
                ServiceType = serviceType,
                ImplementionFactory = factory
            });

            return container;
        }

        /// <summary>
        /// Add a service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImple">ImplementionType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="factory">ImplementionFactory</param>
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
        /// <param name="instance">ImplementionInstance</param>
        /// <param name="lifeStyle"><see cref="ServiceLifeStyle"/></param>
        /// <returns></returns>
        private static IContainer AddService(this IContainer container, Type serviceType, object instance, ServiceLifeStyle lifeStyle = ServiceLifeStyle.Transient)
        {
            var implementionType = instance.GetType();
            if (!implementionType.Is(serviceType))
            {
                throw new InvalidCastException($"type:{implementionType.FullName} can not casted to {serviceType.FullName}");
            }

            container.AddService(new Component()
            {
                LifeStyle = lifeStyle,
                ServiceType = serviceType,
                ImplementionInstance = instance
            });

            return container;
        }

        /// <summary>
        /// Add a service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="instance">ImplementionType</param>
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
        /// <param name="implementionType">ImplementionType</param>
        /// <returns></returns>
        public static IContainer AddSingleton(this IContainer container, Type serviceType, Type implementionType)
        {
            return container.AddService(serviceType, implementionType, ServiceLifeStyle.Singleton);
        }

        /// <summary>
        /// Add a singleton service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType and ImplementionType</param>
        /// <returns></returns>
        public static IContainer AddSingleton(this IContainer container, Type serviceType)
        {
            return container.AddSingleton(serviceType, serviceType);
        }

        /// <summary>
        /// Add a singleton service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImpl">ImplementionType</typeparam>
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
        /// <typeparam name="TType">ServiceType and ImplementionType</typeparam>
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
        /// <param name="factory">ImplementionFactory</param>
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
        /// <param name="factory">ImplementionFactory</param>
        /// <returns></returns>
        public static IContainer AddSingleton<TType>(this IContainer container, Func<IServiceResolver, object> factory)
        {
            return container.AddSingleton(typeof(TType), factory);
        }

        /// <summary>
        /// Add a singleton service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImple">ImplementionType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="factory">ImplementionFactory</param>
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
        /// <param name="instance">ImplementionInstance</param>
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
        /// <param name="instance">ImplementionInstance</param>
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
        /// <param name="implementionType">ImplementionType</param>
        /// <returns></returns>
        public static IContainer AddTransient(this IContainer container, Type serviceType, Type implementionType)
        {
            return container.AddService(serviceType, implementionType);
        }

        /// <summary>
        /// Add a transient service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType and ImplementionType</param>
        /// <returns></returns>
        public static IContainer AddTransient(this IContainer container, Type serviceType)
        {
            return container.AddTransient(serviceType, serviceType);
        }

        /// <summary>
        /// Add a transient service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImpl">ImplementionType</typeparam>
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
        /// <typeparam name="TType">ServiceType and ImplementionType</typeparam>
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
        /// <param name="factory">ImplementionFactory</param>
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
        /// <param name="factory">ImplementionFactory</param>
        /// <returns></returns>
        public static IContainer AddTransient<TType>(this IContainer container, Func<IServiceResolver, object> factory)
        {
            return container.AddTransient(typeof(TType), factory);
        }

        /// <summary>
        /// Add a transient service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImple">ImplementionType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="factory">ImplementionFactory</param>
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
        /// <param name="implementionType">ImplementionType</param>
        /// <returns></returns>
        public static IContainer AddScoped(this IContainer container, Type serviceType, Type implementionType)
        {
            return container.AddService(serviceType, implementionType, ServiceLifeStyle.Scoped);
        }

        /// <summary>
        /// Add a scoped service to container
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType and ImplementionType</param>
        /// <returns></returns>
        public static IContainer AddScoped(this IContainer container, Type serviceType)
        {
            return container.AddScoped(serviceType, serviceType);
        }

        /// <summary>
        /// Add a scoped service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImpl">ImplementionType</typeparam>
        /// <param name="container">Container</param>
        /// <returns></returns>
        public static IContainer AddScoped<TType, TImpl>(this IContainer container)
        {
            return container.AddScoped(typeof(TType), typeof(TImpl));
        }

        /// <summary>
        /// Add a scoped service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType and ImplementionType</typeparam>
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
        /// <param name="factory">ImplementionFactory</param>
        /// <returns></returns>
        public static IContainer AddScoped(this IContainer container, Type serviceType, Func<IServiceResolver, object> factory)
        {
            return container.AddService(serviceType, factory, ServiceLifeStyle.Scoped);
        }

        /// <summary>
        /// Add a scoped service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImple">ImplementionType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="factory">ImplementionFactory</param>
        /// <returns></returns>
        public static IContainer AddScoped<TType>(this IContainer container, Func<IServiceResolver, object> factory)
        {
            return container.AddScoped(typeof(TType), factory);
        }

        /// <summary>
        /// Add a scoped service to container
        /// </summary>
        /// <typeparam name="TType">ServiceType</typeparam>
        /// <typeparam name="TImple">ImplementionType</typeparam>
        /// <param name="container">Container</param>
        /// <param name="factory">ImplementionFactory</param>
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
                component.ImplementionInstance != null)
            {
                throw new InvalidOperationException($"ServiceType:{component.ServiceType.FullName} ServiceLifeStyle must be Singleton when registered with and implemention instance");
            }

            if (component.ImplementionFactory == null &&
                component.ImplementionType == null &&
                component.ImplementionInstance == null)
            {
                throw new InvalidOperationException($"ServiceType:{component.ServiceType.FullName} have not an implemention!");
            }

            container.Components.AddOrUpdate(
                component.ServiceType,
                 new List<Component>() { component },
                (k, v) =>
                {
                    if (v.Contains(component))
                    {
                        return v;
                    }

                    lock (container.Components)
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
