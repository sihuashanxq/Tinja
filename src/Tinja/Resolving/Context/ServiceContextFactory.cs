using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Tinja.Interception;
using Tinja.Interception.Generators;
using Tinja.Resolving.Metadata;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Context
{
    public class ServiceContextFactory : IServiceContextFactory
    {
        protected ConcurrentDictionary<Type, List<Component>> Components { get; }

        protected ITypeMetadataFactory TypeFactory { get; }

        internal IMemberInterceptionCollector InterceptionProvider { get; }

        internal ServiceContextFactory(ITypeMetadataFactory typeFactory, IMemberInterceptionCollector provider)
        {
            InterceptionProvider = provider;
            TypeFactory = typeFactory;
            Components = new ConcurrentDictionary<Type, List<Component>>();
        }

        public virtual void Initialize(ConcurrentDictionary<Type, List<Component>> components)
        {
            foreach (var kv in components)
            {
                Components[kv.Key] = kv.Value.Select(i => i.Clone()).ToList();

                foreach (var item in Components[kv.Key])
                {
                    if (ShouldCreateProxyType(item))
                    {
                        InitializeProxyType(item);
                        continue;
                    }

                    if (item.ImplementionType != null && item.ImplementionType.IsAbstract)
                    {
                        throw new InvalidOperationException($"ImplementionType:{item.ImplementionType.FullName} not can be Abstract when have not Interceptors!");
                    }

                    if (item.ImplementionType != null && item.ImplementionType.IsInterface)
                    {
                        throw new InvalidOperationException($"ImplementionType:{item.ImplementionType.FullName} not can be Interface when have not Interceptors!");
                    }
                }
            }
        }

        protected virtual void InitializeProxyType(Component component)
        {
            if (component.ImplementionType.IsInterface)
            {
                component.ProxyType = new InterfaceProxyTypeGenerator(component.ImplementionType, InterceptionProvider).CreateProxyType();
            }
            else if (component.ImplementionType.IsAbstract)
            {
                component.ProxyType = new ClassProxyTypeGenerator(component.ImplementionType, InterceptionProvider).CreateProxyType();
            }
            else if (component.ServiceType.IsInterface)
            {
                component.ProxyType = new InterfaceWithTargetProxyTypeGenerator(component.ServiceType, component.ImplementionType, InterceptionProvider).CreateProxyType();
            }
            else
            {
                component.ProxyType = new ClassProxyTypeGenerator(component.ImplementionType, InterceptionProvider).CreateProxyType();
            }
        }

        public virtual ServiceContext CreateContext(Type serviceType)
        {
            return
                CreateContextDirectly(serviceType) ??
                CreateContextOpenGeneric(serviceType) ??
                CreateContextEnumerable(serviceType);
        }

        protected ServiceContext CreateContext(Type serviceType, Component component)
        {
            if (component.ImplementionFactory != null || component.ImplementionInstance != null)
            {
                return new ServiceContext()
                {
                    LifeStyle = component.LifeStyle,
                    ServiceType = serviceType,
                    ImplementionFactory = component.ImplementionFactory,
                    ImplementionInstance = component.ImplementionInstance
                };
            }

            var meta = MakeTypeMetadata(serviceType, component.ImplementionType);
            if (meta == null)
            {
                throw new InvalidOperationException(
                    $"Create ImplementionType metadata failed!Service Type:${serviceType.FullName}");
            }

            if (component.ProxyType == null)
            {
                return new ServiceContext()
                {
                    ServiceType = serviceType,
                    LifeStyle = component.LifeStyle,
                    Constrcutors = meta.Constructors,
                    ImplementionType = meta.Type,
                    ImplementionFactory = component.ImplementionFactory
                };
            }

            var proxyMeta = MakeTypeMetadata(serviceType, component.ProxyType);
            if (proxyMeta == null)
            {

                throw new InvalidOperationException($"Create ProxyImplementionType metadata failed!Service Type:${serviceType.FullName}");
            }

            return new ServiceProxyContext()
            {
                ServiceType = serviceType,
                LifeStyle = component.LifeStyle,
                Constrcutors = meta.Constructors,
                ImplementionType = meta.Type,
                ImplementionFactory = component.ImplementionFactory,
                ProxyType = proxyMeta.Type,
                ProxyConstructors = proxyMeta.Constructors
            };
        }

        protected virtual ServiceContext CreateContextDirectly(Type serviceType)
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

                return CreateContext(serviceType, component);
            }

            return null;
        }

        protected virtual ServiceContext CreateContextOpenGeneric(Type serviceType)
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
                if (component != null)
                {
                    return CreateContext(serviceType, component);
                }
            }

            return null;
        }

        protected virtual ServiceContext CreateContextEnumerable(Type serviceType)
        {
            if (!serviceType.IsConstructedGenericType || serviceType.GetGenericTypeDefinition() != typeof(IEnumerable<>))
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
            var elements = CreateManyContext(elementType).Reverse().ToList();
            var meta = MakeTypeMetadata(serviceType, component.ImplementionType);

            return new ServiceManyContext()
            {
                ServiceType = serviceType,
                ImplementionType = meta.Type,
                LifeStyle = component.LifeStyle,
                Constrcutors = meta.Constructors,
                Elements = elements
            };
        }

        protected virtual IEnumerable<ServiceContext> CreateManyContext(Type serviceType)
        {
            var ctxs = new List<ServiceContext>();
            var ctx = CreateContextEnumerable(serviceType);
            if (ctx != null)
            {
                ctxs.Add(ctx);
            }

            ctxs.AddRange(CreateManyContextDirectly(serviceType));
            ctxs.AddRange(CreateManyContextOpenGeneric(serviceType));

            return ctxs;
        }

        protected virtual IEnumerable<ServiceContext> CreateManyContextDirectly(Type serviceType)
        {
            return Components.TryGetValue(serviceType, out var components)
                ? components.Select(i => CreateContext(serviceType, i))
                : new ServiceContext[0];
        }

        protected virtual IEnumerable<ServiceContext> CreateManyContextOpenGeneric(Type serviceType)
        {
            if (!serviceType.IsConstructedGenericType)
            {
                return new ServiceContext[0];
            }

            return Components.TryGetValue(serviceType.GetGenericTypeDefinition(), out var components)
                ? components.Select(i => CreateContext(serviceType, i))
                : new ServiceContext[0];
        }

        protected TypeMetadata MakeTypeMetadata(Type serviceType, Type implementionType)
        {
            if (implementionType.IsGenericTypeDefinition && serviceType.IsConstructedGenericType)
            {
                implementionType = implementionType.MakeGenericType(serviceType.GenericTypeArguments);
            }

            return TypeFactory.Create(implementionType);
        }

        private bool ShouldCreateProxyType(Component component)
        {
            if (component == null)
            {
                return false;
            }

            return component.ImplementionFactory == null &&
                   component.ImplementionInstance == null &&
                   InterceptionProvider.Collect(component.ServiceType, component.ImplementionType, false).Any();
        }
    }
}
