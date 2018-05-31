using System;
using System.Linq;

namespace Tinja.Interception.Members
{
    public class ClassMemberCollector : MemberCollector
    {
        public ClassMemberCollector(Type classType)
            : base(classType)
        {

        }

        protected override void CollectTypeMethods()
        {
            foreach (var methodInfo in ProxyTargetType.GetMethods(BindingFlag).Where(m => m.IsOverrideable()))
            {
                HandleCollectedMemberInfo(methodInfo);
            }
        }

        /// <summary>
        /// </summary>
        protected override void CollectTypeProperties()
        {
            foreach (var property in ProxyTargetType.GetProperties(BindingFlag).Where(m => m.IsOverrideable()))
            {
                HandleCollectedMemberInfo(property);
            }
        }
    }
}
