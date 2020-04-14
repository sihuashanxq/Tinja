using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions;
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
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            ServiceEntryFactory = serviceEntryFactory ?? throw new ArgumentNullException(nameof(serviceEntryFactory));
        }

        public virtual CallDependElement Build(Type serviceType)
        {
            return Build(serviceType, null);
        }

        public virtual CallDependElement Build(Type serviceType, string tag)
        {
            var entry = ServiceEntryFactory.CreateEntry(serviceType, tag);
            if (entry == null)
            {
                return null;
            }

            if (entry is ServiceTypeEntry typeEntry)
            {
                CheckCircularDependency(typeEntry);
            }

            return Build(entry);
        }

        private CallDependElement Build(ServiceEntry entry)
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

        private CallDependElement BuildTags(Type serviceType, BindTagAttribute bindTagAttribute)
        {
            if (bindTagAttribute != null && bindTagAttribute.Tags != null)
            {
                foreach (var tag in bindTagAttribute.Tags.Where(item => item != null))
                {
                    var callDependElement = Build(serviceType, tag);
                    if (callDependElement != null)
                    {
                        return callDependElement;
                    }
                }

                return null;
            }

            return Build(serviceType, null);
        }

        protected virtual CallDependElement BuildDelegate(ServiceDelegateEntry entry)
        {
            return new DelegateCallDependElement()
            {
                Delegate = entry.Delegate,
                LifeStyle = entry.LifeStyle,
                ServiceId = entry.ServiceCacheId,
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
                ServiceId = entry.ServiceCacheId
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
                ServiceId = entry.ServiceCacheId
            };
        }

        protected virtual CallDependElement BuildType(ServiceTypeEntry entry)
        {
            var callDependElement = BuildType(entry, false) ?? BuildType(entry, true);
            if (callDependElement == null)
            {
                throw new InvalidOperationException($"Cannot match a constructor for type:{entry.ImplementationType.FullName}!");
            }

            return callDependElement;
        }

        protected virtual TypeCallDependElement BuildType(ServiceTypeEntry entry, bool useDefauftParameterValue)
        {
            var callDependElement = default(TypeCallDependElement);

            using (CallScope.Begin(entry.ImplementationType))
            {
                foreach (var constructorInfo in entry.Constrcutors.OrderByDescending(i => i.GetParameters().Length))
                {
                    var parameterBindings = BindConstructorParameters(constructorInfo, useDefauftParameterValue);
                    if (parameterBindings == null)
                    {
                        continue;
                    }

                    callDependElement = new TypeCallDependElement()
                    {
                        ParameterBindings = parameterBindings,
                        ConstructorInfo = constructorInfo,
                        LifeStyle = entry.LifeStyle,
                        ServiceType = entry.ServiceType,
                        ServiceId = entry.ServiceCacheId,
                        ImplementationType = entry.ImplementationType
                    };

                    break;
                }
            }

            if (callDependElement != null)
            {
                using (CallScope.Begin(entry.ImplementationType))
                {
                    callDependElement.PropertyBindings = BindProperties(callDependElement);
                }
            }

            return callDependElement;
        }

        private Dictionary<ParameterInfo, CallDependElement> BindConstructorParameters(ConstructorInfo constructorInfo, bool useDefaultValue)
        {
            var parameterBindings = new Dictionary<ParameterInfo, CallDependElement>();
            var parameterInfos = constructorInfo.GetParameters();

            foreach (var item in parameterInfos)
            {
                var parameterBinding = BindConstructorParameter(constructorInfo, item, useDefaultValue);
                if (parameterBinding == null)
                {
                    parameterBindings.Clear();
                    break;
                }

                parameterBindings[item] = parameterBinding;
            }

            return parameterBindings.Count == parameterInfos.Length ? parameterBindings : null;
        }

        private CallDependElement BindConstructorParameter(ConstructorInfo constructorInfo, ParameterInfo parameterInfo, bool useDefaultValue = false)
        {
            if (parameterInfo == null)
            {
                throw new ArgumentException(nameof(parameterInfo));
            }

            var bindTagAttribute = parameterInfo.GetCustomAttribute<BindTagAttribute>();
            var callDependElement = BuildTags(parameterInfo.ParameterType, bindTagAttribute);
            if (callDependElement != null)
            {
                return callDependElement;
            }

            var providerAttribute = parameterInfo.GetCustomAttribute<ValueProviderAttribute>();
            if (providerAttribute != null)
            {
                return new ValueProviderCallDependElement()
                {
                    LifeStyle = ServiceLifeStyle.Transient,
                    Provider = r => providerAttribute.GetValue(r, parameterInfo),
                    ServiceType = parameterInfo.ParameterType,
                };
            }

            if (useDefaultValue && parameterInfo.HasDefaultValue)
            {
                return new ConstantCallDependElement()
                {
                    Constant = parameterInfo.DefaultValue,
                    LifeStyle = ServiceLifeStyle.Transient,
                    ServiceType = parameterInfo.ParameterType
                };
            }

            return null;
        }

        private Dictionary<PropertyInfo, CallDependElement> BindProperties(TypeCallDependElement callDependElement)
        {
            if (callDependElement == null || !Configuration.EnablePropertyInjection)
            {
                return null;
            }

            var propertyBindings = new Dictionary<PropertyInfo, CallDependElement>();
            var propertyInfos = callDependElement.ImplementationType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(item => item.CanWrite && item.CanRead)
                .Where(item => item.GetCustomAttribute<InjectAttribute>() != null);

            foreach (var propertyInfo in propertyInfos)
            {
                try
                {
                    var propertyBinding = BindProperty(propertyInfo);
                    if (propertyBinding != null)
                    {
                        propertyBindings[propertyInfo] = propertyBinding;
                    }
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

            return propertyBindings;
        }

        private CallDependElement BindProperty(PropertyInfo propertyInfo)
        {
            var bindTagAttribute = propertyInfo.GetCustomAttribute<BindTagAttribute>();
            var propertyBinding = BuildTags(propertyInfo.PropertyType, bindTagAttribute);
            if (propertyBinding != null)
            {
                return propertyBinding;
            }

            var providerAttribute = propertyInfo.GetCustomAttribute<ValueProviderAttribute>();
            if (providerAttribute != null)
            {
                return new ValueProviderCallDependElement()
                {
                    LifeStyle = ServiceLifeStyle.Transient,
                    Provider = r => providerAttribute.GetValue(r, propertyInfo),
                    ServiceType = propertyInfo.PropertyType
                };
            }

            return null;
        }

        private void CheckCircularDependency(ServiceTypeEntry typeEntry)
        {
            if (CallScope.Contains(typeEntry.ImplementationType))
            {
                throw new CallCircularException(typeEntry.ImplementationType, CallScope.Clone(), $"type:{typeEntry.ImplementationType.FullName} exists circular dependencies!");
            }
        }
    }
}
