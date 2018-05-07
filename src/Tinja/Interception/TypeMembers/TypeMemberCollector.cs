using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception.TypeMembers
{
    public abstract class TypeMemberCollector : ITypeMemberCollector
    {
        protected List<TypeMemberMetadata> Methods { get; }

        protected List<TypeMemberMetadata> Properties { get; }

        protected Dictionary<Type, InterceptorDeclare> InterceptorDeclarations { get; }

        protected Type DeclareType { get; }

        protected Type ImplementionType { get; }

        public TypeMemberCollector(Type declareType, Type implementionType)
        {
            DeclareType = declareType;
            ImplementionType = implementionType;

            Methods = new List<TypeMemberMetadata>();
            Properties = new List<TypeMemberMetadata>();
            InterceptorDeclarations = new Dictionary<Type, InterceptorDeclare>();
        }

        public IEnumerable<TypeMemberMetadata> Collect()
        {
            CollectProperties();
            CollectMethods();

            var typeMmembers = Properties.Concat(Methods);

            foreach (var item in typeMmembers)
            {
                SetInterceptorDeclares(item, item.ImplementionMemberInfo, item.ImplementionType);
                SetInterceptorDeclares(item, item.DeclareMemberInfo, item.DeclareType);
            }

            return typeMmembers;
        }

        protected virtual void CollectMethods()
        {

        }

        protected virtual void CollectProperties()
        {

        }

        protected void SetInterceptorDeclares(TypeMemberMetadata typeMember, MemberInfo memberInfo, Type typeInfo)
        {
            var interceptors = GetInterceptorTypes(memberInfo);
            var typeInterceptors = GetInterceptorTypes(typeInfo);

            var declares = GetInterceptorDeclares(interceptors, memberInfo);
            var typeDeclares = GetInterceptorDeclares(typeInterceptors, typeInfo);

            if (typeMember.InterceptorDeclares != null)
            {
                typeMember.InterceptorDeclares = typeMember.InterceptorDeclares.Concat(declares).Concat(typeDeclares).Distinct();
            }
            else
            {
                typeMember.InterceptorDeclares = declares.Concat(typeDeclares).Distinct();
            }
        }

        protected InterceptorDeclare[] GetInterceptorDeclares(Type[] interceptors, MemberInfo memberInfo)
        {
            if (interceptors == null || interceptors.Length == 0)
            {
                return new InterceptorDeclare[0];
            }

            var declares = new InterceptorDeclare[interceptors.Length];

            for (var i = 0; i < interceptors.Length; i++)
            {
                var interceptor = interceptors[i];
                if (!InterceptorDeclarations.ContainsKey(interceptor))
                {
                    InterceptorDeclarations[interceptor] = new InterceptorDeclare(interceptor);
                }

                var declare = InterceptorDeclarations[interceptor];

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

        internal static Type[] GetInterceptorTypes(MemberInfo memberInfo)
        {
            var attrs = memberInfo.GetCustomAttributes<InterceptorAttribute>();
            if (attrs == null || !attrs.Any())
            {
                return Type.EmptyTypes;
            }

            return attrs.Select(i => i.InterceptorType).ToArray();
        }

        internal static IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            if (type == null || type == typeof(object))
            {
                return new PropertyInfo[0];
            }

            var typeInfo = type.GetTypeInfo();
            if (typeInfo.BaseType != null)
            {
                return typeInfo.DeclaredProperties.Concat(GetProperties(typeInfo.BaseType));
            }

            return typeInfo.DeclaredProperties;
        }
    }
}