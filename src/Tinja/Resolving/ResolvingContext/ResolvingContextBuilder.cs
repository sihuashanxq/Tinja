using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Tinja.Resolving.ReslovingContext
{
    public class ResolvingContextBuilder : IResolvingContextBuilder
    {
        private ConcurrentDictionary<Type, IResolvingContext> _cache = new ConcurrentDictionary<Type, IResolvingContext>();

        protected ConcurrentDictionary<Type, List<Component>> Components { get; }

        public ResolvingContextBuilder(ConcurrentDictionary<Type, List<Component>> components)
        {
            Components = components;
        }

        public virtual IResolvingContext BuildResolvingContext(Type resolvingType)
        {
            return _cache.GetOrAdd(resolvingType, (k) =>
            {
                return
                    BuildResolvingContextWithDirectly(resolvingType) ??
                    BuildResolvingContextWithOpenGeneric(resolvingType) ??
                    BuildResolvingContextWithEnumerable(resolvingType);
            });
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

                return new ResolvingContext(resolvingType, component);
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

                return new ResolvingContext(resolvingType, component);
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
                LifeStyle = LifeStyle.Scoped
            };

            var elementType = resolvingType.GenericTypeArguments.FirstOrDefault();
            var elesContext = BuildAllResolvingContext(elementType).Reverse().ToList();

            return new ResolvingEnumerableContext(resolvingType, component, elesContext);
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
                return components.Select(i => new ResolvingContext(resolvingType, i));
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
                return components.Select(i => new ResolvingContext(resolvingType, i));
            }

            return new IResolvingContext[0];
        }
    }
}
