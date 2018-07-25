using System;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IProxyTypeFactory
    {
        Type CreateProxyType(Type typeInfo);
    }
}
