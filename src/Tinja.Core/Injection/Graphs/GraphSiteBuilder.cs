using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions;
using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Configurations;
using Tinja.Abstractions.Injection.Graphs;
using Tinja.Abstractions.Injection.Graphs.Sites;

namespace Tinja.Core.Injection.Dependencies
{
    /// <inheritdoc />
    internal class GraphSiteBuilder : IGraphSiteBuilder
    {
        protected GraphSiteScope CallScope { get; set; }

        protected IInjectionConfiguration Configuration { get; }

        protected IServiceEntryFactory ServiceEntryFactory { get; set; }

        public GraphSiteBuilder(IServiceEntryFactory serviceEntryFactory, IInjectionConfiguration configuration)
        {
            CallScope = new GraphSiteScope();
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            ServiceEntryFactory = serviceEntryFactory ?? throw new ArgumentNullException(nameof(serviceEntryFactory));
        }

        public virtual GraphSite Build(Type serviceType, string tag)
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

        protected virtual GraphSite Build(ServiceEntry entry)
        {
            switch (entry)
            {
                case ServiceInstanceEntry instanceEntry:
                    return BuildInstance(instanceEntry);

                case ServiceDelegateEntry delegateEntry:
                    return BuildDelegate(delegateEntry);

                case ServiceEnumerableEntry enumerableEntry:
                    return BuildEnumerable(enumerableEntry);

                case ServiceLazyEntry lazyEntry:
                    return BuildLazy(lazyEntry);

                case ServiceTypeEntry typeEntry:
                    return BuildType(typeEntry);
            }

            throw new InvalidOperationException();
        }

        protected virtual GraphSite BuildDelegate(ServiceDelegateEntry entry)
        {
            return new GraphDelegateSite()
            {
                Delegate = entry.Delegate,
                LifeStyle = entry.LifeStyle,
                ServiceId = entry.ServiceId,
                ServiceType = entry.ServiceType
            };
        }

        protected virtual GraphSite BuildInstance(ServiceInstanceEntry entry)
        {
            return new GraphInstanceSite()
            {
                Instance = entry.Instance,
                LifeStyle = entry.LifeStyle,
                ServiceType = entry.ServiceType,
                ServiceId = entry.ServiceId
            };
        }

        protected virtual GraphSite BuildEnumerable(ServiceEnumerableEntry entry)
        {
            var elements = new List<GraphSite>();

            foreach (var item in entry.Elements)
            {
                var element = Build(item);
                if (element != null)
                {
                    elements.Add(element);
                }
            }

            return new GraphEnumerableSite()
            {
                Elements = elements.ToArray(),
                ElementType = entry.ElementType,
                LifeStyle = entry.LifeStyle,
                ServiceType = entry.ServiceType,
                ServiceId = entry.ServiceId
            };
        }

        protected virtual GraphSite BuildType(ServiceTypeEntry entry)
        {
            var site = BuildType(entry, false) ?? BuildType(entry, true);
            if (site == null)
            {
                throw new InvalidOperationException($"Cannot match a constructor for type:{entry.ImplementationType.FullName}!");
            }

            return site;
        }

        protected virtual GraphSite BuildType(ServiceTypeEntry entry, bool useDefauftParameterValue)
        {
            var site = default(GraphTypeSite);

            using (CallScope.CreateScope(entry.ImplementationType))
            {
                foreach (var constructorInfo in entry.Constrcutors.OrderByDescending(i => i.GetParameters().Length))
                {
                    var parameterSites = BindConstructorParameters(constructorInfo, useDefauftParameterValue);
                    if (parameterSites == null)
                    {
                        continue;
                    }

                    site = new GraphTypeSite()
                    {
                        ParameterSites = parameterSites,
                        ConstructorInfo = constructorInfo,
                        LifeStyle = entry.LifeStyle,
                        ServiceType = entry.ServiceType,
                        ServiceId = entry.ServiceId,
                        ImplementationType = entry.ImplementationType
                    };

                    break;
                }
            }

            if (site != null)
            {
                using (CallScope.CreateScope(entry.ImplementationType))
                {
                    site.PropertySites = BindProperties(site);
                }
            }

            return site;
        }

        protected virtual GraphLazySite BuildLazy(ServiceLazyEntry entry)
        {
            foreach (var constructorInfo in entry.Constrcutors)
            {
                //new Lazy(()=>r=>r.ResolveService(T),true);
                var parameterInfos = constructorInfo.GetParameters();
                if (parameterInfos.Length != 1 ||
                    parameterInfos[0].ParameterType.IsNotType(typeof(Func<>)))
                {
                    continue;
                }

                using (CallScope.CreateTempScope(entry.ImplementationType))
                {
                    return new GraphLazySite()
                    {
                        Tag = entry.Tag,
                        LifeStyle = entry.LifeStyle,
                        ServiceId = entry.ServiceId,
                        ServiceType = entry.ServiceType,
                        ValueType = entry.ImplementationType.GenericTypeArguments[0],
                        ConstructorInfo = constructorInfo,
                        ImplementationType = entry.ImplementationType
                    };
                }
            }

            return null;
        }

        private Dictionary<ParameterInfo, GraphSite> BindConstructorParameters(ConstructorInfo constructorInfo, bool useDefaultValue)
        {
            var parameterSites = new Dictionary<ParameterInfo, GraphSite>();
            var parameterInfos = constructorInfo.GetParameters();

            foreach (var item in parameterInfos)
            {
                var parameterSite = BindConstructorParameter(constructorInfo, item, useDefaultValue);
                if (parameterSite == null)
                {
                    parameterSites.Clear();
                    break;
                }

                parameterSites[item] = parameterSite;
            }

            return parameterSites.Count == parameterInfos.Length ? parameterSites : null;
        }

        private GraphSite BindConstructorParameter(ConstructorInfo constructorInfo, ParameterInfo parameterInfo, bool useDefaultValue = false)
        {
            if (parameterInfo == null)
            {
                throw new ArgumentException(nameof(parameterInfo));
            }

            var tag = parameterInfo.GetCustomAttribute<TagAttribute>();
            var site = Build(parameterInfo.ParameterType, tag?.Value);
            if (site != null)
            {
                return site;
            }

            var provider = parameterInfo.GetCustomAttribute<ValueProviderAttribute>();
            if (provider != null)
            {
                return new GraphValueProviderSite()
                {
                    LifeStyle = ServiceLifeStyle.Transient,
                    Provider = r => provider.GetValue(r, parameterInfo),
                    ServiceType = parameterInfo.ParameterType,
                };
            }

            if (useDefaultValue && parameterInfo.HasDefaultValue)
            {
                return new GraphConstantSite()
                {
                    Constant = parameterInfo.DefaultValue,
                    LifeStyle = ServiceLifeStyle.Transient,
                    ServiceType = parameterInfo.ParameterType
                };
            }

            return null;
        }

        private Dictionary<PropertyInfo, GraphSite> BindProperties(GraphTypeSite site)
        {
            if (site == null || !Configuration.EnablePropertyInjection)
            {
                return null;
            }

            var propertySites = new Dictionary<PropertyInfo, GraphSite>();
            var propertyInfos = site.ImplementationType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(item => item.CanWrite && item.CanRead)
                .Where(item => item.GetCustomAttribute<InjectAttribute>() != null);

            foreach (var propertyInfo in propertyInfos)
            {
                try
                {
                    var propertySite = BindProperty(propertyInfo);
                    if (propertySite != null)
                    {
                        propertySites[propertyInfo] = propertySite;
                    }
                }
                catch (GraphCircularException)
                {
                    throw;
                }
                catch
                {
                    //skip error
                }
            }

            return propertySites;
        }

        private GraphSite BindProperty(PropertyInfo propertyInfo)
        {
            var tag = propertyInfo.GetCustomAttribute<TagAttribute>();
            var site = Build(propertyInfo.PropertyType, tag?.Value);
            if (site != null)
            {
                return site;
            }

            var provider = propertyInfo.GetCustomAttribute<ValueProviderAttribute>();
            if (provider != null)
            {
                return new GraphValueProviderSite()
                {
                    LifeStyle = ServiceLifeStyle.Transient,
                    Provider = r => provider.GetValue(r, propertyInfo),
                    ServiceType = propertyInfo.PropertyType
                };
            }

            return null;
        }

        private void CheckCircularDependency(ServiceTypeEntry typeEntry)
        {
            if (CallScope.Contains(typeEntry.ImplementationType))
            {
                throw new GraphCircularException(typeEntry.ImplementationType, CallScope.Clone(), $"type:{typeEntry.ImplementationType.FullName} exists circular dependencies!");
            }
        }
    }
}
