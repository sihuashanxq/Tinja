using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Configurations;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Abstractions.Extensions;
using Tinja.Core.DynamicProxy.Generators;

namespace Tinja.Core.DynamicProxy
{
    /// <summary>
    /// the default implementation of <see cref="IProxyTypeFactory"/>
    /// </summary>
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

        /// <summary>
        /// create a proxy type of the give type
        /// </summary>
        /// <param name="typeInfo">a <see cref="Type"/></param>
        /// <returns>the generated proxy type</returns>
        public virtual Type CreateProxyType(Type typeInfo)
        {
            if (typeInfo == null)
            {
                throw new NullReferenceException(nameof(typeInfo));
            }

            //type of IInterceptor can't be intercepted
            if (typeInfo.IsType(typeof(IInterceptor)))
            {
                return null;
            }

            //type marked with DisableProxyAttribute can't be intercepted
            if (typeInfo.GetCustomAttribute<DisableProxyAttribute>() != null)
            {
                return null;
            }

            //pure interface can't be intercepted when configuration of EnableInterfaceProxy was not enabled
            if (typeInfo.IsInterface && !_dynamicProxyConfiguration.EnableInterfaceProxy)
            {
                return null;
            }

            //pure abstract-class can't be intercepted when configuration of EnableAstractionClassProxy was not enabled
            if (typeInfo.IsAbstract && !_dynamicProxyConfiguration.EnableAstractionClassProxy)
            {
                return null;
            }

            //type can't be intercepted when none of any Membermetadata was avaliabled
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

        /// <summary>
        /// judge a member of type can be intercpeted
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        protected virtual bool ShouldProxy(MemberInfo memberInfo)
        {
            return _proxyTypeGenerationReferees.Any(referee => referee.ShouldProxy(memberInfo));
        }
    }
}
