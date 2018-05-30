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

        protected List<TypeMember> Members { get; }

        protected Type[] TargetInterfaces { get; }

        protected MethodInfo[] TargetMethods { get; }

        protected PropertyInfo[] TargetProperties { get; }

        public TypeMemberCollector(Type baseType, Type targetType)
        {
            BaseType = baseType;
            TargetType = targetType;

            Members = new List<TypeMember>();

            TargetMethods = TargetType.GetMethods(BindingFlag);
            TargetProperties = TargetType.GetProperties(BindingFlag);
            TargetInterfaces = TargetType.GetInterfaces();
        }

        public virtual IEnumerable<TypeMember> Collect()
        {
            CollectTypeEvents();
            CollectTypeMethods();
            CollectTypeProperties();

            return Members;
        }

        protected virtual void CollectTypeMethods()
        {
            foreach (var methodInfo in TargetMethods.Where(m => m.IsOverrideable()))
            {
                HandleCollectedMemberInfo(methodInfo);
            }
        }

        /// <summary>
        /// </summary>
        protected virtual void CollectTypeProperties()
        {
            foreach (var property in TargetProperties.Where(m => m.IsOverrideable()))
            {
                HandleCollectedMemberInfo(property);
            }
        }

        protected virtual void CollectTypeEvents()
        {

        }

        protected virtual void HandleCollectedMemberInfo(MemberInfo memberInfo)
        {
            var mapMembers = null as IEnumerable<MemberInfo>;

            switch (memberInfo)
            {
                case MethodInfo methodInfo:
                    mapMembers = methodInfo.GetInterfaceMapMembers(TargetInterfaces);
                    break;
                case PropertyInfo propertyInfo:
                    mapMembers = propertyInfo.GetInterfaceMapMembers(TargetInterfaces);
                    break;
                case EventInfo eventInfo:
                    mapMembers = eventInfo.GetInterfaceMapMembers(TargetInterfaces);
                    break;
                default:
                    break;
            }

            if (mapMembers == null)
            {
                mapMembers = new MemberInfo[0];
            }

            var typeMemberInfo = new TypeMember
            {
                Member = memberInfo,
                InterfaceMembers = mapMembers,
                Interfaces = mapMembers.Select(i => i.DeclaringType)
            };

            Members.Add(typeMemberInfo);
        }

        /// <summary>
        /// 收集给定基类型与实现类型代理类的实现成员
        /// </summary>
        /// <param name="baseType">基类型</param>
        /// <param name="implementionType">实现类型</param>
        /// <param name="onlyInterface">仅仅收集接口成员(实现类成员不要求virtual),否则实现类成员需要virtutal</param>
        /// <returns></returns>
        public static IEnumerable<TypeMember> Collect(Type baseType, Type implementionType, bool onlyInterface = false)
        {
            if (baseType == null)
            {
                throw new NullReferenceException(nameof(baseType));
            }

            if (implementionType == null)
            {
                throw new NullReferenceException(nameof(implementionType));
            }

            if (!baseType.IsInterface || !onlyInterface)
            {
                return new TypeMemberCollector(baseType, implementionType).Collect();
            }

            return new InterfaceTargetTypeMemberCollector(baseType, implementionType).Collect();
        }
    }
}