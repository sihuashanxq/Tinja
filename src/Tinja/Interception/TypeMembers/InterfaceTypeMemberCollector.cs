using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tinja.Interception.TypeMembers
{
    public class InterfaceTypeMemberCollector : TypeMemberCollector
    {
        protected Type[] Interfaces { get; }

        protected IEnumerable<PropertyInfo> ImplementionDeclaredProperties { get; }

        public InterfaceTypeMemberCollector(
            Type declareType,
            Type implementionType
        ) : base(declareType, implementionType)
        {
            Interfaces = implementionType.GetInterfaces();
            ImplementionDeclaredProperties = GetProperties(implementionType);
        }

        protected override void CollectMethods()
        {
            foreach (var item in Interfaces)
            {
                var mapping = ImplementionType.GetInterfaceMap(item);
                if (mapping.InterfaceMethods.Length == 0)
                {
                    continue;
                }

                for (var i = 0; i < mapping.InterfaceMethods.Length; i++)
                {
                    var targetMethod = mapping.TargetMethods[i];
                    if (ImplementionDeclaredProperties.Any(p =>
                            p.GetMethod == targetMethod ||
                            p.SetMethod == targetMethod)
                        )
                    {
                        continue;
                    }

                    Methods.Add(new TypeMemberMetadata()
                    {
                        DeclareMemberInfo = mapping.InterfaceMethods[i],
                        ImplementionMemberInfo = mapping.TargetMethods[i],
                        DeclareType = mapping.InterfaceType,
                        ImplementionType = ImplementionType
                    });
                }
            }
        }

        protected override void CollectProperties()
        {
            foreach (var item in Interfaces)
            {
                foreach (var property in item.GetProperties())
                {
                    var classProperty = ImplementionDeclaredProperties.FirstOrDefault(i => i.Name == property.Name);
                    if (classProperty == null)
                    {
                        continue;
                    }

                    Properties.Add(new TypeMemberMetadata()
                    {
                        ImplementionMemberInfo = classProperty,
                        ImplementionType = ImplementionType,
                        DeclareType = item,
                        DeclareMemberInfo = property
                    });
                }
            }
        }
    }
}
