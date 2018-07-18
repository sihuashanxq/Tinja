using System;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface ITypeMemberCollectorFactory
    {
        ITypeMemberCollector Create(Type typeInfo);
    }
}
