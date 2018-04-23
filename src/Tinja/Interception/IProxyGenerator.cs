using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tinja.Interception
{
    public interface IProxyGenerator
    {
        Type GenerateProxy(Type implType, Type baseType);
    }

    public class ProxyGeneratorBase : IProxyGenerator
    {
        public virtual Type GenerateProxy(Type implType, Type baseType)
        {
            return null;
        }

        protected virtual TypeProxyMetadata GetTypeProxyMetadata()
        {
            return null;
        }

        protected virtual IEnumerable<MemberMetadata> GetMembers(Type implType, Type baseType)
        {
            return null;
        }
    }

    public class InterfaceProxyGenerator : ProxyGeneratorBase
    {
        protected override IEnumerable<MemberMetadata> GetMembers(Type implType, Type baseType)
        {
            var map = implType.GetInterfaceMap(baseType);
            var members = new List<MemberMetadata>();
            var intereceptors = implType.GetCustomAttributes<InterceptorAttribute>().Select(i => i.InterceptorType);

            foreach (var item in map.TargetMethods)
            {
                var memberInfo = (MemberInfo)GetProperty(implType, item) ?? item;
                if (members.FirstOrDefault(i => i.Member == memberInfo) != null)
                {
                    continue;
                }

                var memberInterceptors = memberInfo
                        .GetCustomAttributes<InterceptorAttribute>()
                        .Select(i => i.InterceptorType)
                        .Concat(intereceptors)
                        .Distinct()
                        .ToArray();

                members.Add(new MemberMetadata(memberInfo, memberInterceptors));
            }

            return members;
        }

        static PropertyInfo GetProperty(Type implType, MethodInfo methodInfo)
        {
            if (methodInfo.Name.StartsWith("get_"))
            {
                return implType.GetProperty(methodInfo.Name.TrimStart("get_".ToCharArray()));
            }

            if (methodInfo.Name.StartsWith("set_"))
            {
                return implType.GetProperty(methodInfo.Name.TrimStart("set_".ToCharArray()));
            }

            return null;
        }
    }

    public class ClassProxyGenerator : ProxyGeneratorBase
    {
        protected override IEnumerable<MemberMetadata> GetMembers(Type implType, Type baseType)
        {
            if (implType == baseType)
            {
                return GetMembers(implType);
            }

            //var members = new List<MemberMetadata>();
            //var intereceptors = implType.GetCustomAttributes<InterceptorAttribute>().Select(i => i.InterceptorType);

            //foreach (var item in map.TargetMethods)
            //{
            //    var memberInfo = (MemberInfo)GetProperty(implType, item) ?? item;
            //    if (members.FirstOrDefault(i => i.Member == memberInfo) != null)
            //    {
            //        continue;
            //    }

            //    var memberInterceptors = memberInfo
            //            .GetCustomAttributes<InterceptorAttribute>()
            //            .Select(i => i.InterceptorType)
            //            .Concat(intereceptors)
            //            .Distinct()
            //            .ToArray();

            //    members.Add(new MemberMetadata(memberInfo, memberInterceptors));
            //}

            //return members;
        }

        private IEnumerable<MemberMetadata> GetMembers(Type implType)
        {
            var members = new List<MemberMetadata>();
            var intereceptors = implType.GetCustomAttributes<InterceptorAttribute>().Select(i => i.InterceptorType);

            foreach (var item in implType.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                if (item.MemberType != MemberTypes.Property &&
                    item.MemberType != MemberTypes.Method)
                {
                    continue;
                }

                if (item is MethodInfo method)
                {
                    if (!method.IsVirtual)
                    {
                        continue;
                    }
                }

                if (item is PropertyInfo propertyInfo)
                {
                    if (propertyInfo.GetMethod != null && !propertyInfo.GetMethod.IsVirtual)
                    {
                        continue;
                    }

                    if (propertyInfo.SetMethod != null && !propertyInfo.SetMethod.IsVirtual)
                    {
                        continue;
                    }
                }

                var memberInterceptors = item
                        .GetCustomAttributes<InterceptorAttribute>()
                        .Select(i => i.InterceptorType)
                        .Concat(intereceptors)
                        .Distinct()
                        .ToArray();

                members.Add(new MemberMetadata(item, memberInterceptors));
            }

            return members;
        }
    }

    public class TypeProxyMetadata
    {
        public Type BaseType { get; }

        public Type ImplemetionType { get; }

        public MemberMetadata[] Members { get; }

        public ConstructorInfo[] BaseConstructorInfos { get; }
    }

    public class MemberMetadata
    {
        public MemberInfo Member { get; }

        public Type[] Intereceptors { get; }

        public MemberMetadata(MemberInfo memberInfo, Type[] intereceptors)
        {
            Member = memberInfo;
            Intereceptors = intereceptors;
        }
    }
}
