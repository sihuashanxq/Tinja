using System;
using System.Collections.Generic;
using System.Linq;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection.Descriptors
{
    public class ServiceDescriptorFactory : IServiceDescriptorFactory
    {
        private int _currentServiceId = 1;

        private readonly Dictionary<object, int> _serviceIdMapping;

        private readonly Dictionary<Type, List<ServiceEntry>> _entries;

        internal ServiceDescriptorFactory()
        {
            _entries = new Dictionary<Type, List<ServiceEntry>>();
            _serviceIdMapping = new Dictionary<object, int>();
        }

        internal void Initialize(IDictionary<Type, List<ServiceEntry>> serviceDescriptors, IServiceResolver serviceResolver)
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

        private void InitializeDescriptors(IDictionary<Type, List<ServiceEntry>> components, IServiceResolver serviceResolver)
        {
            foreach (var item in components)
            {
                _entries[item.Key] = item.Value.Select(descriptor => descriptor.Clone()).ToList();
            }

            InitializeImplementationTypes(serviceResolver);
        }

        private void InitializeImplementationTypes(IServiceResolver serviceResolver)
        {
            var proxyTypeFactory = serviceResolver.ResolveService<IProxyTypeFactory>();

            foreach (var item in _entries.SelectMany(item => item.Value).Where(item => item.ImplementationType != null))
            {
                if (proxyTypeFactory != null)
                {
                    var newImplementationType = proxyTypeFactory.CreateProxyType(item.ImplementationType);
                    if (newImplementationType != null)
                    {
                        item.ImplementationType = newImplementationType;
                    }
                }

                if (item.ImplementationType.IsAbstract)
                {
                    throw new InvalidOperationException($"ImplementationType:{item.ImplementationType.FullName} not can be Abstract when have any Interceptors!");
                }
            }
        }

        public virtual ServiceDescriptor Create(Type serviceType, string tag, bool tagOptional)
        {
            return CreateCommon(serviceType, tag, tagOptional) ?? CreateEnumerable(serviceType, tag, tagOptional);
        }

        private ServiceDescriptor CreateCommon(Type serviceType, string tag, bool tagOptional)
        {
            var descriptor = CreateDescriptor(serviceType, serviceType, tag, tagOptional);
            if (descriptor != null)
            {
                return descriptor;
            }

            if (serviceType.IsConstructedGenericType)
            {
                return CreateDescriptor(serviceType, serviceType.GetGenericTypeDefinition(), tag, tagOptional);
            }

            return null;
        }

        private ServiceDescriptor CreateEnumerable(Type serviceType, string tag, bool tagOptional)
        {
            if (serviceType.IsConstructedGenericType == false ||
                serviceType.IsNotType(typeof(IEnumerable<>)))
            {
                return null;
            }

            var elementType = serviceType.GenericTypeArguments.FirstOrDefault();
            var elements = CreateEnumerableElements(elementType, tag, tagOptional).Reverse().ToList();

            return new ServiceEnumerableDescriptor()
            {
                Elements = elements,
                ServiceType = serviceType,
                ElementType = elementType,
                LifeStyle = ServiceLifeStyle.Transient
            };
        }

        private IEnumerable<ServiceDescriptor> CreateEnumerableElements(Type serviceType, string tag, bool tagOptional)
        {
            var enumerable = CreateEnumerable(serviceType, tag, tagOptional);
            if (enumerable != null)
            {
                yield return enumerable;
            }

            foreach (var item in CreateDescriptors(serviceType, serviceType, tag, tagOptional))
            {
                yield return item;
            }

            if (serviceType.IsConstructedGenericType)
            {
                foreach (var item in CreateDescriptors(serviceType, serviceType.GetGenericTypeDefinition(), tag, tagOptional))
                {
                    yield return item;
                }
            }
        }

        private ServiceDescriptor CreateDescriptor(Type serviceType, Type serviceDefine, string tag, bool tagOptional)
        {
            if (_entries.TryGetValue(serviceDefine, out var entries))
            {
                var entry = entries?.LastOrDefault(item => IsMatchTag(item, serviceDefine, tag, false));
                if (entry == null)
                {
                    if (tagOptional)
                    {
                        entry = entries?.LastOrDefault();
                    }

                    if (entry == null)
                    {
                        return null;
                    }
                }

                return CreateDescriptor(entry, serviceType, tag, tagOptional);
            }

            return null;
        }

        private ServiceDescriptor CreateDescriptor(ServiceEntry entry, Type serviceType, string tag, bool tagOptional)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            if (entry.ImplementationFactory != null)
            {
                return new ServiceDelegateDescriptor()
                {
                    ServiceType = serviceType,
                    LifeStyle = entry.LifeStyle,
                    Delegate = entry.ImplementationFactory,
                    ServiceId = GetServiceId(entry.ImplementationFactory, entry.LifeStyle)
                };
            }

            if (entry.ImplementationInstance != null)
            {
                return new ServiceInstanceDescriptor()
                {
                    ServiceType = serviceType,
                    LifeStyle = entry.LifeStyle,
                    Instance = entry.ImplementationInstance,
                    ServiceId = GetServiceId(entry.ImplementationInstance, entry.LifeStyle)
                };
            }

            var implementationType = CreateGenericImplementationType(serviceType, entry.ImplementationType);
            if (implementationType == null)
            {
                throw new NullReferenceException(nameof(implementationType));
            }

            if (implementationType.IsLazy())
            {
                return new ServiceLazyDescriptor()
                {
                    Tag = tag,
                    TagOptional = tagOptional,
                    LifeStyle = entry.LifeStyle,
                    ServiceType = serviceType,
                    ImplementationType = implementationType,
                    ServiceId = GetServiceId(implementationType, entry.LifeStyle)
                };
            }

            return new ServiceTypeDescriptor()
            {
                LifeStyle = entry.LifeStyle,
                ServiceType = serviceType,
                ImplementationType = implementationType,
                ServiceId = GetServiceId(implementationType, entry.LifeStyle)
            };
        }

        private IEnumerable<ServiceDescriptor> CreateDescriptors(Type serviceType, Type serviceTypeDefine, string tag, bool tagOptional)
        {
            if (_entries.TryGetValue(serviceTypeDefine, out var entries))
            {
                return entries
                    .Where(item => IsMatchTag(item, serviceType, tag, tagOptional))
                    .Select(item => CreateDescriptor(item, serviceType, tag, tagOptional));
            }

            return ServiceDescriptor.EmptyDescriptors;
        }

        private static bool IsMatchTag(ServiceEntry entry, Type serviceType, string tag, bool tagOptional)
        {
            if (string.IsNullOrEmpty(tag) || serviceType.IsLazy() || tagOptional)
            {
                return true;
            }

            return entry.Tags?.Contains(tag) ?? false;
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

        private int GetServiceId(object implementation, ServiceLifeStyle lifeStyle)
        {
            if (lifeStyle == ServiceLifeStyle.Transient)
            {
                return 0;
            }

            if (implementation == null)
            {
                throw new ArgumentNullException(nameof(implementation));
            }

            return GetServiceId(implementation);
        }

        internal int GetServiceId(object key)
        {
            lock (_serviceIdMapping)
            {
                if (_serviceIdMapping.TryGetValue(key, out var id))
                {
                    return id;
                }

                return _serviceIdMapping[key] = _currentServiceId++;
            }
        }
    }
}
