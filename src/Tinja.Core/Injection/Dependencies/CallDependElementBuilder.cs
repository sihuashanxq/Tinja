using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Configurations;
using Tinja.Abstractions.Injection.DataAnnotations;
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
                    return BuildEnumerableElement(enumerableEntry);

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

        protected virtual CallDependElement BuildEnumerableElement(ServiceEnumerableEntry entry)
        {
            var items = new List<CallDependElement>();

            foreach (var item in entry.Items)
            {
                var element = BuildElement(item);
                if (element != null)
                {
                    items.Add(element);
                }
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

            return BuildTypeElementWithValueProvider(entry);
        }

        protected virtual CallDependElement BuildTypeElementWithValueProvider(ServiceTypeEntry entry)
        {
            var parameters = new Dictionary<ParameterInfo, CallDependElement>();

            foreach (var item in entry.Constrcutors.OrderByDescending(i => i.GetParameters().Length))
            {
                foreach (var parameterInfo in item.GetParameters())
                {
                    if (SetParameterElement(parameterInfo, parameters))
                    {
                        continue;
                    }

                    var valueProvider = parameterInfo.GetCustomAttribute<ValuerProviderAttribute>();
                    if (valueProvider == null)
                    {
                        parameters.Clear();
                        break;
                    }

                    parameters[parameterInfo] = new ValueProviderCallDependElement()
                    {
                        //no cache
                        ServiceCacheId = 0,
                        GetValue = r => valueProvider.GetValue(r, item, parameterInfo),
                        ServiceType = parameterInfo.ParameterType,
                    };
                }

                if (parameters.Count == 0)
                {
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

            return BuildTypeElementWithDefaultValue(entry);
        }

        protected virtual CallDependElement BuildTypeElementWithDefaultValue(ServiceTypeEntry entry)
        {
            var parameters = new Dictionary<ParameterInfo, CallDependElement>();

            foreach (var item in entry.Constrcutors.OrderByDescending(i => i.GetParameters().Length))
            {
                foreach (var parameterInfo in item.GetParameters())
                {
                    if (SetParameterElement(parameterInfo, parameters))
                    {
                        continue;
                    }

                    if (!parameterInfo.HasDefaultValue)
                    {
                        parameters.Clear();
                        break;
                    }

                    parameters[parameterInfo] = new ConstantCallDependElement()
                    {
                        //no cache
                        ServiceCacheId = 0,
                        Constant = parameterInfo.DefaultValue,
                        ServiceType = parameterInfo.ParameterType,
                    };
                }

                if (parameters.Count == 0)
                {
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

            throw new InvalidOperationException($"Cannot match a valid constructor for type:{entry.ImplementationType.FullName}!");
        }

        protected TypeCallDependElement SetPropertyElements(TypeCallDependElement element)
        {
            if (element == null || !Configuration.EnablePropertyInjection)
            {
                return element;
            }

            var properties = new Dictionary<PropertyInfo, CallDependElement>();
            var propertyInfos = element
                .ImplementionType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(item => item.CanWrite && item.CanRead)
                .Where(item => item.GetCustomAttribute<InjectAttribute>() != null);

            foreach (var propertyInfo in propertyInfos)
            {
                try
                {
                    SetPropertyElement(propertyInfo, properties);
                }
                catch (CallCircularException)
                {
                    throw;
                }
                catch
                {
                    //skip error
                }
            }

            if (properties.Count != 0)
            {
                element.Properties = properties;
            }

            return element;
        }

        protected void SetPropertyElement(PropertyInfo propertyInfo, Dictionary<PropertyInfo, CallDependElement> properties)
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
                return;
            }

            if (entry is ServiceTypeEntry typeEntry)
            {
                CheckCircularDependency(typeEntry);
            }

            var element = BuildElement(entry);
            if (element != null)
            {
                properties[propertyInfo] = element;
            }
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

            if (entry is ServiceTypeEntry typeEntry)
            {
                CheckCircularDependency(typeEntry);
            }

            var element = BuildElement(entry);
            if (element == null)
            {
                return false;
            }

            parameters[parameterInfo] = element;

            return true;
        }

        protected void CheckCircularDependency(ServiceTypeEntry typeEntry)
        {
            if (CallScope.Contains(typeEntry.ImplementationType))
            {
                throw new CallCircularException(typeEntry.ImplementationType, CallScope.Clone(), $"type:{typeEntry.ImplementationType.FullName} exists circular dependencies!");
            }
        }
    }
}
