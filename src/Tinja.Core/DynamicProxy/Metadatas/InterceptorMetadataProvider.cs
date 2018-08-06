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
            var typeMemberMetadatas = _provider.GetMemberMetadatas(memberInfo.DeclaringType);
            if (typeMemberMetadatas == null)
            {
                throw new NullReferenceException(nameof(typeMemberMetadatas));
            }
            
            var memberMetadata = typeMemberMetadatas.FirstOrDefault(item => item.Member == memberInfo);
            if (memberMetadata == null)
            {
                return new InterceptorMetadata[0];
            }

            return _collectors.SelectMany(item => item.Collect(memberMetadata));
        }
    }
}
