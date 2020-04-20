using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Configurations;
using Tinja.Abstractions.Injection.Graphs;
using Tinja.Abstractions.Injection.Graphs.Sites;
using Tinja.Core.Injection.Descriptors;

namespace Tinja.Core.Injection.Graphs
{
    /// <inheritdoc />
    internal class GraphSiteBuilder : IGraphSiteBuilder
    {
        protected GraphSiteScope CallScope { get; set; }

        protected IInjectionConfiguration Configuration { get; }

        protected IServiceDescriptorFactory DescriptorFactory { get; set; }

        public GraphSiteBuilder(IServiceDescriptorFactory descriptorFactory, IInjectionConfiguration configuration)
        {
            CallScope = new GraphSiteScope();
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            DescriptorFactory = descriptorFactory ?? throw new ArgumentNullException(nameof(descriptorFactory));
        }

        public virtual GraphSite Build(Type serviceType, string tag, bool tagOptional)
        {
            var descriptor = DescriptorFactory.Create(serviceType, tag, tagOptional);
            if (descriptor != null)
            {
                return Build(descriptor);
            }

            return null;
        }

        public virtual GraphSite Build(Type serviceType, TagAttribute tagAttribute)
        {
            return Build(serviceType, tagAttribute?.Value, tagAttribute?.Optional ?? false);
        }

        protected virtual GraphSite Build(ServiceDescriptor descriptor)
        {
            switch (descriptor)
            {
                case ServiceLazyDescriptor lazyDescriptor:
                    return BuildLazy(lazyDescriptor);

                case ServiceTypeDescriptor typeDescriptor:
                    return BuildType(typeDescriptor);

                case ServiceInstanceDescriptor instanceDescriptor:
                    return BuildInstance(instanceDescriptor);

                case ServiceDelegateDescriptor delegateDescriptor:
                    return BuildDelegate(delegateDescriptor);

                case ServiceEnumerableDescriptor enumerableDescriptor:
                    return BuildEnumerable(enumerableDescriptor);

                default:
                    throw new InvalidOperationException();
            }
        }

        protected virtual GraphSite BuildDelegate(ServiceDelegateDescriptor descriptor)
        {
            return new GraphDelegateSite()
            {
                Delegate = descriptor.Delegate,
                LifeStyle = descriptor.LifeStyle,
                ServiceId = descriptor.ServiceId,
                ServiceType = descriptor.ServiceType
            };
        }

        protected virtual GraphSite BuildInstance(ServiceInstanceDescriptor descriptor)
        {
            return new GraphInstanceSite()
            {
                Instance = descriptor.Instance,
                LifeStyle = descriptor.LifeStyle,
                ServiceId = descriptor.ServiceId,
                ServiceType = descriptor.ServiceType
            };
        }

        protected virtual GraphSite BuildEnumerable(ServiceEnumerableDescriptor descriptor)
        {
            var elements = new List<GraphSite>();

            foreach (var item in descriptor.Elements)
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
                ElementType = descriptor.ElementType,
                LifeStyle = descriptor.LifeStyle,
                ServiceType = descriptor.ServiceType,
                ServiceId = descriptor.ServiceId
            };
        }

        protected virtual GraphSite BuildType(ServiceTypeDescriptor descriptor)
        {
            var site = BuildType(descriptor, false) ?? BuildType(descriptor, true);
            if (site == null)
            {
                throw new InvalidOperationException($"cannot match a constructor for type:{descriptor.ImplementationType.FullName}!");
            }

            return site;
        }

        protected virtual GraphSite BuildType(ServiceTypeDescriptor descriptor, bool useDefauftParameterValue)
        {
            var site = default(GraphTypeSite);

            if (CallScope.Contains(descriptor.ImplementationType))
            {
                throw new GraphCircularException(descriptor.ImplementationType, CallScope.Clone(), $"type:{descriptor.ImplementationType.FullName} exists circular dependencies!");
            }

            using (CallScope.CreateScope(descriptor.ImplementationType))
            {
                foreach (var constructorInfo in descriptor.Constrcutors.OrderByDescending(i => i.GetParameters().Length))
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
                        LifeStyle = descriptor.LifeStyle,
                        ServiceType = descriptor.ServiceType,
                        ServiceId = descriptor.ServiceId,
                        ImplementationType = descriptor.ImplementationType
                    };

                    break;
                }
            }

            if (site != null)
            {
                using (CallScope.CreateScope(descriptor.ImplementationType))
                {
                    site.PropertySites = BindProperties(site);
                }
            }

            return site;
        }

        protected virtual GraphSite BuildLazy(ServiceLazyDescriptor descriptor)
        {
            foreach (var constructorInfo in descriptor.Constrcutors)
            {
                //new Lazy(()=>r=>r.ResolveService(T),true);
                var parameterInfos = constructorInfo.GetParameters();
                if (parameterInfos.Length != 1 ||
                    parameterInfos[0].ParameterType.IsNotType(typeof(Func<>)))
                {
                    continue;
                }

                return new GraphLazySite()
                {
                    Tag = descriptor.Tag,
                    TagOptional = descriptor.TagOptional,
                    LifeStyle = descriptor.LifeStyle,
                    ServiceId = descriptor.ServiceId,
                    ValueType = descriptor.ImplementationType.GenericTypeArguments[0],
                    ServiceType = descriptor.ServiceType,
                    ConstructorInfo = constructorInfo,
                    ImplementationType = descriptor.ImplementationType
                };
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

            var site = Build(parameterInfo.ParameterType, parameterInfo.GetCustomAttribute<TagAttribute>());
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
            var site = Build(propertyInfo.PropertyType, propertyInfo.GetCustomAttribute<TagAttribute>());
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
    }
}
