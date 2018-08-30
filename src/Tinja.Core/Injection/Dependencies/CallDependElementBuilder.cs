using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Configurations;
using Tinja.Abstractions.Injection.Dependencies;
using Tinja.Abstractions.Injection.Dependencies.Elements;

namespace Tinja.Core.Injection.Dependencies
{
    /// <inheritdoc />
    public class CallDependElementBuilder : ICallDependElementBuilder
    {
        protected CallDependElementScope CallScope { get; set; }

        protected IInjectionConfiguration Configuration { get; }

        protected IServiceEntryFactory ServiceEntryFactory { get; set; }

        public CallDependElementBuilder(IServiceEntryFactory serviceEntryFactory, IInjectionConfiguration configuration)
        {
            CallScope = new CallDependElementScope();
            Configuration = configuration ?? throw new NullReferenceException(nameof(configuration));
            ServiceEntryFactory = serviceEntryFactory ?? throw new NullReferenceException(nameof(serviceEntryFactory));
        }

        public virtual CallDependElement Build(Type serviceType)
        {
            var entry = ServiceEntryFactory.CreateEntry(serviceType);
            if (entry == null)
            {
                return null;
            }

            return BuildElement(entry);
        }

        protected virtual CallDependElement BuildElement(ServiceEntry entry)
        {
            switch (entry)
            {
                case ServiceInstanceEntry instanceEntry:
                    return BuildInstanceElement(instanceEntry);

                case ServiceDelegateEntry delegateEntry:
                    return BuildDelegateElement(delegateEntry);

                case ServiceEnumerableEntry enumerableEntry:
                    return BuildManyElement(enumerableEntry);

                case ServiceTypeEntry typeEntry:
                    using (CallScope.Begin(typeEntry.ImplementationType))
                    {
                        return BuildTypeElement(typeEntry);
                    }

                default:
                    throw new InvalidOperationException();
            }
        }

        protected virtual CallDependElement BuildDelegateElement(ServiceDelegateEntry entry)
        {
            return new DelegateCallDependElement()
            {
                Delegate = entry.Delegate,
                LifeStyle = entry.LifeStyle,
                ServiceCacheId = entry.ServiceCacheId,
                ServiceType = entry.ServiceType
            };
        }

        protected virtual CallDependElement BuildInstanceElement(ServiceInstanceEntry entry)
        {
            return new InstanceCallDependElement()
            {
                Instance = entry.Instance,
                LifeStyle = entry.LifeStyle,
                ServiceType = entry.ServiceType,
                ServiceCacheId = entry.ServiceCacheId
            };
        }

        protected virtual CallDependElement BuildManyElement(ServiceEnumerableEntry entry)
        {
            var items = new List<CallDependElement>();

            foreach (var item in entry.Items)
            {
                var element = BuildElement(item);
                if (element == null)
                {
                    continue;
                }

                items.Add(element);
            }

            return new EnumerableCallDependElement()
            {
                Items = items.ToArray(),
                ItemType = entry.ItemType,
                LifeStyle = entry.LifeStyle,
                ServiceType = entry.ServiceType
            };
        }

        protected virtual CallDependElement BuildTypeElement(ServiceTypeEntry entry)
        {
            var parameters = new Dictionary<ParameterInfo, CallDependElement>();

            foreach (var item in entry.Constrcutors.OrderByDescending(i => i.GetParameters().Length))
            {
                var parameterInfos = item.GetParameters();
                if (parameterInfos.Any(parameterInfo => !SetParameterElement(parameterInfo, parameters)))
                {
                    parameters.Clear();
                    continue;
                }

                return SetPropertyElements(new TypeCallDependElement()
                {
                    Parameters = parameters,
                    ServiceCacheId = entry.ServiceCacheId,
                    LifeStyle = entry.LifeStyle,
                    ServiceType = entry.ServiceType,
                    ImplementionType = entry.ImplementationType,
                    ConstructorInfo = item
                });
            }

            return null;
        }

        protected TypeCallDependElement SetPropertyElements(TypeCallDependElement element)
        {
            if (element == null || !Configuration.EnablePropertyInjection)
            {
                return element;
            }

            var properties = new Dictionary<PropertyInfo, CallDependElement>();
            var propertyInfos = element.ImplementionType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var propertyInfo in propertyInfos)
            {
                if (!propertyInfo.CanWrite || !propertyInfo.CanRead)
                {
                    continue;
                }

                var injectAttribute = propertyInfo.GetCustomAttribute<InjectAttribute>();
                if (injectAttribute == null)
                {
                    continue;
                }

                if (SetPropertyElement(propertyInfo, properties))
                {
                    continue;
                }

                if (injectAttribute.Requrired)
                {
                    throw new ResolveRequiredPropertyFailedException(element.ImplementionType, CallScope.Clone(), propertyInfo);
                }
            }

            element.Properties = properties;

            return element;
        }

        protected bool SetPropertyElement(PropertyInfo propertyInfo, Dictionary<PropertyInfo, CallDependElement> properties)
        {
            if (propertyInfo == null)
            {
                throw new NullReferenceException(nameof(propertyInfo));
            }

            if (properties == null)
            {
                throw new NullReferenceException(nameof(properties));
            }

            var entry = ServiceEntryFactory.CreateEntry(propertyInfo.PropertyType);
            if (entry == null)
            {
                return false;
            }

            CheckCircularDependency(entry);

            var element = BuildElement(entry);
            if (element == null)
            {
                return false;
            }

            properties[propertyInfo] = element;

            return true;
        }

        protected bool SetParameterElement(ParameterInfo parameterInfo, Dictionary<ParameterInfo, CallDependElement> parameters)
        {
            if (parameterInfo == null)
            {
                throw new NullReferenceException(nameof(parameterInfo));
            }

            if (parameters == null)
            {
                throw new NullReferenceException(nameof(parameters));
            }

            var entry = ServiceEntryFactory.CreateEntry(parameterInfo.ParameterType);
            if (entry == null)
            {
                return false;
            }

            CheckCircularDependency(entry);

            var element = BuildElement(entry);
            if (element == null)
            {
                return false;
            }

            parameters[parameterInfo] = element;

            return true;
        }

        protected void CheckCircularDependency(ServiceEntry entry)
        {
            if (entry is ServiceTypeEntry typeEntry && CallScope.Contains(typeEntry.ImplementationType))
            {
                throw new CallCircularException(typeEntry.ImplementationType, CallScope.Clone(), $"type:{typeEntry.ImplementationType.FullName} exists circular dependencies!");
            }
        }
    }
}
