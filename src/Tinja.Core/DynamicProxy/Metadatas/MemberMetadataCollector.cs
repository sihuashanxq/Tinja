using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Abstractions.Extensions;

namespace Tinja.Core.DynamicProxy.Metadatas
{
    public abstract class MemberMetadataCollector : IMemberMetadataCollector
    {
        protected const BindingFlags BindingFlag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public virtual IEnumerable<MemberMetadata> Collect(Type typeInfo)
        {
            var interfaces = typeInfo.GetInterfaces() ?? Type.EmptyTypes;

            foreach (var item in CollectEvents(typeInfo, interfaces))
            {
                yield return item;
            }

            foreach (var item in CollectMethods(typeInfo, interfaces))
            {
                yield return item;
            }

            foreach (var item in CollectProperties(typeInfo, interfaces))
            {
                yield return item;
            }
        }

        protected virtual IEnumerable<MemberMetadata> CollectEvents(Type typeInfo, Type[] interfaces)
        {
            return MemberMetadata.EmptyMembers;
        }

        protected virtual IEnumerable<MemberMetadata> CollectMethods(Type typeInfo, Type[] interfaces)
        {
            return MemberMetadata.EmptyMembers;
        }

        protected virtual IEnumerable<MemberMetadata> CollectProperties(Type typeInfo, Type[] interfaces)
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
                InterfaceInherits = memberInfo.GetInterfaceMaps(interfaces).Select(item => new MemberMetadata(item)).ToArray()
            };
        }
    }
}
