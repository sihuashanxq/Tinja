using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

            return _lifeScope.GetOrAddLifeScopeInstance(context, ctx => Resolve(ctx));
        }

        public object Resolve(IResolvingContext resolvingContext)
        {
            if (resolvingContext.Component.ImplementionFactory != null)
            {
                return resolvingContext.Component.ImplementionFactory(_container);
            }

            var implType = resolvingContext.Component.ImplementionType;
            if (implType.IsGenericType && resolvingContext.ReslovingType.IsConstructedGenericType)
            {
                implType = implType.MakeGenericType(resolvingContext.ReslovingType.GenericTypeArguments);
            }

            var descriptor = _typeDescriptorProvider.Get(implType);
            if (descriptor == null || descriptor.Constructors == null || descriptor.Constructors.Length == 0)
            {
                return null;
            }

            var instance = Resolve(resolvingContext, descriptor);
            if (instance == null)
            {
                return instance;
            }

            if (resolvingContext is ResolvingEnumerableContext eResolvingContext)
            {
                ResloveElements(instance as IList, eResolvingContext.ElementsResolvingContext);
            }

            return instance;
        }

        public object Resolve(IResolvingContext resolvingContext, TypeDescriptor descriptor)
        {
            var serviceFactoryBuildContext = CreateServiceActivatingContext(resolvingContext, descriptor);
            if (serviceFactoryBuildContext == null)
            {
                return null;
            }

            return _instanceFacotryBuilder.Build(serviceFactoryBuildContext)(_container, _lifeScope);
        }

        public void ResloveElements(IList list, List<IResolvingContext> elesContext)
        {
            if (list == null)
            {
                return;
            }

            foreach (var item in elesContext)
            {
                var ele = Resolve(item);
                if (ele != null)
                {
                    list.Add(ele);
                }
            }
        }

        public ServiceFactoryBuildContext CreateServiceActivatingContext(IResolvingContext resolvingContext, TypeDescriptor descriptor)
        {
            var parameters = new List<ServiceFactoryBuildParamterContext>();

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
                        parameters.Add(new ServiceFactoryBuildParamterContext()
                        {
                            Parameter = parameter,
                            ParameterTypeContext = new ServiceFactoryBuildContext()
                            {
                                Constructor = null,
                                ResolvingContext = context,
                                ParamtersContext = null
                            }
                        });

                        continue;
                    }

                    var implType = context.Component.ImplementionType;
                    if (implType.IsGenericType && context.ReslovingType.IsConstructedGenericType)
                    {
                        implType = implType.MakeGenericType(context.ReslovingType.GenericTypeArguments);
                    }

                    var typeDescriptor = _typeDescriptorProvider.Get(implType);
                    var parameterTypeContext = CreateServiceActivatingContext(context, typeDescriptor);

                    if (parameterTypeContext == null)
                    {
                        parameters.Clear();
                        break;
                    }

                    parameters.Add(new ServiceFactoryBuildParamterContext()
                    {
                        Parameter = parameter,
                        ParameterTypeContext = parameterTypeContext
                    });
                }

                if (parameters.Count == item.Paramters.Length)
                {
                    return new ServiceFactoryBuildContext()
                    {
                        Constructor = item,
                        ResolvingContext = resolvingContext,
                        ParamtersContext = parameters.ToArray()
                    };
                }
            }

            return null;
        }
    }
}
