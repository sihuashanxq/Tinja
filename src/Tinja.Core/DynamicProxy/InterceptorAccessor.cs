using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Metadatas;

namespace Tinja.Core.DynamicProxy
{
    /// <summary>
    /// Transient
    /// </summary>
    public class InterceptorAccessor : IInterceptorAccessor
    {
        private readonly IInterceptorFactory _interceptorFactory;

        private readonly IInterceptorSelectorProvider _selectorProvider;

        private readonly IInterceptorMetadataProvider _metadataProvider;

        private readonly Dictionary<Type, InterceptorEntry> _interceptors;

        private readonly Dictionary<MemberInfo, IInterceptor[]> _memberInterceptors;

        public InterceptorAccessor(
            IInterceptorFactory interceptorFactory,
            IInterceptorSelectorProvider selectorProvider,
            IInterceptorMetadataProvider metadataProvider
        )
        {
            _selectorProvider = selectorProvider ?? throw new NullReferenceException(nameof(selectorProvider));
            _metadataProvider = metadataProvider ?? throw new NullReferenceException(nameof(selectorProvider));
            _interceptorFactory = interceptorFactory ?? throw new NullReferenceException(nameof(interceptorFactory));

            _interceptors = new Dictionary<Type, InterceptorEntry>();
            _memberInterceptors = new Dictionary<MemberInfo, IInterceptor[]>();
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
                var interceptorMetadatas = _metadataProvider.GetInterceptorMetadatas(memberInfo) ?? new InterceptorMetadata[0];

                foreach (var metadata in interceptorMetadatas)
                {
                    if (metadata == null)
                    {
                        continue;
                    }

                    if (!_interceptors.TryGetValue(metadata.InterceptorType, out var item))
                    {
                        var interceptor = _interceptorFactory.Create(metadata.InterceptorType);
                        if (interceptor == null)
                        {
                            throw new NullReferenceException(nameof(interceptor));
                        }

                        item = new InterceptorEntry((IInterceptor)interceptor, metadata);
                    }

                    interceptors.Add(item);
                }

                return _memberInterceptors[memberInfo] = GetInterceptors(memberInfo, interceptors);
            }
        }

        private IInterceptor[] GetInterceptors(MemberInfo memberInfo, IEnumerable<InterceptorEntry> interceptors)
        {
            //sort
            var sortedInterceptors = interceptors
                .OrderByDescending(item => item.Metadata.Order)
                .Select(item => item.Interceptor)
                .ToArray();

            //select
            return _selectorProvider
                .GetSelectors(memberInfo)
                .Aggregate(sortedInterceptors, (current, selector) => selector.Select(memberInfo, current));
        }
    }
}
