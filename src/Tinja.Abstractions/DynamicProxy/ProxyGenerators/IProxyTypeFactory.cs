using System;

namespace Tinja.Abstractions.DynamicProxy.ProxyGenerators
{
    public interface IProxyTypeFactory
    {
        Type CreateProxyType(Type typeInfo);
    }
}
