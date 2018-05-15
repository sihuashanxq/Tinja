using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tinja.Interception.TypeMembers
{
    public class InterfaceTypeMemberCollector : TypeMemberCollector
    {
        protected IEnumerable<PropertyInfo> ImplementedProperties { get; }

        public InterfaceTypeMemberCollector(
            Type baseType,
            Type implementionType
        ) : base(baseType, implementionType)
        {
            ImplementedProperties = implementionType.GetProperties(new[] { typeof(object) });
        }

        protected override void CollectMethods()
        {
            foreach (var item in ImplementedInterfaces)
            {
                var mapping = ImplementionType.GetInterfaceMap(item);
                if (mapping.InterfaceMethods.Length == 0)
                {
                    continue;
                }

                for (var i = 0; i < mapping.InterfaceMethods.Length; i++)
                {
                    var targetMethod = mapping.TargetMethods[i];
                    if (ImplementedProperties.Any(p =>
                            p.GetMethod == targetMethod ||
                            p.SetMethod == targetMethod)
                        )
                    {
                        continue;
                    }

                    AddCollectedMethodInfo(new TypeMemberMetadata()
                    {
                        BaseTypes = new[] { mapping.InterfaceType },
                        BaseMembers = new[] { mapping.InterfaceMethods[i] },
                        ImplementionMember = mapping.TargetMethods[i],
                        ImplementionType = ImplementionType
                    });
                }
            }
        }

        protected override void CollectProperties()
        {
            foreach (var item in ImplementedInterfaces)
            {
                foreach (var property in item.GetProperties())
                {
                    var classProperty = ImplementedProperties.FirstOrDefault(i => i.Name == property.Name);
                    if (classProperty == null)
                    {
                        continue;
                    }

                    AddCollectedPropertyInfo(new TypeMemberMetadata()
                    {
                        ImplementionMember = classProperty,
                        ImplementionType = ImplementionType,
                        BaseTypes = new[] { item },
                        BaseMembers = new[] { property }
                    });
                }
            }
        }
    }
}
