using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy.Metadatas;

namespace Tinja.Core.DynamicProxy.Metadatas
{
    public class InterceptorMetadataProvider : IInterceptorMetadataProvider
    {
        private readonly IMemberMetadataProvider _provider;

        private readonly IEnumerable<IInterceptorMetadataCollector> _collectors;

        public InterceptorMetadataProvider(IMemberMetadataProvider provider, IEnumerable<IInterceptorMetadataCollector> collectors)
        {
            _provider = provider ?? throw new NullReferenceException(nameof(provider));
            _collectors = collectors ?? throw new NullReferenceException(nameof(collectors));
        }

        public IEnumerable<InterceptorMetadata> GetMetadatas(MemberInfo memberInfo)
        {
            var allMembers = _provider.GetMemberMetadatas(memberInfo.DeclaringType);
            if (allMembers == null)
            {
                throw new NullReferenceException(nameof(allMembers));
            }

            var member = allMembers.FirstOrDefault(item => item.Member == memberInfo);
            if (member == null)
            {
                return new InterceptorMetadata[0];
            }

            return _collectors.SelectMany(item => item.Collect(member));
        }
    }
}
