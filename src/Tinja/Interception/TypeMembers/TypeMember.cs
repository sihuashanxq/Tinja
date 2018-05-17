using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception.TypeMembers
{
    public class TypeMember
    {
        public MemberInfo Member { get; set; }

        public IEnumerable<Type> Interfaces { get; set; }

        public IEnumerable<MemberInfo> InterfaceMembers { get; set; }

        public Type DeclaringType => Member.DeclaringType;

        public bool IsProperty =>
             Member.MemberType == MemberTypes.Property;

        public bool IsEvent =>
            Member.MemberType == MemberTypes.Event;

        public bool IsMethod =>
            Member.MemberType == MemberTypes.Method;
    }
}
