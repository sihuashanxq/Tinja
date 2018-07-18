using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;

namespace Tinja.Core.DynamicProxy.Members
{
    public class InterfaceTypeMemberCollector : TypeMemberCollector
    {
        protected override IEnumerable<MemberMetadata> CollectTypeProperties(Type typeInfo, Type[] interfaces)
        {
            return typeInfo
                .GetProperties()
                .Select(item => CreateMemberMetadata(item, interfaces));
        }

        protected override IEnumerable<MemberMetadata> CollectTypeMethods(Type typeInfo, Type[] interfaces)
        {
            return typeInfo
                .GetMethods()
                .Select(item => CreateMemberMetadata(item, interfaces));
        }

        protected override IEnumerable<MemberMetadata> CollectTypeEvents(Type typeInfo, Type[] interfaces)
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
