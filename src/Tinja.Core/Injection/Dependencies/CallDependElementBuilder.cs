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
    /// <summary>
    /// the default implementation for <see cref="ICallDependencyElementBuilder" />
    /// </summary>
    public class CallDependElementBuilder : ICallDependencyElementBuilder
    {
        protected IInjectionConfiguration Configuration { get; }

        protected CallDependElementScope CallScope { get; set; }

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
                        return BuildConstrcutorElement(typeEntry);
                    }
            }

            throw new InvalidOperationException();
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
                var ele = BuildElement(item);
                if (ele == null)
                {
                    continue;
                }

                items.Add(ele);
            }

            return new EnumerableCallDependElement()
            {
                Items = items.ToArray(),
                LifeStyle = entry.LifeStyle,
                ItemType = entry.ItemType,
                ServiceType = entry.ServiceType,
            };
        }

        protected virtual CallDependElement BuildConstrcutorElement(ServiceTypeEntry entry)
        {
            var parameterElements = new Dictionary<ParameterInfo, CallDependElement>();

            foreach (var item in entry.Constrcutors.OrderByDescending(i => i.GetParameters().Length))
            {
                var parameterInfos = item.GetParameters();
                if (parameterInfos.Any(parameterInfo => !SetParameterElement(parameterInfo, parameterElements)))
                {
                    parameterElements.Clear();
                }

                if (parameterElements.Count != parameterInfos.Length)
                {
                    parameterElements.Clear();
                    continue;
                }

                var dependElement = new TypeCallDependElement()
                {
                    Parameters = parameterElements,
                    ServiceCacheId = entry.ServiceCacheId,
                    LifeStyle = entry.LifeStyle,
                    ServiceType = entry.ServiceType,
                    ImplementionType = entry.ImplementationType,
                    ConstructorInfo = item
                };

                SetPropertyElements(dependElement);

                return dependElement;
            }

            return null;
        }

        protected void SetPropertyElements(TypeCallDependElement dependElement)
        {
            if (dependElement == null || !Configuration.EnablePropertyInjection)
            {
                return;
            }

            var propertyInfos = dependElement
                .ImplementionType
                .GetTypeInfo()
                .DeclaredProperties
                .Where(i => i.CanRead && i.CanWrite && i.IsDefined(typeof(InjectAttribute)));

            var propertyElements = new Dictionary<PropertyInfo, CallDependElement>();

            foreach (var propertyInfo in propertyInfos)
            {
                SetPropertyElement(propertyInfo, propertyElements);
            }

            dependElement.Properties = propertyElements;
        }

        protected void SetPropertyElement(PropertyInfo propertyInfo, Dictionary<PropertyInfo, CallDependElement> propertyElements)
        {
            if (propertyInfo == null)
            {
                throw new NullReferenceException(nameof(propertyInfo));
            }

            if (propertyElements == null)
            {
                throw new NullReferenceException(nameof(propertyElements));
            }

            var entry = ServiceEntryFactory.CreateEntry(propertyInfo.PropertyType);
            if (entry != null)
            {
                ValidateCircularDependency(entry);

                var propertyElement = BuildElement(entry);
                if (propertyElement != null)
                {
                    propertyElements[propertyInfo] = propertyElement;
                }
            }
        }

        protected bool SetParameterElement(ParameterInfo parameterInfo, Dictionary<ParameterInfo, CallDependElement> parameterElements)
        {
            if (parameterInfo == null)
            {
                throw new NullReferenceException(nameof(parameterInfo));
            }

            if (parameterElements == null)
            {
                throw new NullReferenceException(nameof(parameterElements));
            }

            var entry = ServiceEntryFactory.CreateEntry(parameterInfo.ParameterType);
            if (entry == null)
            {
                return false;
            }

            ValidateCircularDependency(entry);

            var parameterElement = BuildElement(entry);
            if (parameterElement == null)
            {
                return false;
            }

            parameterElements[parameterInfo] = parameterElement;
            return true;
        }

        protected void ValidateCircularDependency(ServiceEntry entry)
        {
            if (entry is ServiceTypeEntry typeEntry && CallScope.Contains(typeEntry.ImplementationType))
            {
                throw new CallCircularException(typeEntry.ImplementationType, $"type:{typeEntry.ImplementationType.FullName} exists circular dependencies!");
            }
        }
    }
}
