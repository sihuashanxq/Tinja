using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception.TypeMembers
{
    public class TypeMemberMetadata
    {
        public Type ImplementionType { get; set; }

        public MemberInfo ImplementionMemberInfo { get; set; }

        public IEnumerable<Type> BaseTypes { get; set; }

        public IEnumerable<MemberInfo> BaseMemberInfos { get; set; }

        public IEnumerable<InterceptorBinding> InterceptorBindings { get; set; }
    }
}
