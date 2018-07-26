using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy.Definitions;
using Tinja.Abstractions.DynamicProxy.Metadatas;

namespace Tinja.Core.DynamicProxy.Definitions
{
    public class InterceptorDefinitionProvider : IInterceptorDefinitionProvider
    {
        private readonly IMemberMetadataProvider _metadataProvider;

        private readonly IEnumerable<IInterceptorDefinitionCollector> _collectors;

        public InterceptorDefinitionProvider(IEnumerable<IInterceptorDefinitionCollector> collectors, IMemberMetadataProvider metadataProvider)
        {
            _collectors = collectors ?? throw new NullReferenceException(nameof(collectors));
            _metadataProvider = metadataProvider ?? throw new NullReferenceException(nameof(metadataProvider));
        }

        public IEnumerable<InterceptorDefinition> GetInterceptors(MemberInfo memberInfo)
        {
            var metadatas = _metadataProvider.GetMemberMetadatas(memberInfo.DeclaringType);
            if (metadatas == null)
            {
                throw new NullReferenceException(nameof(metadatas));
            }

            var metadata = metadatas.FirstOrDefault(item => item.Member == memberInfo);
            if (metadata == null)
            {
                return new InterceptorDefinition[0];
            }

            return _collectors.SelectMany(item => item.Collect(metadata));
        }
    }
}
