using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception.TypeMembers
{
    public class TypeMemberMetadata
    {
        public Type ImplementionType { get; set; }

        public MemberInfo ImplementionMemberInfo { get; set; }

        public IEnumerable<Type> DeclareTypes { get; set; }

        public IEnumerable<MemberInfo> DeclareMemberInfos { get; set; }

        public IEnumerable<InterceptorDeclare> InterceptorDeclares { get; set; }
    }
}
