using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tinja.Interception.Members
{
    public class ProxyMember
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

        public IEnumerable<MemberInfo> GetInterceptorCollectTargets()
        {
            return Interfaces.Concat(InterfaceMembers).Concat(new[] { Member }).Concat(new[] { Member.DeclaringType });
        }
    }
}
