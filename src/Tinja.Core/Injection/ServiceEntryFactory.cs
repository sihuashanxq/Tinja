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
        private readonly ServiceCacheIdProvider _serviceCacheIdProvider;

        private readonly Dictionary<Type, List<ServiceDescriptor>> _descriptors;

        internal ServiceEntryFactory()
        {
            _serviceCacheIdProvider = new ServiceCacheIdProvider();
            _descriptors = new Dictionary<Type, List<ServiceDescriptor>>();
        }

        internal void Populate(IDictionary<Type, List<Component>> components, IServiceResolver serviceResolver)
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

        protected void PopulateBegin(IDictionary<Type, List<Component>> components, IServiceResolver serviceResolver)
        {
            foreach (var item in components)
            {
                _descriptors[item.Key] = item.Value.Select(component => new ServiceDescriptor(component)).ToList();
            }
        }

        protected void PopulateEnd(IServiceResolver serviceResolver)
        {
            var descriptors = _descriptors.SelectMany(item => item.Value).Where(item => item.ImplementationType != null).ToArray();
            if (descriptors.Length != 0)
            {
                ReplaceProxyImplementationType(serviceResolver, descriptors);
            }

            foreach (var item in descriptors)
            {
                if (item.ImplementationType.IsAbstract)
                {
                    throw new InvalidOperationException($"ImplementationType:{item.ImplementationType.FullName} not can be Abstract when have any Interceptors!");
                }

                if (item.ImplementationType.IsInterface)
                {
                    throw new InvalidOperationException($"ImplementationType:{item.ImplementationType.FullName} not can be Interface when have any Interceptors!");
                }
            }
        }

        private static void ReplaceProxyImplementationType(IServiceResolver serviceResolver, ServiceDescriptor[] descriptors)
        {
            var proxyTypeFactory = serviceResolver.ResolveService<IProxyTypeFactory>();
            if (proxyTypeFactory == null)
            {
                return;
            }

            foreach (var item in descriptors)
            {
                var proxyImplementationType = proxyTypeFactory.CreateProxyType(item.ImplementationType);
                if (proxyImplementationType != null)
                {
                    item.ImplementationType = proxyImplementationType;
                }
            }
        }

        public virtual ServiceEntry CreateEntry(Type serviceType)
        {
            return
                CreateEntryDirectly(serviceType) ??
                CreateEntryGenerically(serviceType) ??
                CreateEnumerableEntry(serviceType);
        }

        protected ServiceEntry CreateEntry(Type serviceType, ServiceDescriptor descriptor)
        {
            if (serviceType == null)
            {
                throw new NullReferenceException(nameof(serviceType));
            }

            if (descriptor == null)
            {
                throw new NullReferenceException(nameof(descriptor));
            }

            if (descriptor.ImplementationFactory != null)
            {
                return new ServiceDelegateEntry()
                {
                    ServiceType = serviceType,
                    LifeStyle = descriptor.LifeStyle,
                    Delegate = descriptor.ImplementationFactory,
                    ServiceCacheId = GetServiceCacheId(descriptor.ImplementationFactory, descriptor.LifeStyle)
                };
            }

            if (descriptor.ImplementationInstance != null)
            {
                return new ServiceInstanceEntry()
                {
                    ServiceType = serviceType,
                    LifeStyle = descriptor.LifeStyle,
                    Instance = descriptor.ImplementationInstance,
                    ServiceCacheId = GetServiceCacheId(descriptor.ImplementationInstance, descriptor.LifeStyle)
                };
            }

            var implementationType = CreateGenericImplementationType(serviceType, descriptor.ImplementationType);
            if (implementationType == null)
            {
                throw new NullReferenceException(nameof(implementationType));
            }

            return new ServiceTypeEntry()
            {
                ServiceType = serviceType,
                LifeStyle = descriptor.LifeStyle,
                ImplementationType = implementationType,
                ServiceCacheId = GetServiceCacheId(implementationType, descriptor.LifeStyle)
            };
        }

        protected virtual ServiceEntry CreateEntryDirectly(Type serviceType)
        {
            if (_descriptors.TryGetValue(serviceType, out var services))
            {
                var entry = services?.LastOrDefault();
                if (entry == null)
                {
                    return null;
                }

                return CreateEntry(serviceType, entry);
            }

            return null;
        }

        protected virtual ServiceEntry CreateEntryGenerically(Type serviceType)
        {
            if (!serviceType.IsConstructedGenericType)
            {
                return null;
            }

            if (_descriptors.TryGetValue(serviceType.GetGenericTypeDefinition(), out var descriptors))
            {
                var descriptor = descriptors?.LastOrDefault();
                if (descriptor == null)
                {
                    return null;
                }

                return CreateEntry(serviceType, descriptor);
            }

            return null;
        }

        protected virtual ServiceEntry CreateEnumerableEntry(Type serviceType)
        {
            if (!serviceType.IsConstructedGenericType || serviceType.GetGenericTypeDefinition() != typeof(IEnumerable<>))
            {
                return null;
            }

            var elementType = serviceType.GenericTypeArguments.FirstOrDefault();
            var elements = CreateEnumerableItemsEntry(elementType).Reverse().ToList();

            return new ServiceEnumerableEntry()
            {
                Items = elements,
                ServiceType = serviceType,
                ItemType = elementType,
                LifeStyle = ServiceLifeStyle.Transient
            };
        }

        protected virtual IEnumerable<ServiceEntry> CreateEnumerableItemsEntry(Type serviceType)
        {
            var enumerableItems = new List<ServiceEntry>();
            var enumerableEntry = CreateEnumerableEntry(serviceType);
            if (enumerableEntry != null)
            {
                enumerableItems.Add(enumerableEntry);
            }

            enumerableItems.AddRange(CreateEnumerableItemsEntryDirectly(serviceType));
            enumerableItems.AddRange(CreateEnumerableItemsEntryGenerically(serviceType));

            return enumerableItems;
        }

        protected virtual IEnumerable<ServiceEntry> CreateEnumerableItemsEntryDirectly(Type serviceType)
        {
            if (_descriptors.TryGetValue(serviceType, out var descriptors))
            {
                return descriptors.Select(i => CreateEntry(serviceType, i));
            }

            return ServiceEntry.EmptyEntries;
        }

        protected virtual IEnumerable<ServiceEntry> CreateEnumerableItemsEntryGenerically(Type serviceType)
        {
            if (!serviceType.IsConstructedGenericType)
            {
                return ServiceEntry.EmptyEntries;
            }

            if (_descriptors.TryGetValue(serviceType.GetGenericTypeDefinition(), out var descriptors))
            {
                return descriptors.Select(i => CreateEntry(serviceType, i));
            }

            return ServiceEntry.EmptyEntries;
        }

        private static Type CreateGenericImplementationType(Type serviceType, Type implementationType)
        {
            if (serviceType == null)
            {
                throw new NullReferenceException(nameof(serviceType));
            }

            if (implementationType == null)
            {
                throw new NullReferenceException(nameof(implementationType));
            }

            if (serviceType.IsConstructedGenericType && implementationType.IsGenericTypeDefinition)
            {
                return implementationType.MakeGenericType(serviceType.GenericTypeArguments);
            }

            return implementationType;
        }

        private int GetServiceCacheId(object serviceKey, ServiceLifeStyle lifeStyle)
        {
            if (lifeStyle == ServiceLifeStyle.Transient)
            {
                return 0;
            }

            if (serviceKey == null)
            {
                throw new NullReferenceException(nameof(serviceKey));
            }

            return _serviceCacheIdProvider.GetCacheId(serviceKey);
        }
    }
}
