using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy.Metadatas;

namespace Tinja.Core.DynamicProxy.Metadatas
{
    public class InterfaceMemberMetadataCollector : MemberMetadataCollector
    {
        protected override IEnumerable<MemberMetadata> CollectProperties(Type typeInfo, Type[] interfaces)
        {
            return typeInfo
                .GetProperties()
                .Select(item => CreateMemberMetadata(item, interfaces));
        }

        protected override IEnumerable<MemberMetadata> CollectMethods(Type typeInfo, Type[] interfaces)
        {
            return typeInfo
                .GetMethods()
                .Select(item => CreateMemberMetadata(item, interfaces));
        }

        protected override IEnumerable<MemberMetadata> CollectEvents(Type typeInfo, Type[] interfaces)
        {
            return typeInfo
                .GetEvents()
                .Select(item => CreateMemberMetadata(item, interfaces));
        }

        protected override MemberMetadata CreateMemberMetadata(MemberInfo memberInfo, Type[] interfaces)
        {
            return new MemberMetadata(memberInfo);
        }
    }
}
