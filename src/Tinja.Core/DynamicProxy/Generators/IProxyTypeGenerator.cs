using System;

namespace Tinja.Core.DynamicProxy.Generators
{
    public interface IProxyTypeGenerator
    {
        Type CreateProxyType();
    }
}
