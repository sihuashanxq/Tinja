using System;

namespace Tinja.Interception.Generators
{
    public interface IProxyTypeGenerator
    {
        Type CreateProxyType();
    }
}
