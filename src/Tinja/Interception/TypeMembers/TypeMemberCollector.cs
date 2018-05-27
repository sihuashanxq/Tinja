using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tinja.Interception.TypeMembers
{
    public class TypeMemberCollector : ITypeMemberCollector
    {
        protected const BindingFlags BindingFlag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        protected Type BaseType { get; }

        protected Type TargetType { get; }

        protected List<TypeMember> CollectedMembers { get; }

        protected Type[] TargetInterfaces { get; }

        protected MethodInfo[] TargetMethods { get; }

        protected PropertyInfo[] TargetProperties { get; }

        public TypeMemberCollector(Type baseType, Type targetType)
        {
            BaseType = baseType;
            TargetType = targetType;

            CollectedMembers = new List<TypeMember>();

            TargetMethods = TargetType.GetMethods(BindingFlag);
            TargetProperties = TargetType.GetProperties(BindingFlag);
            TargetInterfaces = TargetType.GetInterfaces();
        }

        public virtual IEnumerable<TypeMember> Collect()
        {
            CollectMethods();
            CollectProperties();

            return CollectedMembers;
        }

        protected virtual void CollectMethods()
        {
            foreach (var methodInfo in TargetMethods.Where(m => m.IsOverrideable()))
            {
                HandleCollectedTypeMember(methodInfo);
            }
        }

        protected virtual void CollectProperties()
        {
            foreach (var property in TargetProperties.Where(i => i.IsOverrideable()))
            {
                HandleCollectedTypeMember(property);
            }
        }

        protected virtual void HandleCollectedTypeMember(MemberInfo memberInfo)
        {
            var interfaceMembers = null as IEnumerable<MemberInfo>;

            switch (memberInfo)
            {
                case MethodInfo methodInfo:
                    interfaceMembers = methodInfo.GetInterfaceMembers(TargetInterfaces);
                    break;
                case PropertyInfo propertyInfo:
                    interfaceMembers = propertyInfo.GetInterfaceMembers(TargetInterfaces);
                    break;
                case EventInfo eventInfo:
                    interfaceMembers = eventInfo.GetInterfaceMembers(TargetInterfaces);
                    break;
                default:
                    break;
            }

            if (interfaceMembers == null)
            {
                interfaceMembers = new MemberInfo[0];
            }

            //must be declared in interface 
            if (BaseType.IsInterface && !interfaceMembers.Any())
            {
                return;
            }

            var typeMemberInfo = new TypeMember
            {
                Member = memberInfo,
                InterfaceMembers = interfaceMembers,
                Interfaces = interfaceMembers.Select(i => i.DeclaringType)
            };

            CollectedMembers.Add(typeMemberInfo);
        }

        public static IEnumerable<TypeMember> Collect(Type baseType, Type implementionType)
        {
            return new TypeMemberCollector(baseType, implementionType).Collect();
        }
    }
}