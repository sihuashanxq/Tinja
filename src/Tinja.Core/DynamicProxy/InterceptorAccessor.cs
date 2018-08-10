using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.DynamicProxy
{
    public class InterceptorAccessor : IInterceptorAccessor
    {
        private bool _initialized;

        internal IServiceResolver ServieResolver { get; }

        internal IInterceptorFactory InterceptorFactory { get; set; }

        internal IInterceptorSelectorProvider InterceptorSelectorProvider { get; set; }

        internal IInterceptorMetadataProvider InterceptorMetadataProvider { get; set; }

        internal Dictionary<Type, InterceptorEntry> TypeInterceptors { get; set; }

        internal Dictionary<MemberInfo, IInterceptor[]> MemberInterceptors { get; set; }

        public InterceptorAccessor(IServiceResolver serviceResolver)
        {
            ServieResolver = serviceResolver ?? throw new NullReferenceException(nameof(serviceResolver));
        }

        public IInterceptor[] GetOrCreateInterceptors(MemberInfo memberInfo)
        {
            if (!_initialized)
            {
                lock (this)
                {
                    _initialized = true;
                    TypeInterceptors = new Dictionary<Type, InterceptorEntry>();
                    MemberInterceptors = new Dictionary<MemberInfo, IInterceptor[]>();
                    InterceptorFactory = ServieResolver.ResolveServiceRequired<IInterceptorFactory>();
                    InterceptorSelectorProvider = ServieResolver.ResolveServiceRequired<IInterceptorSelectorProvider>();
                    InterceptorMetadataProvider = ServieResolver.ResolveServiceRequired<IInterceptorMetadataProvider>();
                }
            }

            if (MemberInterceptors.TryGetValue(memberInfo, out var memberInterceptors))
            {
                return memberInterceptors;
            }

            lock (this)
            {
                var entries = new List<InterceptorEntry>();
                var metadatas = InterceptorMetadataProvider.GetMetadatas(memberInfo) ?? new InterceptorMetadata[0];

                foreach (var metadata in metadatas.Where(item => item != null))
                {
                    if (!TypeInterceptors.TryGetValue(metadata.InterceptorType, out var entry))
                    {
                        var interceptor = InterceptorFactory.Create(metadata.InterceptorType);
                        if (interceptor == null)
                        {
                            throw new NullReferenceException($"Create interceptor:{metadata.InterceptorType.FullName}");
                        }

                        entry = new InterceptorEntry(interceptor, metadata);
                    }

                    entries.Add(entry);
                }

                return MemberInterceptors[memberInfo] = GetInterceptors(memberInfo, entries);
            }
        }

        private IInterceptor[] GetInterceptors(MemberInfo memberInfo, IEnumerable<InterceptorEntry> entries)
        {
            if (memberInfo == null)
            {
                throw new NullReferenceException(nameof(memberInfo));
            }

            if (entries == null)
            {
                throw new NullReferenceException(nameof(entries));
            }

            //sort
            var selectors = InterceptorSelectorProvider.GetSelectors(memberInfo);
            if (selectors == null)
            {
                return entries.OrderByDescending(item => item.Metadata.Order).Select(item => item.Interceptor).ToArray();
            }

            var interceptors = entries.OrderByDescending(item => item.Metadata.Order).Select(item => item.Interceptor);

            return selectors.Aggregate(interceptors, (current, selector) => selector.Select(memberInfo, current)).ToArray();
        }
    }
}
