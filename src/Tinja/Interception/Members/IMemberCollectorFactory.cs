using System;

namespace Tinja.Interception.Members
{
    public interface IMemberCollectorFactory
    {
        IMemberCollector Create(Type serviceType, Type implementionType);
    }
}
