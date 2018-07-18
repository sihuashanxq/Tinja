using System;
using System.Collections.Generic;
using Tinja.Abstractions.Configuration;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.Injection.Activators;
using Tinja.Abstractions.Injection.Dependency;

namespace Tinja.Abstractions.Injection.Extensions
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
            var memberInterceptionCollector = new InterceptorDefinitionCollector(configuration.Interception, TypeMemberCollectorFactory.Default);
            var serviceContextFactory = new ServiceDescriptorFactory(memberInterceptionCollector);
            var elementBuilderFactory = new CallDependencyElementBuilderFactory(serviceContextFactory, configuration);
            var activatorFacotry = new ActivatorFactory(elementBuilderFactory);
            var activatorProvider = new ActivatorProvider(activatorFacotry);
            var serviceResolver = new ServiceResolver(activatorProvider, serviceLifeScopeFactory);

            container.AddTransient<IMemberInterceptorProvider, MemberInterceptorProvider>();

            container.AddScoped(typeof(IServiceResolver), resolver => resolver);
            container.AddScoped(typeof(IServiceLifeScope), resolver => resolver.ServiceLifeScope);

            container.AddSingleton<IActivatorFactory>(activatorFacotry);
            container.AddSingleton<IServiceConfiguration>(configuration);
            container.AddSingleton<IActivatorProvider>(activatorProvider);
            container.AddSingleton<IServiceDescriptorFactory>(serviceContextFactory);
            container.AddSingleton<IServiceLifeScopeFactory>(serviceLifeScopeFactory);
            container.AddSingleton<ITypeMemberCollectorFactory>(TypeMemberCollectorFactory.Default);
            container.AddSingleton<IInterceptorDefinitionCollector>(memberInterceptionCollector);
            container.AddSingleton<ICallDependencyElementBuilderFactory>(elementBuilderFactory);
            container.AddSingleton<IMethodInvokerBuilder, MethodInvokerBuilder>();
            container.AddSingleton<IInterceptorCollector, InterceptorCollector>();
            container.AddSingleton<IMethodInvocationExecutor, MethodInvocationExecutor>();
            container.AddSingleton<IObjectMethodExecutorProvider, ObjectMethodExecutorProvider>();
            container.AddSingleton<IInterceptorSelectorProvider, InterceptorSelectorProvider>();

            serviceContextFactory.Populate(container.Components, serviceResolver.ServiceLifeScope);

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

            container.AddComponent(new Component()
            {
                LifeStyle = lifeStyle,
                ServiceType = serviceType,
                ImplementionType = implementionType
            });

            return container;
        }

        /// <summary>
        /// Add Service Definition
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
        /// Add Service Definition
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType</param>
        /// <param name="factory">ImplementionFactory</param>
        /// <param name="lifeStyle"><see cref="ServiceLifeStyle"/></param>
        /// <returns></returns>
        public static IContainer AddService(this IContainer container, Type serviceType, Func<IServiceResolver, object> factory, ServiceLifeStyle lifeStyle = ServiceLifeStyle.Transient)
        {
            container.AddComponent(new Component()
            {
                LifeStyle = lifeStyle,
                ServiceType = serviceType,
                ImplementionFactory = factory
            });

            return container;
        }

        /// <summary>
        /// Add Service Definition
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
        /// Add Service Definition 
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

            container.AddComponent(new Component()
            {
                LifeStyle = lifeStyle,
                ServiceType = serviceType,
                ImplementionInstance = instance
            });

            return container;
        }

        /// <summary>
        /// Add Service Definition
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
        /// Add Singleton Service Defintion
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
        /// Add Singleton Service Defintion
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType and ImplementionType</param>
        /// <returns></returns>
        public static IContainer AddSingleton(this IContainer container, Type serviceType)
        {
            return container.AddSingleton(serviceType, serviceType);
        }

        /// <summary>
        /// Add Singleton Service Defintion
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
        /// Add Singleton Service Defintion
        /// </summary>
        /// <typeparam name="TType">ServiceType and ImplementionType</typeparam>
        /// <param name="container">Container</param>
        /// <returns></returns>
        public static IContainer AddSingleton<TType>(this IContainer container)
        {
            return container.AddSingleton(typeof(TType), typeof(TType));
        }

        /// <summary>
        /// Add Singleton Service Defintion
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
        /// Add Singleton Service Defintion
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
        /// Add Singleton Service Defintion
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
        /// Add Singleton Service Defintion
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
        /// Add Transient Service Definition
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
        /// Add Transient Service Definition
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType and ImplementionType</param>
        /// <returns></returns>
        public static IContainer AddTransient(this IContainer container, Type serviceType)
        {
            return container.AddTransient(serviceType, serviceType);
        }

        /// <summary>
        /// Add Transient Service Definition
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
        /// Add Transient Service Definition
        /// </summary>
        /// <typeparam name="TType">ServiceType and ImplementionType</typeparam>
        /// <param name="container">Container</param>
        /// <returns></returns>
        public static IContainer AddTransient<TType>(this IContainer container)
        {
            return container.AddTransient(typeof(TType), typeof(TType));
        }

        /// <summary>
        /// Add Transient Service Definition
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
        /// Add Transient Service Definition
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
        /// Add Scoped Service Definition
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
        /// Add Scoped Service Definition
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="serviceType">ServiceType and ImplementionType</param>
        /// <returns></returns>
        public static IContainer AddScoped(this IContainer container, Type serviceType)
        {
            return container.AddScoped(serviceType, serviceType);
        }

        /// <summary>
        /// Add Scoped Service Definition
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
        /// Add Scoped Service Definition
        /// </summary>
        /// <typeparam name="TType">ServiceType and ImplementionType</typeparam>
        /// <param name="container">Container</param>
        /// <returns></returns>
        public static IContainer AddScoped<TType>(this IContainer container)
        {
            return container.AddScoped(typeof(TType), typeof(TType));
        }

        /// <summary>
        /// Add Scoped Service Definition
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
        /// Add Scoped Service Definition
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

        internal static void AddComponent(this IContainer container, Component component)
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
