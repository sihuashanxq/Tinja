using System;
using System.Linq;

namespace Tinja.Interception.TypeMembers
{
    public class ClassTypeMemberCollector : TypeMemberCollector
    {
        public ClassTypeMemberCollector(Type baseType, Type targetType) : base(baseType, targetType)
        {

        }

        protected override void CollectMethods()
        {
            foreach (var methodInfo in TargetMethods.Where(m => m.IsOverrideable()))
            {
                HandleCollectedTypeMember(methodInfo);
            }
        }

        protected override void CollectProperties()
        {
            foreach (var property in TargetProperties.Where(i => i.IsOverrideable()))
            {
                HandleCollectedTypeMember(property);
            }
        }
    }
}
