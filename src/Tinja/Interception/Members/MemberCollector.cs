using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Extensions;

namespace Tinja.Interception.Members
{
    public abstract class MemberCollector : IMemberCollector
    {
        protected const BindingFlags BindingFlag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        protected Type ProxyTargetType { get; }

        protected List<ProxyMember> ProxyMembers { get; }

        protected Type[] Interfaces { get; }

        protected MemberCollector(Type proxyTargetType)
        {
            ProxyTargetType = proxyTargetType;
            Interfaces = ProxyTargetType.GetInterfaces();
            ProxyMembers = new List<ProxyMember>();
        }

        public virtual IEnumerable<ProxyMember> Collect()
        {
            CollectTypeEvents();
            CollectTypeMethods();
            CollectTypeProperties();

            return ProxyMembers;
        }

        protected abstract void CollectTypeMethods();

        /// <summary>
        /// </summary>
        protected abstract void CollectTypeProperties();

        protected virtual void CollectTypeEvents()
        {

        }

        protected virtual void HandleCollectedMemberInfo(MemberInfo memberInfo)
        {
            var mapMembers = null as MemberInfo[];

            switch (memberInfo)
            {
                case MethodInfo methodInfo:
                    mapMembers = methodInfo
                        .GetInterfaceMapMembers(Interfaces)
                        .ToArray();
                    break;
                case PropertyInfo propertyInfo:
                    mapMembers = propertyInfo
                        .GetInterfaceMapMembers(Interfaces)
                        .ToArray();
                    break;
                case EventInfo eventInfo:
                    mapMembers = eventInfo
                        .GetInterfaceMapMembers(Interfaces)
                        .ToArray();
                    break;
            }

            if (mapMembers == null)
            {
                mapMembers = new MemberInfo[0];
            }

            var typeMemberInfo = new ProxyMember
            {
                Member = memberInfo,
                InterfaceMembers = mapMembers,
                Interfaces = mapMembers.Select(i => i.DeclaringType)
            };

            ProxyMembers.Add(typeMemberInfo);
        }
    }
}
