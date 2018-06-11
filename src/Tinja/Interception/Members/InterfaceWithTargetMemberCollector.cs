using System;
using System.Linq;
using System.Reflection;
using Tinja.Extensions;

namespace Tinja.Interception.Members
{
    public class InterfaceWithTargetMemberCollector : MemberCollector
    {
        public InterfaceWithTargetMemberCollector(Type interfaceTargetType)
            : base(interfaceTargetType)
        {
            if (!interfaceTargetType.IsClass)
            {
                throw new InvalidOperationException($"Type:{interfaceTargetType.FullName} is not a class type");
            }
        }

        protected override void CollectTypeMethods()
        {
            foreach (var methodInfo in ProxyTargetType.GetMethods(BindingFlag))
            {
                HandleCollectedMemberInfo(methodInfo);
            }
        }

        protected override void CollectTypeProperties()
        {
            foreach (var property in ProxyTargetType.GetProperties(BindingFlag))
            {
                HandleCollectedMemberInfo(property);
            }
        }

        protected override void CollectTypeEvents()
        {
            foreach (var @event in ProxyTargetType.GetEvents(BindingFlag))
            {
                HandleCollectedMemberInfo(@event);
            }
        }

        protected override void HandleCollectedMemberInfo(MemberInfo memberInfo)
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

            if (mapMembers != null && mapMembers.Length != 0)
            {
                ProxyMembers.Add(new ProxyMember
                {
                    Member = memberInfo,
                    InterfaceMembers = mapMembers,
                    Interfaces = mapMembers.Select(i => i.DeclaringType)
                });
            }
        }
    }
}
