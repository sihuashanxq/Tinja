using System;

namespace Tinja.Interception.Members
{
    public class MemberCollectorFactory : IMemberCollectorFactory
    {
        public static readonly IMemberCollectorFactory Default = new MemberCollectorFactory();

        public IMemberCollector Create(Type serviceType, Type implementionType)
        {
            if (serviceType == null)
            {
                throw new NullReferenceException(nameof(serviceType));
            }

            if (implementionType == null)
            {
                throw new NullReferenceException(nameof(implementionType));
            }

            if (implementionType.IsInterface)
            {
                return new InterfaceMemberCollector(implementionType);
            }

            if (serviceType.IsInterface)
            {
                return new InterfaceWithTargetMemberCollector(implementionType);
            }

            return new ClassMemberCollector(implementionType);
        }
    }
}
