using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception.TypeMembers
{
    public abstract class TypeMemberCollector : ITypeMemberCollector
    {
        protected Type BaseType { get; }

        protected Type ImplementionType { get; }

        protected List<TypeMemberMetadata> CollectedMethods { get; }

        protected List<TypeMemberMetadata> CollectedProperties { get; }

        protected Type[] ImplementedInterfaces { get; }

        protected Dictionary<Type, InterceptorMetadata> BindingMap { get; }

        public TypeMemberCollector(Type baseType, Type implementionType)
        {
            BaseType = baseType;
            ImplementionType = implementionType;

            CollectedMethods = new List<TypeMemberMetadata>();
            CollectedProperties = new List<TypeMemberMetadata>();
            BindingMap = new Dictionary<Type, InterceptorMetadata>();

            ImplementedInterfaces = ImplementionType.GetInterfaces();
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

        protected virtual InterceptorMetadata[] GetInterceptorBindings(MemberInfo impl, MemberInfo definition)
        {
            if (definition == null || impl == null)
            {
                return new InterceptorMetadata[0];
            }

            var attrs = definition.GetInterceptorAttributes();
            var bindings = new InterceptorMetadata[attrs.Length];

            for (var i = 0; i < attrs.Length; i++)
            {
                var attr = attrs[i];
                var binding = BindingMap.GetValueOrDefault(attr.InterceptorType);
                if (binding == null)
                {
                    binding = BindingMap[attr.InterceptorType] = new InterceptorMetadata(attr);
                }

                binding.AddTarget(impl);
                bindings[i] = binding;
            }

            return bindings;
        }

        protected virtual void HandleTypeMemberInterceptors()
        {
            foreach (var item in CollectedProperties)
            {
                HandleTypeMemberInterceptors(item, item.ImplementionMember, item.ImplementionType);

                foreach (var declareMember in item.BaseMembers.Where(i => i.DeclaringType.IsInterface))
                {
                    HandleTypeMemberInterceptors(item, declareMember, declareMember.DeclaringType);
                }
            }

            foreach (var item in CollectedMethods)
            {
                HandleTypeMemberInterceptors(item, item.ImplementionMember, item.ImplementionType);

                foreach (var declareMember in item.BaseMembers.Where(i => i.DeclaringType.IsInterface))
                {
                    HandleTypeMemberInterceptors(item, declareMember, declareMember.DeclaringType);
                }
            }
        }

        protected void HandleTypeMemberInterceptors(TypeMemberMetadata typeMember, MemberInfo memberInfo, Type typeInfo)
        {
            var typeBindings = GetInterceptorBindings(typeMember.ImplementionType, typeInfo);
            var memberBindings = GetInterceptorBindings(typeMember.ImplementionMember, memberInfo);

            if (typeMember.InterceptorBindings != null)
            {
                typeMember.InterceptorBindings = typeMember
                    .InterceptorBindings
                    .Concat(memberBindings)
                    .Concat(typeBindings)
                    .Distinct(declare => declare.Attribute);
            }
            else
            {
                typeMember.InterceptorBindings = memberBindings
                    .Concat(typeBindings)
                    .Distinct(declare => declare.Attribute);
            }
        }
    }
}