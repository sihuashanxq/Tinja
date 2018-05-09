using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Tinja.ServiceLife;
using Tinja.Resolving.Metadata;

namespace Tinja.Resolving
{
    public class ServiceResolvingContextBuilder : IServiceResolvingContextBuilder
    {
        protected ConcurrentDictionary<Type, List<Component>> Components { get; }

        protected ITypeMetadataFactory TypeMetadataFactory { get; }

        public ServiceResolvingContextBuilder(ITypeMetadataFactory typeMetadataFactory)
        {
            TypeMetadataFactory = typeMetadataFactory;
            Components = new ConcurrentDictionary<Type, List<Component>>();
        }

        public virtual void Initialize(ConcurrentDictionary<Type, List<Component>> components)
        {
            foreach (var item in components)
            {
                Components[item.Key] = item.Value.ToList();
            }
        }

        public virtual IServiceResolvingContext BuildResolvingContext(Type serviceType)
        {
            return
                BuildResolvingContextWithDirectly(serviceType) ??
                BuildResolvingContextWithOpenGeneric(serviceType) ??
                BuildResolvingContextWithEnumerable(serviceType);
        }

        protected virtual IServiceResolvingContext BuildResolvingContextWithDirectly(Type serviceType)
        {
            if (Components.TryGetValue(serviceType, out var components))
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

                return new ServiceResolvingContext(
                    serviceType,
                    GetImplementionMeta(serviceType, component),
                    component
                );
            }

            return null;
        }

        protected virtual IServiceResolvingContext BuildResolvingContextWithOpenGeneric(Type serviceType)
        {
            if (!serviceType.IsConstructedGenericType)
            {
                return null;
            }

            if (Components.TryGetValue(serviceType.GetGenericTypeDefinition(), out var components))
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

                return new ServiceResolvingContext(
                    serviceType,
                    GetImplementionMeta(serviceType, component),
                    component
                );
            }

            return null;
        }

        protected virtual IServiceResolvingContext BuildResolvingContextWithEnumerable(Type serviceType)
        {
            if (!serviceType.IsConstructedGenericType ||
                serviceType.GetGenericTypeDefinition() != typeof(IEnumerable<>))
            {
                return null;
            }

            var component = new Component()
            {
                ServiceType = typeof(IEnumerable<>),
                ImplementionType = typeof(List<>).MakeGenericType(serviceType.GenericTypeArguments),
                ImplementionFactory = null,
                LifeStyle = ServiceLifeStyle.Scoped
            };

            var elementType = serviceType.GenericTypeArguments.FirstOrDefault();
            var elesContext = BuildAllResolvingContext(elementType).Reverse().ToList();

            return new ServiceResolvingEnumerableContext(
                serviceType,
                GetImplementionMeta(serviceType, component),
                component,
                elesContext
            );
        }

        protected virtual IEnumerable<IServiceResolvingContext> BuildAllResolvingContext(Type serviceType)
        {
            var contexts = new List<IServiceResolvingContext>();

            var context = BuildResolvingContextWithEnumerable(serviceType);
            if (context != null)
            {
                contexts.Add(context);
            }

            contexts.AddRange(BuildAllResolvingContextWithDirectly(serviceType));
            contexts.AddRange(BuildAllResolvingContextWithOpenGeneric(serviceType));

            return contexts;
        }

        protected virtual IEnumerable<IServiceResolvingContext> BuildAllResolvingContextWithDirectly(Type serviceType)
        {
            if (Components.TryGetValue(serviceType, out var components))
            {
                return components.Select(i => new ServiceResolvingContext(serviceType, GetImplementionMeta(serviceType, i), i));
            }

            return new IServiceResolvingContext[0];
        }

        protected virtual IEnumerable<IServiceResolvingContext> BuildAllResolvingContextWithOpenGeneric(Type serviceType)
        {
            if (!serviceType.IsConstructedGenericType)
            {
                return new IServiceResolvingContext[0];
            }

            if (Components.TryGetValue(serviceType.GetGenericTypeDefinition(), out var components))
            {
                return components
                    .Select(i => new ServiceResolvingContext(
                        serviceType, 
                        GetImplementionMeta(serviceType, i), i)
                    );
            }

            return new IServiceResolvingContext[0];
        }

        protected TypeMetadata GetImplementionMeta(Type typeInfo, Component component)
        {
            if (component.ImplementionFactory != null)
            {
                return TypeMetadataFactory.Create(typeInfo);
            }

            var impl = component.ImplementionType;
            if (impl.IsGenericTypeDefinition && typeInfo.IsConstructedGenericType)
            {
                impl = impl.MakeGenericType(typeInfo.GenericTypeArguments);
            }

            return TypeMetadataFactory.Create(impl);
        }
    }
}
