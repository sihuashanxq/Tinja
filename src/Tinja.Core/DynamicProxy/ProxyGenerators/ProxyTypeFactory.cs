using System;
using Tinja.Abstractions.Configuration;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Abstractions.DynamicProxy.ProxyGenerators;

namespace Tinja.Core.DynamicProxy.ProxyGenerators
{
    public class ProxyTypeFactory : IProxyTypeFactory
    {
        private readonly IMemberProxyTypeGenerationReferee _referee;

        private readonly IMemberMetadataProvider _memberMetadataProvider;

        private readonly IServiceConfiguration _serviceConfiguration;

        public ProxyTypeFactory(
            IMemberMetadataProvider metadataProvider,
            IMemberProxyTypeGenerationReferee referee,
            IServiceConfiguration serviceConfiguration
        )
        {
            _referee = referee;
            _memberMetadataProvider = metadataProvider;
            _serviceConfiguration = serviceConfiguration;
        }

        public virtual Type CreateProxyType(Type typeInfo)
        {
            if (typeInfo == null)
            {
                throw new NullReferenceException(nameof(typeInfo));
            }

            if (!_serviceConfiguration.Interception.EnableInterception)
            {
                return null;
            }

            var metadatas = _memberMetadataProvider.GetMemberMetadatas(typeInfo);
            if (metadatas == null)
            {
                return null;
            }

            if (typeInfo.IsInterface)
            {
                return new InterfaceProxyTypeGenerator(typeInfo, metadatas).CreateProxyType();
            }

            return new ClassProxyTypeGenerator(typeInfo, metadatas).CreateProxyType();
        }
    }
}
