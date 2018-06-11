using System;
using System.Linq;
using Tinja.Extensions;

namespace Tinja.Interception.Members
{
    public class ClassMemberCollector : MemberCollector
    {
        public ClassMemberCollector(Type classType)
            : base(classType)
        {

        }

        /// <inheritdoc />
        protected override void CollectTypeMethods()
        {
            foreach (var methodInfo in ProxyTargetType.GetMethods(BindingFlag).Where(m => m.IsOverrideable()))
            {
                HandleCollectedMemberInfo(methodInfo);
            }
        }

        /// <inheritdoc />
        protected override void CollectTypeProperties()
        {
            foreach (var property in ProxyTargetType.GetProperties(BindingFlag).Where(m => m.IsOverrideable()))
            {
                HandleCollectedMemberInfo(property);
            }
        }

        /// <inheritdoc />
        protected override void CollectTypeEvents()
        {
            foreach (var eventInfo in ProxyTargetType.GetEvents(BindingFlag))
            {
                if (eventInfo.AddMethod != null && eventInfo.AddMethod.IsAbstract)
                {
                    HandleCollectedMemberInfo(eventInfo);
                }
                else if (eventInfo.RemoveMethod != null && eventInfo.RemoveMethod.IsAbstract)
                {
                    HandleCollectedMemberInfo(eventInfo);
                }
                else if (eventInfo.RaiseMethod != null && eventInfo.RaiseMethod.IsAbstract)
                {
                    HandleCollectedMemberInfo(eventInfo);
                }
            }
        }
    }
}
