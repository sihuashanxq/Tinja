using System;
using System.Collections.Generic;
using System.Linq;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Descriptors;
using Tinja.Core.Injection.Descriptors.Internals;

namespace Tinja.Core.Injection.Descriptors
{
    public class ServiceDescriptorFactory : IServiceDescriptorFactory
    {
        private readonly SeviceIdFactory _serviceIdFactory;

        private readonly Dictionary<Type, List<ServiceEntry>> _registeredServices;

        internal ServiceDescriptorFactory()
        {
            _serviceIdFactory = new SeviceIdFactory();
            _registeredServices = new Dictionary<Type, List<ServiceEntry>>();
        }

        internal virtual void Populate(IDictionary<Type, List<Component>> components, IServiceResolver serviceResolver)
        {
            if (components == null)
            {
                throw new NullReferenceException(nameof(components));
            }

            if (serviceResolver == null)
            {
                throw new NullReferenceException(nameof(serviceResolver));
            }

            PopulateBegin(components, serviceResolver);
            PopulateEnd(serviceResolver);
        }

        protected virtual void PopulateBegin(IDictionary<Type, List<Component>> components, IServiceResolver serviceResolver)
        {
            foreach (var kv in components)
            {
                _registeredServices[kv.Key] = kv
                    .Value
                    .Select(item => new ServiceEntry(_serviceIdFactory.CreateSeviceId(), item))
                    .ToList();

                foreach (var item in _registeredServices[kv.Key].Where(item => item.ImplementationInstance != null))
                {
                    serviceResolver.Scope.ServiceRootScope.Factory.CreateService(item.ServiceId, resolver => item.ImplementationInstance);
                }
            }
        }

        protected virtual void PopulateEnd(IServiceResolver serviceResolver)
        {
            var factory = (IProxyTypeFactory)serviceResolver.Resolve(typeof(IProxyTypeFactory));

            foreach (var kv in _registeredServices)
            {
                foreach (var item in _registeredServices[kv.Key].Where(item => item.ImplementationType != null))
                {
                    var proxyImplementationType = factory?.CreateProxyType(item.ImplementationType);
                    if (proxyImplementationType != null)
                    {
                        item.ImplementationType = proxyImplementationType;
                        continue;
                    }

                    if (item.ImplementationType != null &&
                        item.ImplementationType.IsAbstract)
                    {
                        throw new InvalidOperationException($"ImplementationType:{item.ImplementationType.FullName} not can be Abstract when have not Interceptors!");
                    }

                    if (item.ImplementationType != null &&
                        item.ImplementationType.IsInterface)
                    {
                        throw new InvalidOperationException($"ImplementationType:{item.ImplementationType.FullName} not can be Interface when have not Interceptors!");
                    }
                }
            }
        }

        public virtual ServiceDescriptor Create(Type serviceType)
        {
            return
                CreateDirectly(serviceType) ??
                CreateGenerically(serviceType) ??
                CreateEnumerable(serviceType);
        }

        protected ServiceDescriptor Create(Type serviceType, ServiceEntry entry)
        {
            if (entry.ImplementationFactory != null)
            {
                return new ServiceDelegateDescriptor()
                {
                    ServiceType = serviceType,
                    LifeStyle = entry.LifeStyle,
                    ServiceId = entry.ServiceId,
                    Delegate = entry.ImplementationFactory
                };
            }

            if (entry.ImplementationInstance != null)
            {
                return new ServiceInstanceDescriptor()
                {
                    ServiceType = serviceType,
                    LifeStyle = entry.LifeStyle,
                    Instance = entry.ImplementationInstance,
                    ServiceId = entry.ServiceId
                };
            }

            return new ServiceConstrcutorDescriptor()
            {
                ServiceType = serviceType,
                LifeStyle = entry.LifeStyle,
                ServiceId = entry.ServiceId,
                ImplementationType = CreateGenericImplementationType(serviceType, entry.ImplementationType)
            };
        }

        protected virtual ServiceDescriptor CreateDirectly(Type serviceType)
        {
            if (_registeredServices.TryGetValue(serviceType, out var services))
            {
                var entry = services?.LastOrDefault();
                if (entry == null)
                {
                    return null;
                }

                return Create(serviceType, entry);
            }

            return null;
        }

        protected virtual ServiceDescriptor CreateGenerically(Type serviceType)
        {
            if (!serviceType.IsConstructedGenericType)
            {
                return null;
            }

            if (_registeredServices.TryGetValue(serviceType.GetGenericTypeDefinition(), out var entries))
            {
                var entry = entries?.LastOrDefault();
                if (entry == null)
                {
                    return null;
                }

                return Create(serviceType, entry);
            }

            return null;
        }

        protected virtual ServiceDescriptor CreateEnumerable(Type serviceType)
        {
            if (!serviceType.IsConstructedGenericType || serviceType.GetGenericTypeDefinition() != typeof(IEnumerable<>))
            {
                return null;
            }

            var elementType = serviceType.GenericTypeArguments.FirstOrDefault();
            var elements = CreateElements(elementType).Reverse().ToList();

            return new ServiceManyDescriptor()
            {
                Elements = elements,
                ServiceType = serviceType,
                ElementType = elementType,
                LifeStyle = ServiceLifeStyle.Transient
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
            elements.AddRange(CreateElementsGenerically(serviceType));

            return elements;
        }

        protected virtual IEnumerable<ServiceDescriptor> CreateElementsDirectly(Type serviceType)
        {
            if (_registeredServices.TryGetValue(serviceType, out var components))
            {
                return components.Select(i => Create(serviceType, i));
            }

            return ServiceDescriptor.EmptyDesciptors;
        }

        protected virtual IEnumerable<ServiceDescriptor> CreateElementsGenerically(Type serviceType)
        {
            if (!serviceType.IsConstructedGenericType)
            {
                return ServiceDescriptor.EmptyDesciptors;
            }

            if (_registeredServices.TryGetValue(serviceType.GetGenericTypeDefinition(), out var entries))
            {
                return entries.Select(i => Create(serviceType, i));
            }

            return ServiceDescriptor.EmptyDesciptors;
        }

        private static Type CreateGenericImplementationType(Type serviceType, Type implementationType)
        {
            if (serviceType.IsConstructedGenericType && implementationType.IsGenericTypeDefinition)
            {
                return implementationType.MakeGenericType(serviceType.GenericTypeArguments);
            }

            return implementationType;
        }
    }
}
