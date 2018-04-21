using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Tinja.LifeStyle;
using Tinja.Resolving.Service;

namespace Tinja.Resolving.Context
{
    public class ResolvingContextBuilder : IResolvingContextBuilder
    {
        protected ConcurrentDictionary<Type, List<Component>> Components { get; }

        protected IServiceInfoFactory ServiceInfoFactory { get; }

        public ResolvingContextBuilder(IServiceInfoFactory serviceInfoFactory)
        {
            ServiceInfoFactory = serviceInfoFactory;
            Components = new ConcurrentDictionary<Type, List<Component>>();
        }

        public virtual void Initialize(ConcurrentDictionary<Type, List<Component>> components)
        {
            foreach (var item in components)
            {
                Components[item.Key] = item.Value.ToList();
            }
        }

        public virtual IResolvingContext BuildResolvingContext(Type resolvingType)
        {
            return
                BuildResolvingContextWithDirectly(resolvingType) ??
                BuildResolvingContextWithOpenGeneric(resolvingType) ??
                BuildResolvingContextWithEnumerable(resolvingType);
        }

        protected virtual IResolvingContext BuildResolvingContextWithDirectly(Type resolvingType)
        {
            if (Components.TryGetValue(resolvingType, out var components))
            {
                if (components == null)
                {
                    return null;
                }

                var component = components.LastOrDefault();
                if (component == null)
                {
                    return null;
                }

                return new ResolvingContext(
                    resolvingType,
                    GetServiceInfo(resolvingType, component),
                    component
                );
            }

            return null;
        }

        protected virtual IResolvingContext BuildResolvingContextWithOpenGeneric(Type resolvingType)
        {
            if (!resolvingType.IsConstructedGenericType)
            {
                return null;
            }

            if (Components.TryGetValue(resolvingType.GetGenericTypeDefinition(), out var components))
            {
                if (components == null)
                {
                    return null;
                }

                var component = components.LastOrDefault();
                if (component == null)
                {
                    return null;
                }

                return new ResolvingContext(
                    resolvingType,
                    GetServiceInfo(resolvingType, component),
                    component
                );
            }

            return null;
        }

        protected virtual IResolvingContext BuildResolvingContextWithEnumerable(Type resolvingType)
        {
            if (!resolvingType.IsConstructedGenericType ||
                resolvingType.GetGenericTypeDefinition() != typeof(IEnumerable<>))
            {
                return null;
            }

            var component = new Component()
            {
                ServiceType = typeof(IEnumerable<>),
                ImplementionType = typeof(List<>).MakeGenericType(resolvingType.GenericTypeArguments),
                ImplementionFactory = null,
                LifeStyle = ServiceLifeStyle.Scoped
            };

            var elementType = resolvingType.GenericTypeArguments.FirstOrDefault();
            var elesContext = BuildAllResolvingContext(elementType).Reverse().ToList();

            return new ResolvingEnumerableContext(
                resolvingType, 
                GetServiceInfo(resolvingType, component), 
                component, 
                elesContext
            );
        }

        protected virtual IEnumerable<IResolvingContext> BuildAllResolvingContext(Type resolvingType)
        {
            var contexts = new List<IResolvingContext>();

            var context = BuildResolvingContextWithEnumerable(resolvingType);
            if (context != null)
            {
                contexts.Add(context);
            }

            contexts.AddRange(BuildAllResolvingContextWithDirectly(resolvingType));
            contexts.AddRange(BuildAllResolvingContextWithOpenGeneric(resolvingType));

            return contexts;
        }

        protected virtual IEnumerable<IResolvingContext> BuildAllResolvingContextWithDirectly(Type resolvingType)
        {
            if (Components.TryGetValue(resolvingType, out var components))
            {
                return components.Select(i => new ResolvingContext(resolvingType, GetServiceInfo(resolvingType, i), i));
            }

            return new IResolvingContext[0];
        }

        protected virtual IEnumerable<IResolvingContext> BuildAllResolvingContextWithOpenGeneric(Type resolvingType)
        {
            if (!resolvingType.IsConstructedGenericType)
            {
                return new IResolvingContext[0];
            }

            if (Components.TryGetValue(resolvingType.GetGenericTypeDefinition(), out var components))
            {
                return components.Select(i => new ResolvingContext(resolvingType, GetServiceInfo(resolvingType, i), i));
            }

            return new IResolvingContext[0];
        }

        protected ServiceInfo GetServiceInfo(Type serviceType, Component component)
        {
            if (component.ImplementionFactory != null)
            {
                return null;
            }

            var implementionType = component.ImplementionType;
            if (implementionType.IsGenericTypeDefinition && serviceType.IsConstructedGenericType)
            {
                implementionType = implementionType.MakeGenericType(serviceType.GenericTypeArguments);
            }

            var serviceInfo = ServiceInfoFactory.Create(implementionType);
            if (serviceInfo == null || serviceInfo.Constructors == null || serviceInfo.Constructors.Length == 0)
            {
                return null;
            }

            return serviceInfo;
        }
    }
}
