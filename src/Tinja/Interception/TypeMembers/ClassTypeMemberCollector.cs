using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tinja.Interception.TypeMembers
{
    public class ClassTypeMemberCollector : TypeMemberCollector
    {
        public ClassTypeMemberCollector(Type declareType, Type implementionType)
            : base(declareType, implementionType)
        {

        }

        protected override void CollectMethods()
        {
            foreach (var item in ImplementionType.GetMethods(new[] { typeof(object) }).Where(i => i.IsVirtual && !i.IsPrivate))
            {
                var baseDefinitions = GetBaseDefinition(item);
                var typeMember = new TypeMemberMetadata()
                {
                    ImplementionType = ImplementionType,
                    ImplementionMemberInfo = item,
                    BaseTypes = baseDefinitions.Select(i => i.DeclaringType),
                    BaseMemberInfos = baseDefinitions
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

                var baseDefinitions = GetBaseDefinition(methodInfo);
                var baseTypes = baseDefinitions.Select(i => i.DeclaringType);
                var list = new List<MemberInfo>();

                foreach (var property in baseTypes.Select(baseType => baseType.GetProperty(item.Name)))
                {
                    list.Add(property);
                }

                var typeMember = new TypeMemberMetadata()
                {
                    ImplementionType = ImplementionType,
                    ImplementionMemberInfo = item,
                    BaseTypes = baseTypes,
                    BaseMemberInfos = list
                };

                AddCollectedPropertyInfo(typeMember);
            }
        }
    }
}
