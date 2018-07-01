using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Configuration;
using Tinja.Resolving.Context;

namespace Tinja.Resolving.Dependency
{
    public class CallDependencyElementBuilder
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

            return Build(ctx);
        }

        protected virtual CallDepenencyElement Build(ServiceContext ctx)
        {
            using (DenpendencyScope.Begin(ctx.ImplementionType))
            {
                return BuildCallDependency(ctx);
            }
        }

        protected virtual CallDepenencyElement BuildCallDependency(ServiceContext ctx)
        {
            if (ctx.ImplementionInstance != null)
            {
                return BuildInstanceCallDepdency(ctx);
            }

            if (ctx.ImplementionFactory != null)
            {
                return BuildDelegateCallDepenency(ctx);
            }

            switch (ctx)
            {
                case ServiceManyContext many:
                    return BuildManyCallDependency(many);
                case ServiceProxyContext proxy:
                    return BuildProxyCallDependency(proxy);
                default:
                    return BuildDefaultCallDependency(ctx);
            }
        }

        protected virtual CallDepenencyElement BuildDelegateCallDepenency(ServiceContext ctx)
        {
            return new DelegateCallDepenencyElement()
            {
                LifeStyle = ctx.LifeStyle,
                ServiceType = ctx.ServiceType,
                Delegate = ctx.ImplementionFactory
            };
        }

        protected virtual CallDepenencyElement BuildInstanceCallDepdency(ServiceContext ctx)
        {
            return new InstanceCallDependencyElement()
            {
                LifeStyle = ctx.LifeStyle,
                ServiceType = ctx.ServiceType,
                Instance = ctx.ImplementionInstance
            };
        }

        protected virtual CallDepenencyElement BuildProxyCallDependency(ServiceProxyContext ctx)
        {
            var parameterElements = new Dictionary<ParameterInfo, CallDepenencyElement>();

            foreach (var item in ctx.ProxyConstructors.OrderByDescending(i => i.Paramters.Length))
            {
                foreach (var parameterInfo in item.Paramters)
                {
                    if (parameterInfo.ParameterType == ctx.ImplementionType)
                    {
                        var parameterElement = Build(new ServiceContext()
                        {
                            ServiceType = ctx.ServiceType,
                            ImplementionType = ctx.ImplementionType,
                            LifeStyle = ctx.LifeStyle,
                            Constrcutors = ctx.Constrcutors,
                            ImplementionFactory = ctx.ImplementionFactory
                        });

                        if (parameterElement == null)
                        {
                            parameterElements.Clear();
                            break;
                        }

                        parameterElements[parameterInfo] = parameterElement;
                        continue;
                    }

                    if (BuildParameter(parameterInfo, parameterElements))
                    {
                        continue;
                    }

                    parameterElements.Clear();
                    break;
                }

                if (parameterElements.Count == item.Paramters.Length)
                {
                    return new ConstructorCallDependencyElement()
                    {
                        Parameters = parameterElements,
                        LifeStyle = ctx.LifeStyle,
                        ServiceType = ctx.ServiceType,
                        ImplementionType = ctx.ProxyType,
                        ConstructorInfo = item.ConstructorInfo
                    };
                }
            }

            return null;
        }

        protected virtual CallDepenencyElement BuildManyCallDependency(ServiceManyContext ctx)
        {
            var elements = new List<CallDepenencyElement>();

            foreach (var item in ctx.Elements)
            {
                var ele = Build(item);
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
                ConstructorInfo = ctx.Constrcutors.FirstOrDefault()?.ConstructorInfo
            };
        }

        protected virtual CallDepenencyElement BuildDefaultCallDependency(ServiceContext ctx)
        {
            var parameterElements = new Dictionary<ParameterInfo, CallDepenencyElement>();

            foreach (var item in ctx.Constrcutors.OrderByDescending(i => i.Paramters.Length))
            {
                if (item.Paramters.Any(parameterInfo => !BuildParameter(parameterInfo, parameterElements)))
                {
                    parameterElements.Clear();
                }

                if (parameterElements.Count != item.Paramters.Length)
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
                    ConstructorInfo = item.ConstructorInfo
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
                BuildProperty(propertieInfo, properties);
            }

            element.Properties = properties;

            return element;
        }

        protected void BuildProperty(PropertyInfo propertyInfo, Dictionary<PropertyInfo, CallDepenencyElement> propertyElements)
        {
            var ctx = ContextFactory.CreateContext(propertyInfo.PropertyType);
            if (ctx == null)
            {
                return;
            }

            CheckCircularDependency(ctx.ImplementionType);

            var propertyElement = Build(ctx);
            if (propertyElement == null)
            {
                return;
            }

            propertyElements[propertyInfo] = propertyElement;
        }

        protected bool BuildParameter(ParameterInfo parameterInfo, Dictionary<ParameterInfo, CallDepenencyElement> parameterElements)
        {
            var ctx = ContextFactory.CreateContext(parameterInfo.ParameterType);
            if (ctx == null)
            {
                return false;
            }

            CheckCircularDependency(ctx.ImplementionType);

            var parameterElement = Build(ctx);
            if (parameterElement == null)
            {
                return false;
            }

            parameterElements[parameterInfo] = parameterElement;

            return true;
        }

        protected void CheckCircularDependency(Type implementionType)
        {
            if (implementionType == null)
            {
                return;
            }

            if (DenpendencyScope.Contains(implementionType))
            {
                throw new ServiceCallCircularException(implementionType, string.Empty);
            }
        }
    }
}
