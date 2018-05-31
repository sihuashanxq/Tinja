using System;
using System.Reflection;

namespace Tinja.Interception.Members
{
    public class InterfaceMemberCollector : MemberCollector
    {
        protected Type InterfaceType { get; }

        public InterfaceMemberCollector(Type interfaceType)
            : base(interfaceType)
        {
            if (!interfaceType.IsInterface)
            {
                throw new InvalidOperationException($"Type:{interfaceType.FullName} is not an interface type");
            }

            InterfaceType = interfaceType;
        }

        protected override void CollectTypeProperties()
        {
            foreach (var item in InterfaceType.GetProperties())
            {
                HandleCollectedMemberInfo(item);
            }
        }

        protected override void CollectTypeMethods()
        {
            foreach (var item in InterfaceType.GetMethods())
            {
                HandleCollectedMemberInfo(item);
            }
        }

        protected override void CollectTypeEvents()
        {
            foreach (var item in InterfaceType.GetEvents())
            {
                HandleCollectedMemberInfo(item);
            }
        }

        protected override void HandleCollectedMemberInfo(MemberInfo memberInfo)
        {
            ProxyMembers.Add(new ProxyMember()
            {
                Member = memberInfo,
                InterfaceMembers = new MemberInfo[0],
                Interfaces = Interfaces
            });
        }
    }
}
