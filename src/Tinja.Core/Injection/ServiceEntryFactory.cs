using System;
using System.Collections.Generic;
using System.Linq;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection
{
    public class ServiceEntryFactory : IServiceEntryFactory
    {
        private readonly ServiceIdProvider _serviceIdProvider;

        private readonly Dictionary<Type, List<ServiceDescriptor>> _descriptors;

        internal ServiceEntryFactory()
        {
            _serviceIdProvider = new ServiceIdProvider();
            _descriptors = new Dictionary<Type, List<ServiceDescriptor>>();
        }

        internal void Initialize(IDictionary<Type, List<ServiceDescriptor>> serviceDescriptors, IServiceResolver serviceResolver)
        {
            if (serviceDescriptors == null)
            {
                throw new ArgumentNullException(nameof(serviceDescriptors));
            }

            if (serviceResolver == null)
            {
                throw new ArgumentNullException(nameof(serviceResolver));
            }

            InitializeDescriptors(serviceDescriptors, serviceResolver);
        }

        private void InitializeDescriptors(IDictionary<Type, List<ServiceDescriptor>> components, IServiceResolver serviceResolver)
        {
            foreach (var item in components)
            {
                _descriptors[item.Key] = item.Value.Select(descriptor => descriptor.Clone()).ToList();
            }

            InitializeProxyImplementations(serviceResolver);
            ValidateImplementations();
        }

        private void ValidateImplementations()
        {
            foreach (var descriptor in _descriptors.Values.SelectMany(item => item).Where(item => item.ImplementationType != null))
            {
                if (descriptor.ImplementationType.IsAbstract)
                {
                    throw new InvalidOperationException($"ImplementationType:{descriptor.ImplementationType.FullName} not can be Abstract when have any Interceptors!");
                }
            }
        }

        private void InitializeProxyImplementations(IServiceResolver serviceResolver)
        {
            var proxyTypeFactory = serviceResolver.ResolveService<IProxyTypeFactory>();
            if (proxyTypeFactory != null)
            {
                foreach (var item in _descriptors.SelectMany(item => item.Value).Where(item => item.ImplementationType != null))
                {
                    var newImplementationType = proxyTypeFactory.CreateProxyType(item.ImplementationType);
                    if (newImplementationType != null)
                    {
                        item.ImplementationType = newImplementationType;
                    }
                }
            }
        }

        public virtual ServiceEntry CreateEntry(Type serviceType, string tag)
        {
            return
                CreateEntryDirectly(serviceType, tag) ??
                CreateEntryGenerically(serviceType, tag) ??
                CreateEnumerableEntry(serviceType, tag);
        }

        private ServiceEntry CreateEntry(Type serviceType, ServiceDescriptor descriptor, string tag)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            if (descriptor.ImplementationFactory != null)
            {
                return new ServiceDelegateEntry()
                {
                    Tag = tag,
                    ServiceType = serviceType,
                    LifeStyle = descriptor.LifeStyle,
                    Delegate = descriptor.ImplementationFactory,
                    ServiceId = GetServiceId(descriptor.ImplementationFactory, descriptor.LifeStyle)
                };
            }

            if (descriptor.ImplementationInstance != null)
            {
                return new ServiceInstanceEntry()
                {
                    Tag = tag,
                    ServiceType = serviceType,
                    LifeStyle = descriptor.LifeStyle,
                    Instance = descriptor.ImplementationInstance,
                    ServiceId = GetServiceId(descriptor.ImplementationInstance, descriptor.LifeStyle)
                };
            }

            var implementationType = CreateGenericImplementationType(serviceType, descriptor.ImplementationType);
            if (implementationType == null)
            {
                throw new NullReferenceException(nameof(implementationType));
            }

            if (implementationType.IsLazy())
            {
                return new ServiceLazyEntry()
                {
                    Tag = tag,
                    LifeStyle = descriptor.LifeStyle,
                    ServiceType = serviceType,
                    ImplementationType = implementationType,
                    ServiceId = GetServiceId(implementationType, descriptor.LifeStyle)
                };
            }

            return new ServiceTypeEntry()
            {
                Tag = tag,
                LifeStyle = descriptor.LifeStyle,
                ServiceType = serviceType,
                ImplementationType = implementationType,
                ServiceId = GetServiceId(implementationType, descriptor.LifeStyle)
            };
        }

        private ServiceEntry CreateEntryDirectly(Type serviceType, string tag)
        {
            if (_descriptors.TryGetValue(serviceType, out var descriptors))
            {
                var descriptor = descriptors?.LastOrDefault(item => tag == null || item.Tags.Contains(tag));
                if (descriptor == null)
                {
                    return null;
                }

                return CreateEntry(serviceType, descriptor, tag);
            }

            return null;
        }

        private ServiceEntry CreateEntryGenerically(Type serviceType, string tag)
        {
            if (!serviceType.IsConstructedGenericType)
            {
                return null;
            }

            if (_descriptors.TryGetValue(serviceType.GetGenericTypeDefinition(), out var descriptors))
            {
                var isLazy = serviceType.IsLazy();
                var descriptor = descriptors?.LastOrDefault(item => tag == null || isLazy || item.Tags.Contains(tag));
                if (descriptor == null)
                {
                    return null;
                }

                return CreateEntry(serviceType, descriptor, tag);
            }

            return null;
        }

        private ServiceEntry CreateEnumerableEntry(Type serviceType, string tag)
        {
            if (!serviceType.IsConstructedGenericType || serviceType.GetGenericTypeDefinition() != typeof(IEnumerable<>))
            {
                return null;
            }

            var elementType = serviceType.GenericTypeArguments.FirstOrDefault();
            var elements = CreateEnumerableElementEntry(elementType, tag).Reverse().ToList();

            return new ServiceEnumerableEntry()
            {
                Elements = elements,
                ServiceType = serviceType,
                ElementType = elementType,
                LifeStyle = ServiceLifeStyle.Transient
            };
        }

        private IEnumerable<ServiceEntry> CreateEnumerableElementEntry(Type serviceType, string tag)
        {
            var enumerableItems = new List<ServiceEntry>();
            var enumerableEntry = CreateEnumerableEntry(serviceType, tag);
            if (enumerableEntry != null)
            {
                enumerableItems.Add(enumerableEntry);
            }

            enumerableItems.AddRange(CreateEnumerableItemsEntryDirectly(serviceType, tag));
            enumerableItems.AddRange(CreateEnumerableItemsEntryGenerically(serviceType, tag));

            return enumerableItems;
        }

        private IEnumerable<ServiceEntry> CreateEnumerableItemsEntryDirectly(Type serviceType, string tag)
        {
            if (_descriptors.TryGetValue(serviceType, out var descriptors))
            {
                return descriptors
                    .Where(item => tag == null || item.Tags.Contains(tag))
                    .Select(i => CreateEntry(serviceType, i, tag));
            }

            return ServiceEntry.EmptyEntries;
        }

        private IEnumerable<ServiceEntry> CreateEnumerableItemsEntryGenerically(Type serviceType, string tag)
        {
            if (!serviceType.IsConstructedGenericType)
            {
                return ServiceEntry.EmptyEntries;
            }

            if (_descriptors.TryGetValue(serviceType.GetGenericTypeDefinition(), out var descriptors))
            {
                var isLazy = serviceType.IsLazy();

                return descriptors
                    .Where(item => tag == null || isLazy || item.Tags.Contains(tag))
                    .Select(i => CreateEntry(serviceType, i, tag));
            }

            return ServiceEntry.EmptyEntries;
        }

        private static Type CreateGenericImplementationType(Type serviceType, Type implementationType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }

            if (serviceType.IsConstructedGenericType && implementationType.IsGenericTypeDefinition)
            {
                return implementationType.MakeGenericType(serviceType.GenericTypeArguments);
            }

            return implementationType;
        }

        private int GetServiceId(object serviceImplementation, ServiceLifeStyle lifeStyle)
        {
            if (lifeStyle == ServiceLifeStyle.Transient)
            {
                return 0;
            }

            if (serviceImplementation == null)
            {
                throw new ArgumentNullException(nameof(serviceImplementation));
            }

            return _serviceIdProvider.GetServiceId(serviceImplementation);
        }
    }
}
