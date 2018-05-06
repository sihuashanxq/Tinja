using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Resolving;

namespace Tinja.Interception
{
    public class InterceptorProvider : IInterceptorProvider
    {
        private IServiceResolver _resolver;

        private IEnumerable<IInterceptorSelector> _intereceptorSelectors;

        private ConcurrentDictionary<MethodInfo, Type[]> _methodIntereceptors;

        public InterceptorProvider(IServiceResolver resolver, IEnumerable<IInterceptorSelector> selectors)
        {
            _resolver = resolver;
            _intereceptorSelectors = selectors;
            _methodIntereceptors = new ConcurrentDictionary<MethodInfo, Type[]>();
        }

        public IEnumerable<IInterceptor> Get(object target, MethodInfo targetMethod, MethodInfo serviceMethod)
        {
            return CollectIntereceptors(target, targetMethod, serviceMethod);
        }

        private IEnumerable<IInterceptor> CollectIntereceptors(object target, MethodInfo targetMethod, MethodInfo serviceMethod)
        {
            var declaredIntereceptorTypes = FindIntereceptorTypes(targetMethod)
              .Union(FindIntereceptorTypes(serviceMethod)).Distinct();

            var list = new List<IInterceptor>();

            foreach (var declaredIntereceptorType in declaredIntereceptorTypes)
            {
                if (_resolver.Resolve(declaredIntereceptorType) is IInterceptor item)
                {
                    list.Add(item);
                }
            }

            return SelectIntereceptors(target, targetMethod, list.ToArray());
        }

        private IEnumerable<IInterceptor> SelectIntereceptors(object target, MethodInfo targetMethod, IInterceptor[] intereceptors)
        {
            foreach (var item in _intereceptorSelectors)
            {
                intereceptors = item.Select(target, targetMethod, intereceptors);
            }

            return intereceptors;
        }

        private IEnumerable<Type> FindIntereceptorTypes(MethodInfo methodInfo)
        {
            return _methodIntereceptors.GetOrAdd(methodInfo, m =>
            {
                return m
                .GetCustomAttributes<InterceptorAttribute>()
                .Concat(m.DeclaringType.GetCustomAttributes<InterceptorAttribute>())
                .Select(i => i.InterceptorType)
                .Distinct()
                .ToArray();
            });
        }
    }
}
