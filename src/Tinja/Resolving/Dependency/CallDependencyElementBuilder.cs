using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Configuration;
using Tinja.Resolving.Context;
using Tinja.Resolving.Dependency.Elements;

namespace Tinja.Resolving.Dependency
{
    public class CallDependencyElementBuilder : ICallDependencyElementBuilder
    {
        protected IServiceConfiguration Configuration { get; }

        protected IServiceContextFactory ContextFactory { get; set; }

        protected CallDependencyElementScope DenpendencyScope { get; set; }

        public CallDependencyElementBuilder(IServiceContextFactory contextFactory, IServiceConfiguration configuration)
        {
            Configuration = configuration;
            ContextFactory = contextFactory;
            DenpendencyScope = new CallDependencyElementScope();
        }

        public virtual CallDepenencyElement Build(Type serviceType)
        {
            var ctx = ContextFactory.CreateContext(serviceType);
            if (ctx == null)
            {
                return null;
            }

            return BuildElement(ctx);
        }

        protected virtual CallDepenencyElement BuildElement(ServiceContext ctx)
        {
            switch (ctx)
            {
                case ServiceManyContext many:
                    return BuildManyElement(many);

                case ServiceInstanceContext instance:
                    return BuildInstanceElement(instance);

                case ServiceDelegateContext @delegate:
                    return BuildDelegateElement(@delegate);

                case ServiceProxyContext proxy:
                    return BuildProxyElement(proxy);

                case ServiceConstrcutorContext constrcutor:
                    using (DenpendencyScope.Begin(constrcutor.ImplementionType))
                        return BuildConstrcutorElement(constrcutor);
            }

            throw new InvalidOperationException();
        }

        protected virtual CallDepenencyElement BuildDelegateElement(ServiceDelegateContext ctx)
        {
            return new DelegateCallDepenencyElement()
            {
                LifeStyle = ctx.LifeStyle,
                ServiceType = ctx.ServiceType,
                Delegate = ctx.Delegate
            };
        }

        protected virtual CallDepenencyElement BuildInstanceElement(ServiceInstanceContext ctx)
        {
            return new InstanceCallDependencyElement()
            {
                LifeStyle = ctx.LifeStyle,
                ServiceType = ctx.ServiceType,
                Instance = ctx.Instance
            };
        }

        protected virtual CallDepenencyElement BuildProxyElement(ServiceProxyContext ctx)
        {
            var parameterElements = new Dictionary<ParameterInfo, CallDepenencyElement>();

            foreach (var item in ctx.Constrcutors.OrderByDescending(i => i.GetParameters().Length))
            {
                var parameterInfos = item.GetParameters();
                if (parameterInfos.Length == 0)
                {
                    return new ConstructorCallDependencyElement()
                    {
                        Parameters = parameterElements,
                        LifeStyle = ctx.LifeStyle,
                        ServiceType = ctx.ServiceType,
                        ImplementionType = ctx.ProxyType,
                        ConstructorInfo = item
                    };
                }

                foreach (var parameterInfo in parameterInfos)
                {
                    if (parameterInfo.ParameterType == ctx.TargetType)
                    {
                        var parameterElement = BuildElement(new ServiceConstrcutorContext()
                        {
                            ServiceType = ctx.ServiceType,
                            ImplementionType = ctx.TargetType,
                            LifeStyle = ctx.LifeStyle
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
                        LifeStyle = ctx.LifeStyle,
                        ServiceType = ctx.ServiceType,
                        ImplementionType = ctx.ProxyType,
                        ConstructorInfo = item
                    };
                }
            }

            return null;
        }

        protected virtual CallDepenencyElement BuildManyElement(ServiceManyContext ctx)
        {
            var elements = new List<CallDepenencyElement>();

            foreach (var item in ctx.Elements)
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
                LifeStyle = ctx.LifeStyle,
                ServiceType = ctx.ServiceType,
                ImplementionType = ctx.CollectionType,
                ConstructorInfo = ctx.CollectionType.GetConstructors().FirstOrDefault(i => i.GetParameters().Length == 0)
            };
        }

        protected virtual CallDepenencyElement BuildConstrcutorElement(ServiceConstrcutorContext ctx)
        {
            var parameterElements = new Dictionary<ParameterInfo, CallDepenencyElement>();

            foreach (var item in ctx.Constrcutors.OrderByDescending(i => i.GetParameters().Length))
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
                    LifeStyle = ctx.LifeStyle,
                    ServiceType = ctx.ServiceType,
                    ImplementionType = ctx.ImplementionType,
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
            var ctx = ContextFactory.CreateContext(propertyInfo.PropertyType);
            if (ctx == null)
            {
                return;
            }

            CheckCircularDependency(ctx as ServiceConstrcutorContext);

            var propertyElement = BuildElement(ctx);
            if (propertyElement == null)
            {
                return;
            }

            propertyElements[propertyInfo] = propertyElement;
        }

        protected bool BuildParameterElement(ParameterInfo parameterInfo, Dictionary<ParameterInfo, CallDepenencyElement> parameterElements)
        {
            var ctx = ContextFactory.CreateContext(parameterInfo.ParameterType);
            if (ctx == null)
            {
                return false;
            }

            CheckCircularDependency(ctx as ServiceConstrcutorContext);

            var parameterElement = BuildElement(ctx);
            if (parameterElement == null)
            {
                return false;
            }

            parameterElements[parameterInfo] = parameterElement;

            return true;
        }

        protected void CheckCircularDependency(ServiceConstrcutorContext ctx)
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
