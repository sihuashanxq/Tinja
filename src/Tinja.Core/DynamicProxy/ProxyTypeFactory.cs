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
        private readonly IMemberMetadataProvider _metadataProvider;

        private readonly IDynamicProxyConfiguration _configuration;

        private readonly IEnumerable<IProxyTypeGenerationReferee> _referees;

        public ProxyTypeFactory(
            IMemberMetadataProvider metadataProvider,
            IDynamicProxyConfiguration configuration,
            IEnumerable<IProxyTypeGenerationReferee> referees
        )
        {
            _referees = referees ?? throw new NullReferenceException(nameof(_referees));
            _configuration = configuration ?? throw new NullReferenceException(nameof(_configuration));
            _metadataProvider = metadataProvider ?? throw new NullReferenceException(nameof(metadataProvider));
        }

        public virtual Type CreateProxyType(Type typeInfo)
        {
            if (typeInfo == null)
            {
                throw new NullReferenceException(nameof(typeInfo));
            }

            if (typeInfo.IsInterface && !_configuration.EnableInterfaceProxy)
            {
                return null;
            }

            if (typeInfo.IsAbstract && !_configuration.EnableAstractionClassProxy)
            {
                return null;
            }

            var metadatas = _metadataProvider.GetMemberMetadatas(typeInfo).Where(item => ShouldProxy(item.Member)).ToArray();
            if (metadatas.Length == 0)
            {
                return null;
            }

            if (typeInfo.IsInterface)
            {
                return new InterfaceProxyTypeGenerator(typeInfo, metadatas).CreateProxyType();
            }

            return new ClassProxyTypeGenerator(typeInfo, metadatas).CreateProxyType();
        }

        protected virtual bool ShouldProxy(MemberInfo memberInfo)
        {
            return _referees.Any(referee => referee.ShouldProxy(memberInfo));
        }
    }
}
