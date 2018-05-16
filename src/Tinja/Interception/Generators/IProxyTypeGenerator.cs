using System;

namespace Tinja.Interception
{
    public interface IProxyTypeGenerator
    {
        Type CreateProxyType();
    }
}
