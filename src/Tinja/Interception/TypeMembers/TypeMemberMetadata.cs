using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception.TypeMembers
{
    public class TypeMemberMetadata
    {
        public Type DeclareType { get; set; }

        public Type ImplementionType { get; set; }

        public MemberInfo DeclareMemberInfo { get; set; }

        public MemberInfo ImplementionMemberInfo { get; set; }

        public IEnumerable<InterceptorDeclare> InterceptorDeclares { get; set; }
    }
}
