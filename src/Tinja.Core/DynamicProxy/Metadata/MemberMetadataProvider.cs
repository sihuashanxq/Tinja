using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Core.DynamicProxy.Metadata;

namespace Tinja.Core.DynamicProxy.Members
{
    public class MemberMetadataProvider : IMemberMetadataProvider
    {
        private static readonly IMemberMetadataCollector ClassTypeMemberCollector = new ClassTypeMemberMetadataCollector();

        private static readonly IMemberMetadataCollector InterfaceTypeMemberCollector = new InterfaceTypeMemberMetadataCollector();

        private readonly ConcurrentDictionary<Type, IEnumerable<MemberMetadata>> _typeMetadatas;

        public MemberMetadataProvider()
        {
            _typeMetadatas = new ConcurrentDictionary<Type, IEnumerable<MemberMetadata>>();
        }

        public IEnumerable<MemberMetadata> GetMemberMetadatas(Type typeInfo)
        {
            if (typeInfo == null)
            {
                throw new NullReferenceException(nameof(typeInfo));
            }

            return _typeMetadatas.GetOrAdd(typeInfo, type => type.IsClass ? ClassTypeMemberCollector.Collect(type) : InterfaceTypeMemberCollector.Collect(type));
        }
    }
}
