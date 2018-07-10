using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Tinja.Interception;
using Tinja.Interception.Generators;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Context
{
    public class ServiceContextFactory : IServiceContextFactory
    {
        protected Dictionary<Type, List<Component>> Components { get; }

        internal IMemberInterceptionCollector InterceptionProvider { get; }

        internal ServiceContextFactory(IMemberInterceptionCollector provider)
        {
            InterceptionProvider = provider;
            Components = new Dictionary<Type, List<Component>>();
        }

        public virtual void Populate(ConcurrentDictionary<Type, List<Component>> components, IServiceLifeScope lifeScope)
        {
            foreach (var kv in components)
            {
                Components[kv.Key] = kv.Value.Select(i => i.Clone()).ToList();

                foreach (var item in Components[kv.Key])
                {
                    if (item.ImplementionInstance != null)
                    {
                        lifeScope.AddResolvedService(item.ImplementionInstance);
                    }

                    if (ShouldCreateProxyType(item))
                    {
                        InitializeProxyType(item);
                        continue;
                    }

                    if (item.ImplementionType != null && item.ImplementionType.IsAbstract)
                    {
                        throw new InvalidOperationException($"ImplementionType:{item.ImplementionType.FullName} not can be Abstract when have not Interceptors!");
                    }

                    if (item.ImplementionType != null && item.ImplementionType.IsInterface)
                    {
                        throw new InvalidOperationException($"ImplementionType:{item.ImplementionType.FullName} not can be Interface when have not Interceptors!");
                    }
                }
            }
        }

        protected virtual void InitializeProxyType(Component component)
        {
            if (component.ImplementionType.IsInterface)
            {
                component.ProxyType = new InterfaceProxyTypeGenerator(component.ImplementionType, InterceptionProvider).CreateProxyType();
            }
            else if (component.ImplementionType.IsAbstract)
            {
                component.ProxyType = new ClassProxyTypeGenerator(component.ImplementionType, InterceptionProvider).CreateProxyType();
            }
            else if (component.ServiceType.IsInterface)
            {
                component.ProxyType = new InterfaceTargetProxyTypeGenerator(component.ServiceType, component.ImplementionType, InterceptionProvider).CreateProxyType();
            }
            else
            {
                component.ProxyType = new ClassProxyTypeGenerator(component.ImplementionType, InterceptionProvider).CreateProxyType();
            }
        }

        public virtual ServiceContext CreateContext(Type serviceType)
        {
            return
                CreateContextDirectly(serviceType) ??
                CreateContextOpenGeneric(serviceType) ??
                CreateContextEnumerable(serviceType);
        }

        protected ServiceContext CreateContext(Type serviceType, Component component)
        {
            if (component.ImplementionFactory != null)
            {
                return new ServiceDelegateContext()
                {
                    ServiceType = serviceType,
                    LifeStyle = component.LifeStyle,
                    Delegate = component.ImplementionFactory
                };
            }

            if (component.ImplementionInstance != null)
            {
                return new ServiceInstanceContext()
                {
                    ServiceType = serviceType,
                    LifeStyle = component.LifeStyle,
                    Instance = component.ImplementionInstance
                };
            }

            if (component.ProxyType == null)
            {
                return new ServiceConstrcutorContext()
                {
                    ServiceType = serviceType,
                    LifeStyle = component.LifeStyle,
                    ImplementionType = MakeGenericImplementionType(serviceType, component.ImplementionType)
                };
            }

            return new ServiceProxyContext()
            {
                ServiceType = serviceType,
                LifeStyle = component.LifeStyle,
                TargetType = MakeGenericImplementionType(serviceType, component.ImplementionType),
                ProxyType = MakeGenericImplementionType(serviceType, component.ProxyType)
            };
        }

        protected virtual ServiceContext CreateContextDirectly(Type serviceType)
        {
            if (Components.TryGetValue(serviceType, out var components))
            {
                if (components == null)
                {
                    return null;
                }

                var component = components.LastOrDefault();
                if (component == null)
                {
                    return null;
                }

                return CreateContext(serviceType, component);
            }

            return null;
        }

        protected virtual ServiceContext CreateContextOpenGeneric(Type serviceType)
        {
            if (!serviceType.IsConstructedGenericType)
            {
                return null;
            }

            if (Components.TryGetValue(serviceType.GetGenericTypeDefinition(), out var components))
            {
                if (components == null)
                {
                    return null;
                }

                var component = components.LastOrDefault();
                if (component != null)
                {
                    return CreateContext(serviceType, component);
                }
            }

            return null;
        }

        protected virtual ServiceContext CreateContextEnumerable(Type serviceType)
        {
            if (!serviceType.IsConstructedGenericType || serviceType.GetGenericTypeDefinition() != typeof(IEnumerable<>))
            {
                return null;
            }

            var component = new Component()
            {
                ServiceType = typeof(IEnumerable<>),
                ImplementionType = typeof(List<>).MakeGenericType(serviceType.GenericTypeArguments),
                LifeStyle = ServiceLifeStyle.Scoped
            };

            var elementType = serviceType.GenericTypeArguments.FirstOrDefault();
            var elements = CreateContextElements(elementType).Reverse().ToList();

            return new ServiceManyContext()
            {
                ServiceType = serviceType,
                CollectionType = MakeGenericImplementionType(serviceType, component.ImplementionType),
                LifeStyle = component.LifeStyle,
                Elements = elements
            };
        }

        protected virtual IEnumerable<ServiceContext> CreateContextElements(Type serviceType)
        {
            var elements = new List<ServiceContext>();
            var enumerable = CreateContextEnumerable(serviceType);
            if (enumerable != null)
            {
                elements.Add(enumerable);
            }

            elements.AddRange(CreateContextElementsDirectly(serviceType));
            elements.AddRange(CreateContextElementsOpenGeneric(serviceType));

            return elements;
        }

        protected virtual IEnumerable<ServiceContext> CreateContextElementsDirectly(Type serviceType)
        {
            if (Components.TryGetValue(serviceType, out var components))
            {
                return components.Select(i => CreateContext(serviceType, i));
            }

            return ServiceContext.Empties;
        }

        protected virtual IEnumerable<ServiceContext> CreateContextElementsOpenGeneric(Type serviceType)
        {
            if (!serviceType.IsConstructedGenericType)
            {
                return ServiceContext.Empties;
            }

            if (Components.TryGetValue(serviceType.GetGenericTypeDefinition(), out var components))
            {
                return components.Select(i => CreateContext(serviceType, i));
            }

            return ServiceContext.Empties;
        }

        private static Type MakeGenericImplementionType(Type serviceType, Type implementionType)
        {
            if (!implementionType.IsGenericTypeDefinition || !serviceType.IsConstructedGenericType)
            {
                return implementionType;
            }

            return implementionType.MakeGenericType(serviceType.GenericTypeArguments);
        }

        private bool ShouldCreateProxyType(Component component)
        {
            if (component == null)
            {
                return false;
            }

            return component.ImplementionFactory == null &&
                   component.ImplementionInstance == null &&
                   InterceptionProvider.Collect(component.ServiceType, component.ImplementionType).Any();
        }
    }
}
