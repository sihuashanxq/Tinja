using System;
using System.Collections.Generic;

namespace Tinja.Interception.TypeMembers
{
    public class InterfaceTypeMemberCollector : TypeMemberCollector
    {
        public InterfaceTypeMemberCollector(Type baseType, Type targetType) : base(baseType, targetType)
        {

        }

        public override IEnumerable<TypeMember> Collect()
        {
            CollectEvents();

            return base.Collect();
        }

        protected override void CollectMethods()
        {
            foreach (var methodInfo in TargetMethods)
            {
                HandleCollectedTypeMember(methodInfo);
            }
        }

        protected override void CollectProperties()
        {
            foreach (var property in TargetProperties)
            {
                HandleCollectedTypeMember(property);
            }
        }

        protected virtual void CollectEvents()
        {
            foreach (var @event in TargetType.GetEvents(BindingFlag))
            {
                HandleCollectedTypeMember(@event);
            }
        }
    }
}
