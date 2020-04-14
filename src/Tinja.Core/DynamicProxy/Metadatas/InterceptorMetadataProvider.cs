using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Metadatas;

namespace Tinja.Core.DynamicProxy.Metadatas
{
    [DisableProxy]
    internal class InterceptorMetadataProvider : IInterceptorMetadataProvider
    {
        private readonly IMemberMetadataProvider _provider;

        private readonly IEnumerable<IInterceptorMetadataCollector> _collectors;

        internal InterceptorMetadataProvider(IMemberMetadataProvider provider, IEnumerable<IInterceptorMetadataCollector> collectors)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _collectors = collectors ?? throw new ArgumentNullException(nameof(collectors));
        }

        public IEnumerable<InterceptorMetadata> GetInterceptors(MemberInfo memberInfo)
        {
            var members = _provider.GetMembers(memberInfo.DeclaringType);
            if (members == null)
            {
                throw new ArgumentNullException(nameof(members));
            }
            
            var metadata = members.FirstOrDefault(item => item.Member == memberInfo);
            if (metadata == null)
            {
                return new InterceptorMetadata[0];
            }

            return _collectors.SelectMany(item => item.Collect(metadata));
        }
    }
}
