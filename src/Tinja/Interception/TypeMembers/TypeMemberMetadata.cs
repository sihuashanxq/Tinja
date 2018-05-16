using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception.TypeMembers
{
    public class TypeMemberMetadata
    {
        public Type ImplementionType { get; set; }

        public MemberInfo ImplementionMember { get; set; }

        public IEnumerable<Type> BaseTypes { get; set; }

        public IEnumerable<MemberInfo> BaseMembers { get; set; }

        public IEnumerable<InterceptorMetadata> InterceptorBindings { get; set; }

        public bool IsProperty =>
             ImplementionMember.MemberType == MemberTypes.Property;

        public bool IsEvent =>
            ImplementionMember.MemberType == MemberTypes.Event;

        public bool IsMethod => 
            ImplementionMember.MemberType == MemberTypes.Method;
    }
}
