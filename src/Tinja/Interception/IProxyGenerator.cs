using System;

namespace Tinja.Interception
{
    public interface IProxyGenerator
    {
        Type CreateProxyType();
    }
}
