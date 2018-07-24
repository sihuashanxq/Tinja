using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy.Definitions;
using Tinja.Abstractions.Injection;

namespace Tinja.Abstractions.DynamicProxy
{
    /// <summary>
    /// Transient
    /// </summary>
    public class InterceptorAccessor : IInterceptorAccessor
    {
        private readonly IServiceResolver _serviceResolver;

        private readonly IInterceptorSelectorProvider _interceptorSelectorProvider;

        private readonly IInterceptorDefinitionProvider _interceptorDefinitionProvider;

        private readonly ConcurrentDictionary<Type, InterceptorEntry> _interceptors;

        private readonly ConcurrentDictionary<MemberInfo, IInterceptor[]> _memberInterceptors;

        public InterceptorAccessor(
            IServiceResolver serviceResolver,
            IInterceptorSelectorProvider interceptorSelectorProvider,
            IInterceptorDefinitionProvider interceptorDefinitionProvider
        )
        {
            _serviceResolver = serviceResolver ?? throw new NullReferenceException(nameof(serviceResolver));
            _interceptorSelectorProvider = interceptorSelectorProvider ?? throw new NullReferenceException(nameof(interceptorSelectorProvider));
            _interceptorDefinitionProvider = interceptorDefinitionProvider ?? throw new NullReferenceException(nameof(interceptorSelectorProvider));

            _interceptors = new ConcurrentDictionary<Type, InterceptorEntry>();
            _memberInterceptors = new ConcurrentDictionary<MemberInfo, IInterceptor[]>();
        }

        public IInterceptor[] GetOrCreateInterceptors(MemberInfo memberInfo)
        {
            if (_memberInterceptors.TryGetValue(memberInfo, out var memberInterceptors))
            {
                return memberInterceptors;
            }

            lock (this)
            {
                var interceptors = new List<InterceptorEntry>();
                var definitions = _interceptorDefinitionProvider.GetInterceptors(memberInfo) ?? new InterceptorDefinition[0];

                foreach (var interceptorDefinition in definitions)
                {
                    if (!_interceptors.TryGetValue(interceptorDefinition.InterceptorType, out var entry))
                    {
                        var interceptor = _serviceResolver.Resolve(interceptorDefinition.InterceptorType);
                        if (interceptor == null)
                        {
                            throw new NullReferenceException(nameof(interceptor));
                        }

                        entry = new InterceptorEntry((IInterceptor)interceptor, interceptorDefinition);
                    }

                    interceptors.Add(entry);
                }

                return _memberInterceptors[memberInfo] = GetMemberInterceptors(memberInfo, interceptors);
            }
        }

        private IInterceptor[] GetMemberInterceptors(MemberInfo memberInfo, IEnumerable<InterceptorEntry> interceptorEntries)
        {
            var sortedInterceptors = interceptorEntries
                .OrderByDescending(item => item.Definition.Order)
                .Select(item => item.Interceptor)
                .ToArray();

            return _interceptorSelectorProvider
                .GetSelectors(memberInfo)
                .Aggregate(
                    sortedInterceptors,
                    (current, selector) => selector.Select(memberInfo, current)
                );
        }
    }
}
