using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tinja.Interception.TypeMembers
{
    public abstract class TypeMemberCollector : ITypeMemberCollector
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

        protected abstract void CollectMethods();

        protected abstract void CollectProperties();

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
            if (baseType.IsInterface)
            {
                return new InterfaceTypeMemberCollector(baseType, implementionType).Collect();
            }

            return new ClassTypeMemberCollector(baseType, implementionType).Collect();
        }
    }
}