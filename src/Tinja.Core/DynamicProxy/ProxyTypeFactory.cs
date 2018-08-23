using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Configurations;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Core.DynamicProxy.Generators;

namespace Tinja.Core.DynamicProxy
{
    public class ProxyTypeFactory : IProxyTypeFactory
    {
        private readonly IMemberMetadataProvider _memberMetadataProvider;

        private readonly IDynamicProxyConfiguration _dynamicProxyConfiguration;

        private readonly IEnumerable<IProxyTypeGenerationReferee> _proxyTypeGenerationReferees;

        public ProxyTypeFactory(
            IMemberMetadataProvider provider,
            IDynamicProxyConfiguration configuration,
            IEnumerable<IProxyTypeGenerationReferee> referees
        )
        {
            _memberMetadataProvider = provider ?? throw new NullReferenceException(nameof(provider));
            _dynamicProxyConfiguration = configuration ?? throw new NullReferenceException(nameof(configuration));
            _proxyTypeGenerationReferees = referees ?? throw new NullReferenceException(nameof(referees));
        }

        public virtual Type CreateProxyType(Type typeInfo)
        {
            if (typeInfo == null)
            {
                throw new NullReferenceException(nameof(typeInfo));
            }

            if (typeInfo.IsInterface && !_dynamicProxyConfiguration.EnableInterfaceProxy)
            {
                return null;
            }

            if (typeInfo.IsAbstract && !_dynamicProxyConfiguration.EnableAstractionClassProxy)
            {
                return null;
            }

            var members = _memberMetadataProvider.GetMembers(typeInfo).Where(item => ShouldProxy(item.Member)).ToArray();
            if (members.Length == 0)
            {
                return null;
            }

            if (typeInfo.IsInterface)
            {
                return new InterfaceProxyTypeGenerator(typeInfo, members).BuildProxyType();
            }

            return new ClassProxyTypeGenerator(typeInfo, members).BuildProxyType();
        }

        protected virtual bool ShouldProxy(MemberInfo memberInfo)
        {
            return _proxyTypeGenerationReferees.Any(referee => referee.ShouldProxy(memberInfo));
        }
    }
}
