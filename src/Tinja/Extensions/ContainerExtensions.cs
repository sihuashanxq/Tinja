using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Interception;
using Tinja.Interception.Generators;
using Tinja.Interception.Internal;
using Tinja.Interception.Members;
using Tinja.Resolving;
using Tinja.Resolving.Activation;
using Tinja.Resolving.Metadata;
using Tinja.ServiceLife;

namespace Tinja
{
    public static class ContainerExtensions
    {
        public static IServiceResolver BuildResolver(this IContainer ioc)
        {
            var builder = new ServiceResolvingContextBuilder(new TypeMetadataFactory());
            var serviceLifeScopeFactory = new ServiceLifeScopeFactory();
            var serviceActivatorProvider = new ServiceActivatorProvider(builder);

            ioc.AddScoped(typeof(IServiceResolver), resolver => resolver);
            ioc.AddScoped(typeof(IServiceLifeScope), resolver => resolver.ServiceLifeScope);

            ioc.AddSingleton(typeof(IServiceResolvingContextBuilder), _ => builder);
            ioc.AddSingleton(typeof(IServiceLifeScopeFactory), _ => serviceLifeScopeFactory);
            ioc.AddSingleton(typeof(IServiceActivatorProvider), _ => serviceActivatorProvider);

            ioc.AddSingleton<IMethodInvocationExecutor, MethodInvocationExecutor>();
            ioc.AddSingleton<IMethodInvokerBuilder, MethodInvokerBuilder>();
            ioc.AddSingleton<IInterceptorCollector, InterceptorCollector>();
            ioc.AddSingleton<IInterceptionTargetProvider, InterceptionTargetProvider>();
            ioc.AddSingleton<IObjectMethodExecutorProvider, ObjectMethodExecutorProvider>();
            ioc.AddSingleton(typeof(IMemberCollectorFactory), _ => MemberCollectorFactory.Default);

            builder.Initialize(ioc.Components);

            return new ServiceResolver(serviceActivatorProvider, serviceLifeScopeFactory);
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
            if (component.ImplementionType != null)
            {
                //if (component.ImplementionType.IsInterface || component.ImplementionType.IsAbstract)
                //{
                //    throw new NotImplementedException($"ImplementionType:{component.ImplementionType.FullName} is abstract or interface!");
                //}

                CreateImplementionProxyType(component);
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

        private static void CreateImplementionProxyType(Component component)
        {
            if (component.ImplementionType.IsSealed)
            {
                return;
            }

            if (ShouldCreateProxy(component.ServiceType) ||
                ShouldCreateProxy(component.ImplementionType))
            {
                component.ImplementionType = new ClassProxyTypeGenerator(component.ImplementionType).CreateProxyType();
            }
        }

        private static bool ShouldCreateProxy(Type typeInfo)
        {
            if (typeInfo.IsSealed)
            {
                return false;
            }

            if (typeInfo.GetCustomAttributes<InterceptorAttribute>(false).Any())
            {
                return true;
            };

            if (typeInfo.GetCustomAttributes<InterceptorAttribute>(true).Where(i => i.Inherited = true).Any())
            {
                return true;
            }

            foreach (var item in typeInfo
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(i => !i.IsFinal))
            {
                if (item.GetCustomAttributes<InterceptorAttribute>(false).Any())
                {
                    return true;
                };

                if (item.GetCustomAttributes<InterceptorAttribute>(true).Where(i => i.Inherited = true).Any())
                {
                    return true;
                }
            }

            foreach (var item in typeInfo
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (item.GetCustomAttributes<InterceptorAttribute>(false).Any())
                {
                    return true;
                };

                if (item.GetCustomAttributes<InterceptorAttribute>(true).Where(i => i.Inherited = true).Any())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
