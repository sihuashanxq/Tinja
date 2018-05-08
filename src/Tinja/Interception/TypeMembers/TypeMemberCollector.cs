using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception.TypeMembers
{
    public abstract class TypeMemberCollector : ITypeMemberCollector
    {
        protected Type DeclareType { get; }

        protected Type ImplementionType { get; }

        protected List<TypeMemberMetadata> CollectedMethods { get; }

        protected List<TypeMemberMetadata> CollectedProperties { get; }

        protected Type[] ImplementedInterfaces { get; }

        protected IEnumerable<PropertyInfo> ImplementedProperties { get; }

        protected Dictionary<Type, InterceptorDeclare> InterceptorMapping { get; }

        public TypeMemberCollector(Type declareType, Type implementionType)
        {
            DeclareType = declareType;
            ImplementionType = implementionType;

            CollectedMethods = new List<TypeMemberMetadata>();
            CollectedProperties = new List<TypeMemberMetadata>();
            InterceptorMapping = new Dictionary<Type, InterceptorDeclare>();

            ImplementedInterfaces = ImplementionType.GetInterfaces();
            ImplementedProperties = ImplementionType.GetProperties(new[] { typeof(object) });
        }

        public IEnumerable<TypeMemberMetadata> Collect()
        {
            CollectProperties();
            CollectMethods();

            HandleTypeMemberInterceptors();

            return CollectedProperties.Concat(CollectedMethods);
        }

        protected virtual void CollectEvents()
        {

        }

        protected abstract void CollectMethods();

        protected abstract void CollectProperties();

        protected void AddCollectedMethodInfo(TypeMemberMetadata typeMethodInfo)
        {
            CollectedMethods.Add(typeMethodInfo);
        }

        protected void AddCollectedPropertyInfo(TypeMemberMetadata typePropertyInfo)
        {
            CollectedProperties.Add(typePropertyInfo);
        }

        protected IEnumerable<MethodInfo> GetBaseDefinition(MethodInfo memberInfo)
        {
            var baseDefinition = memberInfo.GetBaseDefinition();
            var baseDeclareType = baseDefinition.DeclaringType;
            var list = new List<MethodInfo>();

            foreach (var item in ImplementedInterfaces)
            {
                var mapping = baseDeclareType.GetInterfaceMap(item);

                for (var i = 0; i < mapping.TargetMethods.Length; i++)
                {
                    if (mapping.TargetMethods[i] == baseDefinition)
                    {
                        list.Add(mapping.InterfaceMethods[i]);
                        break;
                    }
                }
            }

            if (!list.Any())
            {
                list.Add(baseDefinition);
            }

            return list;
        }

        protected virtual InterceptorDeclare[] GetInterceptorDeclares(MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                return new InterceptorDeclare[0];
            }

            var attrs = memberInfo.GetInterceptorAttributes();
            var declares = new InterceptorDeclare[attrs.Length];

            for (var i = 0; i < attrs.Length; i++)
            {
                var attr = attrs[i];
                var declare = InterceptorMapping.GetValueOrDefault(attr.InterceptorType);
                if (declare == null)
                {
                    declare = InterceptorMapping[attr.InterceptorType] = new InterceptorDeclare(attr);
                }

                if (memberInfo is Type typeInfo)
                {
                    declare.DeclaredTypes.Add(typeInfo);
                }
                else if (memberInfo is MethodInfo methodInfo)
                {
                    declare.DeclaredMembers.Add(methodInfo);
                }

                declares[i] = declare;
            }

            return declares;
        }

        protected virtual void HandleTypeMemberInterceptors()
        {
            foreach (var item in CollectedProperties)
            {
                HandleTypeMemberInterceptors(item, item.ImplementionMemberInfo, item.ImplementionType);

                foreach (var declareMember in item.DeclareMemberInfos.Where(i => i.DeclaringType.IsInterface))
                {
                    HandleTypeMemberInterceptors(item, declareMember, declareMember.DeclaringType);
                }
            }

            foreach (var item in CollectedMethods)
            {
                HandleTypeMemberInterceptors(item, item.ImplementionMemberInfo, item.ImplementionType);

                foreach (var declareMember in item.DeclareMemberInfos.Where(i => i.DeclaringType.IsInterface))
                {
                    HandleTypeMemberInterceptors(item, declareMember, declareMember.DeclaringType);
                }
            }
        }

        protected void HandleTypeMemberInterceptors(TypeMemberMetadata typeMember, MemberInfo memberInfo, Type typeInfo)
        {
            var typeDeclares = GetInterceptorDeclares(typeInfo);
            var memberDeclares = GetInterceptorDeclares(memberInfo);

            if (typeMember.InterceptorDeclares != null)
            {
                typeMember.InterceptorDeclares = typeMember
                    .InterceptorDeclares
                    .Concat(memberDeclares)
                    .Concat(typeDeclares)
                    .Distinct(declare => declare.Interceptor);
            }
            else
            {
                typeMember.InterceptorDeclares = memberDeclares
                    .Concat(typeDeclares)
                    .Distinct(declare => declare.Interceptor);
            }
        }
  }
}