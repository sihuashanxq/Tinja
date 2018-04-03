using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Resolving.Builder;
using Tinja.Resolving.Descriptor;
using Tinja.Resolving.ReslovingContext;

namespace Tinja.Resolving
{
    public class ServiceNodeBuilder : IServiceNodeBuilder
    {
        private Dictionary<Type, IServiceNode> _reslovedNodes;

        private ITypeDescriptorProvider _typeDescriptorProvider;

        private IResolvingContextBuilder _resolvingContextBuilder;

        public ServiceNodeBuilder(
            ITypeDescriptorProvider typeDescriptorProvider,
            IResolvingContextBuilder resolvingContextBuilder
        )
        {
            _reslovedNodes = new Dictionary<Type, IServiceNode>();
            _typeDescriptorProvider = typeDescriptorProvider;
            _resolvingContextBuilder = resolvingContextBuilder;
        }

        public IServiceNode Build(IResolvingContext resolvingContext)
        {
            var implementionType = GetImplementionType(
                 resolvingContext.ReslovingType,
                 resolvingContext.Component.ImplementionType
             );

            var descriptor = _typeDescriptorProvider.Get(implementionType);
            if (descriptor == null || descriptor.Constructors == null || descriptor.Constructors.Length == 0)
            {
                return null;
            }

            var node = CreateNode(resolvingContext, descriptor, new Dictionary<Type, IServiceNode>());
            if (node != null)
            {
                CreateProperties(node, new Dictionary<Type, IServiceNode>());
            }

            return node;
        }

        protected IServiceNode CreateNode(
            IResolvingContext context,
            TypeDescriptor descriptor,
            Dictionary<Type, IServiceNode> resolvedNodes
        )
        {
            if (context is ResolvingEnumerableContext eResolvingContext)
            {
                return CreateEnumerable(eResolvingContext, descriptor, resolvedNodes);
            }

            var parameters = new Dictionary<ParameterInfo, IServiceNode>();
            var allScoped = new Dictionary<Type, IServiceNode>();

            foreach (var item in descriptor.Constructors.OrderByDescending(i => i.Paramters.Length))
            {
                foreach (var parameter in item.Paramters)
                {
                    var pContext = _resolvingContextBuilder.BuildResolvingContext(parameter.ParameterType);
                    if (pContext == null)
                    {
                        parameters.Clear();
                        break;
                    }

                    if (pContext.Component.ImplementionFactory != null)
                    {
                        parameters[parameter] = new ServiceConstrutorNode()
                        {
                            Constructor = null,
                            Paramters = null,
                            ResolvingContext = pContext
                        };

                        continue;
                    }

                    var implementionType = GetImplementionType(
                        pContext.ReslovingType,
                        pContext.Component.ImplementionType
                    );

                    var scoped = new Dictionary<Type, IServiceNode>();
                    var paramterDescriptor = _typeDescriptorProvider.Get(implementionType);
                    var parameterTypeContext = CreateNode(pContext, paramterDescriptor, scoped);

                    if (parameterTypeContext == null)
                    {
                        parameters.Clear();
                        allScoped.Clear();
                        break;
                    }

                    allScoped.AddRange(scoped);
                    parameters[parameter] = parameterTypeContext;
                }

                if (parameters.Count == item.Paramters.Length)
                {
                    var node = new ServiceConstrutorNode()
                    {
                        Constructor = item,
                        ResolvingContext = context,
                        Paramters = parameters
                    };

                    resolvedNodes.AddRange(allScoped);

                    if (resolvedNodes.ContainsKey(descriptor.Type))
                    {
                        throw new NotSupportedException($"Circular dependencies:type{descriptor.Type.FullName}!");
                    }

                    resolvedNodes[descriptor.Type] = node;

                    return node;
                }
            }

            return null;
        }

        protected IServiceNode CreateEnumerable(
            ResolvingEnumerableContext context,
            TypeDescriptor descriptor,
            Dictionary<Type, IServiceNode> resolvedNodes
        )
        {
            var elements = new IServiceNode[context.ElementsResolvingContext.Count];

            for (var i = 0; i < elements.Length; i++)
            {
                var scoped = new Dictionary<Type, IServiceNode>();
                var implementionType = GetImplementionType(
                    context.ElementsResolvingContext[i].ReslovingType,
                    context.ElementsResolvingContext[i].Component.ImplementionType
                );

                elements[i] = CreateNode(
                    context.ElementsResolvingContext[i],
                    _typeDescriptorProvider.Get(implementionType),
                    scoped
                );

                resolvedNodes.AddRange(scoped);
            }

            var node = new ServiceEnumerableNode()
            {
                Constructor = descriptor.Constructors.FirstOrDefault(i => i.Paramters.Length == 0),
                Paramters = new Dictionary<ParameterInfo, IServiceNode>(),
                ResolvingContext = context,
                Elements = elements
            };

            if (resolvedNodes.ContainsKey(descriptor.Type))
            {
                throw new NotSupportedException($"Circular dependencies:type{descriptor.Type.FullName}!");
            }

            resolvedNodes[descriptor.Type] = node;

            return node;
        }

        protected Dictionary<PropertyInfo, IServiceNode> CreateProperties(
            IServiceNode node,
            TypeDescriptor descriptor,
            Dictionary<Type, IServiceNode> resolvedNodes
        )
        {
            var propertyNodes = new Dictionary<PropertyInfo, IServiceNode>();

            if (descriptor.Properties == null || descriptor.Properties.Length == 0)
            {
                return propertyNodes;
            }

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
                resolvedNodes = new Dictionary<Type, IServiceNode>()
                {
                    [descriptor.Type] = node
                };

                try
                {
                    var propertyNode = CreateNode(context, propertyDescriptor, resolvedNodes);
                    if (propertyNode == null)
                    {
                        continue;
                    }

                    propertyNodes[item] = propertyNode;
                }
                catch (Exception e)
                {
                    //don't warn
                    if (resolvedNodes.TryGetValue(propertyDescriptor.Type, out var cacheNode))
                    {
                        if (cacheNode.ResolvingContext.Component.LifeStyle != LifeStyle.Transient)
                        {
                            propertyNodes[item] = cacheNode;
                        }
                    }
                }
            }

            node.Properties = propertyNodes;

            return propertyNodes;
        }

        protected void CreateProperties(IServiceNode node, Dictionary<Type, IServiceNode> resolvedNodes)
        {
            if (node is ServiceEnumerableNode eNode)
            {
                CreateProperties(
                    node,
                    _typeDescriptorProvider.Get(
                        node.Constructor.ConstructorInfo.DeclaringType
                    ),
                    resolvedNodes
                );

                foreach (var item in eNode.Elements.Where(i => i.Constructor != null))
                {
                    CreateProperties(
                        item,
                        _typeDescriptorProvider.Get(
                            item.Constructor.ConstructorInfo.DeclaringType
                        ),
                        resolvedNodes
                    );
                }
            }
            else
            {
                CreateProperties(
                    node,
                    _typeDescriptorProvider.Get(
                        node.Constructor.ConstructorInfo.DeclaringType
                    ),
                    resolvedNodes
                );

                foreach (var item in node.Paramters.Where(i => i.Value.Constructor != null))
                {
                    CreateProperties(
                        item.Value,
                        _typeDescriptorProvider.Get(
                            item.Value.Constructor.ConstructorInfo.DeclaringType
                        ),
                        resolvedNodes
                    );
                }
            }
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

    internal static class DictionaryExtension
    {
        internal static Dictionary<TKey, TValue> Clone<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
            {
                return new Dictionary<TKey, TValue>();
            }

            var dic = new Dictionary<TKey, TValue>();

            foreach (var kv in dictionary)
            {
                dic[kv.Key] = kv.Value;
            }

            return dic;
        }

        internal static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> values)
        {
            if (values == null)
            {
                return;
            }

            foreach (var kv in values)
            {
                dictionary[kv.Key] = kv.Value;
            }
        }
    }
}
