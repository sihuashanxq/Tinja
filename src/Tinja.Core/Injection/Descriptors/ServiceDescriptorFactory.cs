using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Descriptors;

namespace Tinja.Core.Injection.Descriptors
{
    public class ServiceDescriptorFactory : IServiceDescriptorFactory
    {
        protected Dictionary<Type, List<Component>> Components { get; }

        internal ServiceDescriptorFactory()
        {
            Components = new Dictionary<Type, List<Component>>();
        }

        public virtual void Populate(ConcurrentDictionary<Type, List<Component>> components, IServiceResolver serviceResolver)
        {
            PopulateBegin(components, serviceResolver);
            PopulateEnd(serviceResolver);
        }

        protected virtual void PopulateBegin(ConcurrentDictionary<Type, List<Component>> components, IServiceResolver serviceResolver)
        {
            foreach (var kv in components)
            {
                Components[kv.Key] = kv.Value.Select(i => i.Clone()).ToList();

                foreach (var item in Components[kv.Key])
                {
                    if (item.ImplementationInstance != null)
                    {
                        serviceResolver.Scope.GetOrAddResolvedService(Tuple.Create(item.ServiceType, item.ImplementationInstance), item.LifeStyle, resolver => item.ImplementationInstance);
                    }
                }
            }
        }

        protected virtual void PopulateEnd(IServiceResolver serviceResolver)
        {
            var proxyTypeFactory = (IProxyTypeFactory)serviceResolver.Resolve(typeof(IProxyTypeFactory));
  
            foreach (var kv in Components)
            {
                foreach (var item in Components[kv.Key])
                {
                    if (item.ImplementationType == null)
                    {
                        continue;
                    }

                    var proxyType = proxyTypeFactory?.CreateProxyType(item.ImplementationType);
                    if (proxyType != null)
                    {
                        item.ImplementationType = proxyType;
                        continue;
                    }

                    if (item.ImplementationType != null && item.ImplementationType.IsAbstract)
                    {
                        throw new InvalidOperationException($"ImplementionType:{item.ImplementationType.FullName} not can be Abstract when have not Interceptors!");
                    }

                    if (item.ImplementationType != null && item.ImplementationType.IsInterface)
                    {
                        throw new InvalidOperationException($"ImplementionType:{item.ImplementationType.FullName} not can be Interface when have not Interceptors!");
                    }
                }
            }
        }

        public virtual ServiceDescriptor Create(Type serviceType)
        {
            return
                CreateDirectly(serviceType) ??
                CreateOpenGeneric(serviceType) ??
                CreateEnumerable(serviceType);
        }

        protected ServiceDescriptor Create(Type serviceType, Component component)
        {
            if (component.ImplementationFactory != null)
            {
                return new ServiceDelegateDescriptor()
                {
                    ServiceType = serviceType,
                    LifeStyle = component.LifeStyle,
                    Delegate = component.ImplementationFactory
                };
            }

            if (component.ImplementationInstance != null)
            {
                return new ServiceInstanceDescriptor()
                {
                    ServiceType = serviceType,
                    LifeStyle = component.LifeStyle,
                    Instance = component.ImplementationInstance
                };
            }

            return new ServiceConstrcutorDescriptor()
            {
                ServiceType = serviceType,
                LifeStyle = component.LifeStyle,
                ImplementationType = MakeGenericImplementionType(serviceType, component.ImplementationType)
            };
        }

        protected virtual ServiceDescriptor CreateDirectly(Type serviceType)
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

                return Create(serviceType, component);
            }

            return null;
        }

        protected virtual ServiceDescriptor CreateOpenGeneric(Type serviceType)
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
                    return Create(serviceType, component);
                }
            }

            return null;
        }

        protected virtual ServiceDescriptor CreateEnumerable(Type serviceType)
        {
            if (!serviceType.IsConstructedGenericType || serviceType.GetGenericTypeDefinition() != typeof(IEnumerable<>))
            {
                return null;
            }

            var component = new Component()
            {
                ServiceType = typeof(IEnumerable<>),
                ImplementationType = typeof(List<>).MakeGenericType(serviceType.GenericTypeArguments),
                LifeStyle = ServiceLifeStyle.Scoped
            };

            var elementType = serviceType.GenericTypeArguments.FirstOrDefault();
            var elements = CreateElements(elementType).Reverse().ToList();

            return new ServiceManyDescriptor()
            {
                ServiceType = serviceType,
                CollectionType = MakeGenericImplementionType(serviceType, component.ImplementationType),
                LifeStyle = component.LifeStyle,
                Elements = elements
            };
        }

        protected virtual IEnumerable<ServiceDescriptor> CreateElements(Type serviceType)
        {
            var elements = new List<ServiceDescriptor>();
            var enumerable = CreateEnumerable(serviceType);
            if (enumerable != null)
            {
                elements.Add(enumerable);
            }

            elements.AddRange(CreateElementsDirectly(serviceType));
            elements.AddRange(CreateElementsOpenGeneric(serviceType));

            return elements;
        }

        protected virtual IEnumerable<ServiceDescriptor> CreateElementsDirectly(Type serviceType)
        {
            if (Components.TryGetValue(serviceType, out var components))
            {
                return components.Select(i => Create(serviceType, i));
            }

            return ServiceDescriptor.EmptyDesciptors;
        }

        protected virtual IEnumerable<ServiceDescriptor> CreateElementsOpenGeneric(Type serviceType)
        {
            if (!serviceType.IsConstructedGenericType)
            {
                return ServiceDescriptor.EmptyDesciptors;
            }

            if (Components.TryGetValue(serviceType.GetGenericTypeDefinition(), out var components))
            {
                return components.Select(i => Create(serviceType, i));
            }

            return ServiceDescriptor.EmptyDesciptors;
        }

        private static Type MakeGenericImplementionType(Type serviceType, Type implementionType)
        {
            if (!implementionType.IsGenericTypeDefinition || !serviceType.IsConstructedGenericType)
            {
                return implementionType;
            }

            return implementionType.MakeGenericType(serviceType.GenericTypeArguments);
        }
    }
}
