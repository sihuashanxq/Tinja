using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tinja.Interception.TypeMembers
{
    public class InterfaceTargetTypeMemberCollector : TypeMemberCollector
    {
        public InterfaceTargetTypeMemberCollector(Type baseType, Type targetType)
            : base(baseType, targetType)
        {

        }

        protected override void CollectTypeMethods()
        {
            foreach (var methodInfo in TargetMethods)
            {
                HandleCollectedMemberInfo(methodInfo);
            }
        }

        protected override void CollectTypeProperties()
        {
            foreach (var property in TargetProperties)
            {
                HandleCollectedMemberInfo(property);
            }
        }

        protected override void CollectTypeEvents()
        {
            foreach (var @event in TargetType.GetEvents(BindingFlag))
            {
                HandleCollectedMemberInfo(@event);
            }
        }

        protected override void HandleCollectedMemberInfo(MemberInfo memberInfo)
        {
            var mapMembers = null as IEnumerable<MemberInfo>;

            switch (memberInfo)
            {
                case MethodInfo methodInfo:
                    mapMembers = methodInfo.GetInterfaceMapMembers(TargetInterfaces);
                    break;
                case PropertyInfo propertyInfo:
                    mapMembers = propertyInfo.GetInterfaceMapMembers(TargetInterfaces);
                    break;
                case EventInfo eventInfo:
                    mapMembers = eventInfo.GetInterfaceMapMembers(TargetInterfaces);
                    break;
                default:
                    break;
            }

            if (mapMembers == null || !mapMembers.Any())
            {
                return;
            }

            var typeMemberInfo = new TypeMember
            {
                Member = memberInfo,
                InterfaceMembers = mapMembers,
                Interfaces = mapMembers.Select(i => i.DeclaringType)
            };

            Members.Add(typeMemberInfo);
        }
    }
}
