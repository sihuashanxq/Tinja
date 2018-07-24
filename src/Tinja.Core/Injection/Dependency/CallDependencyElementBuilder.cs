using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.Configuration;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Dependency;
using Tinja.Abstractions.Injection.Dependency.Elements;
using Tinja.Core.Injection.Internals;

namespace Tinja.Core.Injection.Dependency
{
    public class CallDependencyElementBuilder : ICallDependencyElementBuilder
    {
        protected IServiceConfiguration Configuration { get; }

        protected IServiceDescriptorFactory ServiceDescriptorFactory { get; set; }

        protected CallDependencyElementScope DenpendencyScope { get; set; }

        public CallDependencyElementBuilder(IServiceDescriptorFactory serviceDescriptorFactory, IServiceConfiguration configuration)
        {
            Configuration = configuration;
            ServiceDescriptorFactory = serviceDescriptorFactory;
            DenpendencyScope = new CallDependencyElementScope();
        }

        public virtual CallDepenencyElement Build(Type serviceType)
        {
            var descriptor = ServiceDescriptorFactory.Create(serviceType);
            if (descriptor == null)
            {
                return null;
            }

            return BuildElement(descriptor);
        }

        protected virtual CallDepenencyElement BuildElement(ServiceDescriptor descriptor)
        {
            switch (descriptor)
            {
                case ServiceManyDescriptor many:
                    return BuildManyElement(many);

                case ServiceInstanceDescriptor instance:
                    return BuildInstanceElement(instance);

                case ServiceDelegateDescriptor @delegate:
                    return BuildDelegateElement(@delegate);

                case ServiceProxyDescriptor proxy:
                    return BuildProxyElement(proxy);

                case ServiceConstrcutorDescriptor constrcutor:
                    using (DenpendencyScope.Begin(constrcutor.ImplementionType))
                        return BuildConstrcutorElement(constrcutor);
            }

            throw new InvalidOperationException();
        }

        protected virtual CallDepenencyElement BuildDelegateElement(ServiceDelegateDescriptor descriptor)
        {
            return new DelegateCallDepenencyElement()
            {
                LifeStyle = descriptor.LifeStyle,
                ServiceType = descriptor.ServiceType,
                Delegate = descriptor.Delegate
            };
        }

        protected virtual CallDepenencyElement BuildInstanceElement(ServiceInstanceDescriptor descriptor)
        {
            return new InstanceCallDependencyElement()
            {
                LifeStyle = descriptor.LifeStyle,
                ServiceType = descriptor.ServiceType,
                Instance = descriptor.Instance
            };
        }

        protected virtual CallDepenencyElement BuildProxyElement(ServiceProxyDescriptor descriptor)
        {
            var parameterElements = new Dictionary<ParameterInfo, CallDepenencyElement>();

            foreach (var item in descriptor.Constrcutors.OrderByDescending(i => i.GetParameters().Length))
            {
                var parameterInfos = item.GetParameters();
                if (parameterInfos.Length == 0)
                {
                    return new ConstructorCallDependencyElement()
                    {
                        Parameters = parameterElements,
                        LifeStyle = descriptor.LifeStyle,
                        ServiceType = descriptor.ServiceType,
                        ImplementionType = descriptor.ProxyType,
                        ConstructorInfo = item
                    };
                }

                foreach (var parameterInfo in parameterInfos)
                {
                    if (parameterInfo.ParameterType == descriptor.TargetType)
                    {
                        var parameterElement = BuildElement(new ServiceConstrcutorDescriptor()
                        {
                            ServiceType = descriptor.ServiceType,
                            ImplementionType = descriptor.TargetType,
                            LifeStyle = descriptor.LifeStyle
                        });

                        if (parameterElement == null)
                        {
                            parameterElements.Clear();
                            break;
                        }

                        parameterElements[parameterInfo] = parameterElement;
                        continue;
                    }

                    if (BuildParameterElement(parameterInfo, parameterElements))
                    {
                        continue;
                    }

                    parameterElements.Clear();
                    break;
                }

                if (parameterElements.Count == parameterInfos.Length)
                {
                    return new ConstructorCallDependencyElement()
                    {
                        Parameters = parameterElements,
                        LifeStyle = descriptor.LifeStyle,
                        ServiceType = descriptor.ServiceType,
                        ImplementionType = descriptor.ProxyType,
                        ConstructorInfo = item
                    };
                }
            }

            return null;
        }

        protected virtual CallDepenencyElement BuildManyElement(ServiceManyDescriptor descriptor)
        {
            var elements = new List<CallDepenencyElement>();

            foreach (var item in descriptor.Elements)
            {
                var ele = BuildElement(item);
                if (ele == null)
                {
                    continue;
                }

                elements.Add(ele);
            }

            return new ManyCallDepenencyElement()
            {
                Elements = elements.ToArray(),
                LifeStyle = descriptor.LifeStyle,
                ServiceType = descriptor.ServiceType,
                ImplementionType = descriptor.CollectionType,
                ConstructorInfo = descriptor.CollectionType.GetConstructors().FirstOrDefault(i => i.GetParameters().Length == 0)
            };
        }

        protected virtual CallDepenencyElement BuildConstrcutorElement(ServiceConstrcutorDescriptor descriptor)
        {
            var parameterElements = new Dictionary<ParameterInfo, CallDepenencyElement>();

            foreach (var item in descriptor.Constrcutors.OrderByDescending(i => i.GetParameters().Length))
            {
                var parameterInfos = item.GetParameters();
                if (parameterInfos.Any(parameterInfo => !BuildParameterElement(parameterInfo, parameterElements)))
                {
                    parameterElements.Clear();
                }

                if (parameterElements.Count != parameterInfos.Length)
                {
                    parameterElements.Clear();
                    continue;
                }

                var element = new ConstructorCallDependencyElement()
                {
                    Parameters = parameterElements,
                    LifeStyle = descriptor.LifeStyle,
                    ServiceType = descriptor.ServiceType,
                    ImplementionType = descriptor.ImplementionType,
                    ConstructorInfo = item
                };

                return BuildProperty(element);
            }

            return null;
        }

        protected CallDepenencyElement BuildProperty(ConstructorCallDependencyElement element)
        {
            if (element == null || !Configuration.Injection.EnablePropertyInjection)
            {
                return element;
            }

            var propertieInfos = element
                .ImplementionType
                .GetTypeInfo()
                .DeclaredProperties
                .Where(i => i.CanRead && i.CanWrite && i.IsDefined(typeof(InjectAttribute)));

            var properties = new Dictionary<PropertyInfo, CallDepenencyElement>();

            foreach (var propertieInfo in propertieInfos)
            {
                BuildPropertyElement(propertieInfo, properties);
            }

            element.Properties = properties;

            return element;
        }

        protected void BuildPropertyElement(PropertyInfo propertyInfo, Dictionary<PropertyInfo, CallDepenencyElement> propertyElements)
        {
            var descriptor = ServiceDescriptorFactory.Create(propertyInfo.PropertyType);
            if (descriptor == null)
            {
                return;
            }

            CheckCircularDependency(descriptor as ServiceConstrcutorDescriptor);

            var propertyElement = BuildElement(descriptor);
            if (propertyElement == null)
            {
                return;
            }

            propertyElements[propertyInfo] = propertyElement;
        }

        protected bool BuildParameterElement(ParameterInfo parameterInfo, Dictionary<ParameterInfo, CallDepenencyElement> parameterElements)
        {
            var ctx = ServiceDescriptorFactory.Create(parameterInfo.ParameterType);
            if (ctx == null)
            {
                return false;
            }

            CheckCircularDependency(ctx as ServiceConstrcutorDescriptor);

            var parameterElement = BuildElement(ctx);
            if (parameterElement == null)
            {
                return false;
            }

            parameterElements[parameterInfo] = parameterElement;

            return true;
        }

        protected void CheckCircularDependency(ServiceConstrcutorDescriptor ctx)
        {
            if (ctx == null || ctx.ImplementionType == null)
            {
                return;
            }

            if (DenpendencyScope.Contains(ctx.ImplementionType))
            {
                throw new CallCircularException(ctx.ImplementionType, $"type:{ctx.ImplementionType.FullName} exists circular dependencies!");
            }
        }
    }
}
