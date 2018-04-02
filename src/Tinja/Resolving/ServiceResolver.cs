using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Resolving.Builder;
using Tinja.Resolving.Descriptor;
using Tinja.Resolving.ReslovingContext;

namespace Tinja.Resolving
{
    public class ServiceResolver : IServiceResolver
    {
        private IContainer _container;

        private ILifeStyleScope _lifeScope;

        private ITypeDescriptorProvider _typeDescriptorProvider;

        private IResolvingContextBuilder _resolvingContextBuilder;

        private IServiceFactoryBuilder _instanceFacotryBuilder;

        public ServiceResolver(
            IContainer container,
            ILifeStyleScope lifeScope,
            ITypeDescriptorProvider typeDescriptorProvider,
            IResolvingContextBuilder resolvingContextBuilder)
        {
            _container = container;
            _lifeScope = lifeScope;
            _typeDescriptorProvider = typeDescriptorProvider;
            _resolvingContextBuilder = resolvingContextBuilder;
            _instanceFacotryBuilder = new ServiceFactoryBuilder();
        }

        public object Resolve(Type resolvingType)
        {
            var factory = _instanceFacotryBuilder.Build(resolvingType);
            if (factory != null)
            {
                return factory(_container, _lifeScope);
            }

            var context = _resolvingContextBuilder.BuildResolvingContext(resolvingType);
            if (context == null)
            {
                return null;
            }

            return _lifeScope.GetOrAddLifeScopeInstance(context, ctx =>
            {
                var iFactory = GetInstanceFactory(ctx);
                if (iFactory == null)
                {
                    return null;
                }

                return iFactory(_container, _lifeScope);
            });
        }

        protected Func<IContainer, ILifeStyleScope, object> GetInstanceFactory(IResolvingContext resolvingContext)
        {
            var component = resolvingContext.Component;
            if (component.ImplementionFactory != null)
            {
                return (o, scoped) =>
                {
                    return resolvingContext.Component.ImplementionFactory(_container);
                };
            }

            var implementionType = GetImplementionType(
                resolvingContext.ReslovingType,
                resolvingContext.Component.ImplementionType
            );

            var descriptor = _typeDescriptorProvider.Get(implementionType);
            if (descriptor == null || descriptor.Constructors == null || descriptor.Constructors.Length == 0)
            {
                return null;
            }

            var node = CreateServiceNode(resolvingContext, descriptor, new HashSet<Type>());
            if (node == null)
            {
                return null;
            }

            return _instanceFacotryBuilder.Build(node);
        }

        protected IServiceNode CreateServiceNode(
            IResolvingContext resolvingContext,
            TypeDescriptor descriptor,
            HashSet<Type> resolvedTypes
        )
        {
            if (resolvedTypes.Contains(descriptor.Type))
            {
                throw new NotSupportedException($"Circular dependencies:type{descriptor.Type.FullName}!");
            }

            resolvedTypes.Add(descriptor.Type);

            if (resolvingContext is ResolvingEnumerableContext eResolvingContext)
            {
                return CreateServiceEnumerableNode(eResolvingContext, descriptor, resolvedTypes);
            }

            var parameters = new Dictionary<ParameterInfo, IServiceNode>();

            foreach (var item in descriptor.Constructors.OrderByDescending(i => i.Paramters.Length))
            {
                foreach (var parameter in item.Paramters)
                {
                    var context = _resolvingContextBuilder.BuildResolvingContext(parameter.ParameterType);
                    if (context == null)
                    {
                        parameters.Clear();
                        break;
                    }

                    if (context.Component.ImplementionFactory != null)
                    {
                        parameters[parameter] = new ServiceConstrutorNode()
                        {
                            Constructor = null,
                            Paramters = null,
                            ResolvingContext = context
                        };

                        continue;
                    }

                    var implementionType = GetImplementionType(
                        context.ReslovingType,
                        context.Component.ImplementionType
                    );

                    var paramterDescriptor = _typeDescriptorProvider.Get(implementionType);
                    var parameterTypeContext = CreateServiceNode(context, paramterDescriptor, resolvedTypes);

                    if (parameterTypeContext == null)
                    {
                        parameters.Clear();
                        break;
                    }

                    parameters[parameter] = parameterTypeContext;
                }

                if (parameters.Count == item.Paramters.Length)
                {
                    return new ServiceConstrutorNode()
                    {
                        Constructor = item,
                        ResolvingContext = resolvingContext,
                        Paramters = parameters,
                        Properties = CreatePropertyNodes(resolvingContext, descriptor)
                    };
                }
            }

            return null;
        }

        protected IServiceNode CreateServiceEnumerableNode(
            ResolvingEnumerableContext eResolvingContext,
            TypeDescriptor descriptor,
            HashSet<Type> resolvedTypes
        )
        {
            var elements = new IServiceNode[eResolvingContext.ElementsResolvingContext.Count];

            for (var i = 0; i < elements.Length; i++)
            {
                var implementionType = GetImplementionType(
                    eResolvingContext.ElementsResolvingContext[i].ReslovingType,
                    eResolvingContext.ElementsResolvingContext[i].Component.ImplementionType
                );

                elements[i] = CreateServiceNode(
                    eResolvingContext.ElementsResolvingContext[i],
                    _typeDescriptorProvider.Get(implementionType),
                    resolvedTypes
                );
            }

            return new ServiceEnumerableNode()
            {
                Constructor = descriptor.Constructors.FirstOrDefault(i => i.Paramters.Length == 0),
                Paramters = new Dictionary<ParameterInfo, IServiceNode>(),
                ResolvingContext = eResolvingContext,
                Elements = elements
            };
        }

        protected Dictionary<PropertyInfo, IServiceNode> CreatePropertyNodes(
            IResolvingContext resolvingContext,
            TypeDescriptor descriptor
        )
        {
            var propertyNodes = new Dictionary<PropertyInfo, IServiceNode>();

            foreach (var item in descriptor.Properties)
            {
                var context = _resolvingContextBuilder.BuildResolvingContext(item.PropertyType);
                if (context == null)
                {
                    continue;
                }

                if (context.Component.ImplementionFactory != null)
                {
                    propertyNodes[item] = new ServiceConstrutorNode()
                    {
                        Constructor = null,
                        Paramters = null,
                        ResolvingContext = context
                    };

                    continue;
                }

                var implementionType = GetImplementionType(
                    context.ReslovingType,
                    context.Component.ImplementionType
                );

                var propertyDescriptor = _typeDescriptorProvider.Get(implementionType);
                if (propertyDescriptor == null)
                {
                    continue;
                }

                var propertyNode = CreateServiceNode(context, propertyDescriptor, new HashSet<Type>());
                if (propertyNode == null)
                {
                    continue;
                }

                propertyNodes[item] = propertyNode;
            }

            return propertyNodes;
        }

        private static Type GetImplementionType(Type resolvingType, Type implementionType)
        {
            if (implementionType.IsGenericTypeDefinition && resolvingType.IsConstructedGenericType)
            {
                return implementionType.MakeGenericType(resolvingType.GenericTypeArguments);
            }

            return implementionType;
        }
    }
}
