using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.Injection.Extensions;

namespace Tinja.Abstractions.DynamicProxy
{
    public abstract class TypeMemberCollector : ITypeMemberCollector
    {
        protected const BindingFlags BindingFlag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public virtual IEnumerable<MemberMetadata> Collect(Type typeInfo)
        {
            var members = new List<MemberMetadata>();
            var interfaces = typeInfo.GetInterfaces();

            var events = CollectTypeEvents(typeInfo, interfaces);
            var methods = CollectTypeMethods(typeInfo, interfaces);
            var properties = CollectTypeProperties(typeInfo, interfaces);

            if (events != null)
            {
                members.AddRange(events);
            }

            if (methods != null)
            {
                members.AddRange(methods);
            }

            if (properties != null)
            {
                members.AddRange(properties);
            }

            return members;
        }

        protected abstract IEnumerable<MemberMetadata> CollectTypeMethods(Type typeInfo, Type[] interfaces);

        protected abstract IEnumerable<MemberMetadata> CollectTypeProperties(Type typeInfo, Type[] interfaces);

        protected virtual IEnumerable<MemberMetadata> CollectTypeEvents(Type typeInfo, Type[] interfaces)
        {
            return MemberMetadata.EmptyMembers;
        }

        protected virtual MemberMetadata CreateMemberMetadata(MemberInfo memberInfo, Type[] interfaces)
        {
            if (memberInfo == null)
            {
                throw new NullReferenceException(nameof(memberInfo));
            }

            return new MemberMetadata(memberInfo)
            {
                InterfaceInherits = memberInfo
                    .GetInterfaceMaps(interfaces)
                    .Select(item => new MemberMetadata(item))
                    .ToArray()
            };
        }
    }
}
