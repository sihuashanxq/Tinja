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
        protected ConcurrentDictionary<Type, List<ServiceComponent>> Components { get; }

        protected ITypeMetadataFactory TypeFactory { get; }

        internal IMemberInterceptionCollector InterceptionProvider { get; }

        internal ServiceContextFactory(ITypeMetadataFactory typeFactory, IMemberInterceptionCollector provider)
        {
            InterceptionProvider = provider;
            TypeFactory = typeFactory;
            Components = new ConcurrentDictionary<Type, List<ServiceComponent>>();
        }

        public virtual void Initialize(ConcurrentDictionary<Type, List<ServiceComponent>> components)
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

        protected virtual void InitializeProxyType(ServiceComponent component)
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

        public virtual IServiceContext CreateContext(Type serviceType)
        {
            return
                CreateContextDirectly(serviceType) ??
                CreateContextOpenGeneric(serviceType) ??
                CreateContextEnumerable(serviceType);
        }

        protected IServiceContext CreateContext(Type serviceType, ServiceComponent component)
        {
            if (component.ImplementionFactory != null)
            {
                return new ServiceDelegateContext(serviceType, component.LifeStyle, component.ImplementionFactory);
            }

            var meta = MakeTypeMetadata(serviceType, component.ImplementionType);
            if (meta == null)
            {
                throw new InvalidOperationException($"Create TypeMetada failed!Service Type:${serviceType.FullName}");
            }

            if (component.ProxyType != null)
            {
                var proxyMeta = MakeTypeMetadata(serviceType, component.ProxyType);
                if (proxyMeta != null)
                {
                    return new ServiceProxyContext(serviceType, proxyMeta.Type, meta.Type, component.LifeStyle, meta.Constructors, proxyMeta.Constructors);
                }
            }

            return new ServiceTypeContext(serviceType, meta.Type, component.LifeStyle, meta.Constructors);
        }

        protected virtual IServiceContext CreateContextDirectly(Type serviceType)
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

        protected virtual IServiceContext CreateContextOpenGeneric(Type serviceType)
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

        protected virtual IServiceContext CreateContextEnumerable(Type serviceType)
        {
            if (!serviceType.IsConstructedGenericType || serviceType.GetGenericTypeDefinition() != typeof(IEnumerable<>))
            {
                return null;
            }

            var component = new ServiceComponent()
            {
                ServiceType = typeof(IEnumerable<>),
                ImplementionType = typeof(List<>).MakeGenericType(serviceType.GenericTypeArguments),
                ImplementionFactory = null,
                LifeStyle = ServiceLifeStyle.Scoped
            };

            var elementType = serviceType.GenericTypeArguments.FirstOrDefault();
            var eles = CreateManyContext(elementType).Reverse().ToList();
            var meta = MakeTypeMetadata(serviceType, component.ImplementionType);

            return new ServiceManyContext(
                serviceType,
                meta.Type,
                component.LifeStyle,
                meta.Constructors,
                eles
            );
        }

        protected virtual IEnumerable<IServiceContext> CreateManyContext(Type serviceType)
        {
            var ctxs = new List<IServiceContext>();
            var ctx = CreateContextEnumerable(serviceType);
            if (ctx != null)
            {
                ctxs.Add(ctx);
            }

            ctxs.AddRange(CreateManyContextDirectly(serviceType));
            ctxs.AddRange(CreateManyContextOpenGeneric(serviceType));

            return ctxs;
        }

        protected virtual IEnumerable<IServiceContext> CreateManyContextDirectly(Type serviceType)
        {
            return Components.TryGetValue(serviceType, out var components)
                ? components.Select(i => CreateContext(serviceType, i))
                : new IServiceContext[0];
        }

        protected virtual IEnumerable<IServiceContext> CreateManyContextOpenGeneric(Type serviceType)
        {
            if (!serviceType.IsConstructedGenericType)
            {
                return new IServiceContext[0];
            }

            return Components.TryGetValue(serviceType.GetGenericTypeDefinition(), out var components)
                ? components.Select(i => CreateContext(serviceType, i))
                : new IServiceContext[0];
        }

        protected TypeMetadata MakeTypeMetadata(Type serviceType, Type implementionType)
        {
            if (implementionType.IsGenericTypeDefinition && serviceType.IsConstructedGenericType)
            {
                implementionType = implementionType.MakeGenericType(serviceType.GenericTypeArguments);
            }

            return TypeFactory.Create(implementionType);
        }

        private bool ShouldCreateProxyType(ServiceComponent component)
        {
            if (component == null)
            {
                return false;
            }

            return component.ImplementionFactory == null && InterceptionProvider.Collect(component.ServiceType, component.ImplementionType, false).Any();
        }
    }
}
