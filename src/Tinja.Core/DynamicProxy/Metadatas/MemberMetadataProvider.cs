using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Tinja.Abstractions.DynamicProxy.Metadatas;

namespace Tinja.Core.DynamicProxy.Metadatas
{
    public class MemberMetadataProvider : IMemberMetadataProvider
    {
        private static readonly IMemberMetadataCollector ClassTypeMemberCollector = new ClassMemberMetadataCollector();

        private static readonly IMemberMetadataCollector InterfaceTypeMemberCollector = new InterfaceMemberMetadataCollector();

        private readonly ConcurrentDictionary<Type, IEnumerable<MemberMetadata>> _metadatas;

        public MemberMetadataProvider()
        {
            _metadatas = new ConcurrentDictionary<Type, IEnumerable<MemberMetadata>>();
        }

        public IEnumerable<MemberMetadata> GetMemberMetadatas(Type typeInfo)
        {
            if (typeInfo == null)
            {
                throw new NullReferenceException(nameof(typeInfo));
            }

            return _metadatas.GetOrAdd(typeInfo, type => type.IsClass ? ClassTypeMemberCollector.Collect(type) : InterfaceTypeMemberCollector.Collect(type));
        }
    }
}
