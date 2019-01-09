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
    internal class CallDependElementBuilder : ICallDependElementBuilder
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

            return Build(entry);
        }

        protected virtual CallDependElement Build(ServiceEntry entry)
        {
            switch (entry)
            {
                case ServiceInstanceEntry instanceEntry:
                    return BuildInstance(instanceEntry);

                case ServiceDelegateEntry delegateEntry:
                    return BuildDelegate(delegateEntry);

                case ServiceEnumerableEntry enumerableEntry:
                    return BuildEnumerable(enumerableEntry);

                case ServiceTypeEntry typeEntry:
                    return BuildType(typeEntry);
            }

            throw new InvalidOperationException();
        }

        protected virtual CallDependElement BuildDelegate(ServiceDelegateEntry entry)
        {
            return new DelegateCallDependElement()
            {
                Delegate = entry.Delegate,
                LifeStyle = entry.LifeStyle,
                ServiceCacheId = entry.ServiceCacheId,
                ServiceType = entry.ServiceType
            };
        }

        protected virtual CallDependElement BuildInstance(ServiceInstanceEntry entry)
        {
            return new InstanceCallDependElement()
            {
                Instance = entry.Instance,
                LifeStyle = entry.LifeStyle,
                ServiceType = entry.ServiceType,
                ServiceCacheId = entry.ServiceCacheId
            };
        }

        protected virtual CallDependElement BuildEnumerable(ServiceEnumerableEntry entry)
        {
            var items = new List<CallDependElement>();

            foreach (var item in entry.Items)
            {
                var element = Build(item);
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
                ServiceType = entry.ServiceType,
                ServiceCacheId = entry.ServiceCacheId
            };
        }

        protected virtual CallDependElement BuildType(ServiceTypeEntry entry)
        {
            var callParamters = new Dictionary<ParameterInfo, CallDependElement>();
            var callDependElement = BuildTypeWithContainerProvider(entry, callParamters) ??
                                    BuildTypeWithContainerAndAnnotaionProvider(entry, callParamters) ??
                                    BuildTypeWithAnyProvider(entry, callParamters);

            if (callDependElement == null)
            {
                throw new InvalidOperationException($"Cannot match a valid constructor for type:{entry.ImplementationType.FullName}!");
            }

            using (CallScope.Begin(entry.ImplementationType))
            {
                SetProperties(callDependElement);
            }

            return callDependElement;
        }

        protected virtual TypeCallDependElement BuildTypeWithAnyProvider(ServiceTypeEntry entry, Dictionary<ParameterInfo, CallDependElement> parameters)
        {
            using (CallScope.Begin(entry.ImplementationType))
            {
                foreach (var item in entry.Constrcutors.OrderByDescending(i => i.GetParameters().Length))
                {
                    var paramterInfos = item.GetParameters();
                    if (paramterInfos.Any(parameterInfo
                        => !SetParameterWithContainer(parameterInfo, parameters) &&
                           !SetParameterWithValueProvider(item, parameterInfo, parameters) &&
                           !SetParameterWithDefaultValue(parameterInfo, parameters)))
                    {
                        parameters.Clear();
                        continue;
                    }

                    return new TypeCallDependElement()
                    {
                        Parameters = parameters,
                        ConstructorInfo = item,
                        LifeStyle = entry.LifeStyle,
                        ServiceType = entry.ServiceType,
                        ServiceCacheId = entry.ServiceCacheId,
                        ImplementationType = entry.ImplementationType
                    };
                }

                return null;
            }
        }

        protected virtual TypeCallDependElement BuildTypeWithContainerProvider(ServiceTypeEntry entry, Dictionary<ParameterInfo, CallDependElement> parameters)
        {
            using (CallScope.Begin(entry.ImplementationType))
            {
                foreach (var item in entry.Constrcutors.OrderByDescending(i => i.GetParameters().Length))
                {
                    var parameterInfos = item.GetParameters();
                    if (parameterInfos.Any(parameterInfo => !SetParameterWithContainer(parameterInfo, parameters)))
                    {
                        parameters.Clear();
                        continue;
                    }

                    return new TypeCallDependElement()
                    {
                        Parameters = parameters,
                        ConstructorInfo = item,
                        LifeStyle = entry.LifeStyle,
                        ServiceType = entry.ServiceType,
                        ServiceCacheId = entry.ServiceCacheId,
                        ImplementationType = entry.ImplementationType
                    };
                }

                return null;
            }
        }

        protected virtual TypeCallDependElement BuildTypeWithContainerAndAnnotaionProvider(ServiceTypeEntry entry, Dictionary<ParameterInfo, CallDependElement> parameters)
        {
            using (CallScope.Begin(entry.ImplementationType))
            {
                foreach (var item in entry.Constrcutors.OrderByDescending(i => i.GetParameters().Length))
                {
                    var paramterInfos = item.GetParameters();
                    if (paramterInfos.Any(parameterInfo
                        => !SetParameterWithContainer(parameterInfo, parameters) &&
                           !SetParameterWithValueProvider(item, parameterInfo, parameters)))
                    {
                        parameters.Clear();
                        continue;
                    }

                    return new TypeCallDependElement()
                    {
                        Parameters = parameters,
                        ConstructorInfo = item,
                        LifeStyle = entry.LifeStyle,
                        ServiceType = entry.ServiceType,
                        ServiceCacheId = entry.ServiceCacheId,
                        ImplementationType = entry.ImplementationType
                    };
                }

                return null;
            }
        }

        protected void SetProperties(TypeCallDependElement callDependElement)
        {
            if (callDependElement == null || !Configuration.EnablePropertyInjection)
            {
                return;
            }

            var propertyInfos = callDependElement.ImplementationType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(item => item.CanWrite && item.CanRead)
                .Where(item => item.GetCustomAttribute<InjectAttribute>() != null);

            foreach (var propertyInfo in propertyInfos)
            {
                try
                {
                    SetProperty(propertyInfo, callDependElement.Properties);
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
        }

        protected void SetProperty(PropertyInfo propertyInfo, Dictionary<PropertyInfo, CallDependElement> properties)
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

            var callDependElement = Build(entry);
            if (callDependElement != null)
            {
                properties[propertyInfo] = callDependElement;
            }
        }

        protected bool SetParameterWithContainer(ParameterInfo parameterInfo, Dictionary<ParameterInfo, CallDependElement> parameters)
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

            var element = Build(entry);
            if (element == null)
            {
                return false;
            }

            parameters[parameterInfo] = element;

            return true;
        }

        protected bool SetParameterWithDefaultValue(ParameterInfo parameterInfo, Dictionary<ParameterInfo, CallDependElement> parameters)
        {
            if (parameterInfo == null)
            {
                throw new NullReferenceException(nameof(parameterInfo));
            }

            if (parameters == null)
            {
                throw new NullReferenceException(nameof(parameters));
            }

            if (!parameterInfo.HasDefaultValue)
            {
                return false;
            }

            parameters[parameterInfo] = new ConstantCallDependElement()
            {
                Constant = parameterInfo.DefaultValue,
                LifeStyle = ServiceLifeStyle.Transient,
                ServiceType = parameterInfo.ParameterType
            };

            return true;
        }

        protected bool SetParameterWithValueProvider(ConstructorInfo constructorInfo, ParameterInfo parameterInfo, Dictionary<ParameterInfo, CallDependElement> parameters)
        {
            if (parameterInfo == null)
            {
                throw new NullReferenceException(nameof(parameterInfo));
            }

            if (parameters == null)
            {
                throw new NullReferenceException(nameof(parameters));
            }

            var valueProvider = parameterInfo.GetCustomAttribute<ValueProviderAttribute>();
            if (valueProvider == null)
            {
                return false;
            }

            parameters[parameterInfo] = new ValueProviderCallDependElement()
            {
                LifeStyle = ServiceLifeStyle.Transient,
                GetValue = r => valueProvider.GetValue(r, constructorInfo, parameterInfo),
                ServiceType = parameterInfo.ParameterType,
            };

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
