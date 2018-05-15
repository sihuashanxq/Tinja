using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tinja.Interception.TypeMembers
{
    public class ClassTypeMemberCollector : TypeMemberCollector
    {
        public ClassTypeMemberCollector(Type baseType, Type implementionType)
            : base(baseType, implementionType)
        {

        }

        protected override void CollectMethods()
        {
            foreach (var item in ImplementionType.GetMethods(new[] { typeof(object) }).Where(i => i.IsVirtual && !i.IsPrivate))
            {
                var definitions = GetBaseDefinition(item);
                var typeMember = new TypeMemberMetadata()
                {
                    ImplementionType = ImplementionType,
                    ImplementionMember = item,
                    BaseTypes = definitions.Select(i => i.DeclaringType),
                    BaseMembers = definitions
                };

                AddCollectedMethodInfo(typeMember);
            }
        }

        protected override void CollectProperties()
        {
            foreach (var item in ImplementionType.GetProperties(new[] { typeof(object) }))
            {
                MethodInfo methodInfo = null;
                if (item.CanRead && item.GetMethod.IsVirtual && !item.GetMethod.IsPrivate)
                {
                    methodInfo = item.GetMethod;
                }
                else if (item.CanWrite && item.SetMethod.IsVirtual && !item.SetMethod.IsPrivate)
                {
                    methodInfo = item.SetMethod;
                }

                if (methodInfo == null)
                {
                    continue;
                }

                var definitions = GetBaseDefinition(methodInfo);
                var baseTypes = definitions.Select(i => i.DeclaringType);
                var list = new List<MemberInfo>();

                foreach (var property in baseTypes.Select(baseType => baseType.GetProperty(item.Name)))
                {
                    list.Add(property);
                }

                var typeMember = new TypeMemberMetadata()
                {
                    ImplementionType = ImplementionType,
                    ImplementionMember = item,
                    BaseTypes = baseTypes,
                    BaseMembers = list
                };

                AddCollectedPropertyInfo(typeMember);
            }
        }
    }
}
