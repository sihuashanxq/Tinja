using System;
using Tinja.Abstractions.DynamicProxy;

namespace Tinja.Core.DynamicProxy.Members
{
    public class TypeMemberCollectorFactory : ITypeMemberCollectorFactory
    {
        public static readonly ITypeMemberCollectorFactory Default = new TypeMemberCollectorFactory();

        private static readonly ITypeMemberCollector ClassTypeMemberCollector = new ClassTypeMemberCollector();

        private static readonly ITypeMemberCollector InterfaceTypeMemberCollector = new InterfaceTypeMemberCollector();

        public ITypeMemberCollector Create(Type typeInfo)
        {
            if (typeInfo == null)
            {
                throw new NullReferenceException(nameof(typeInfo));
            }

            return typeInfo.IsClass ? ClassTypeMemberCollector : InterfaceTypeMemberCollector;
        }
    }
}
