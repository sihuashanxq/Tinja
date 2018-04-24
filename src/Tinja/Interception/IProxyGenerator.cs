using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tinja.Interception
{
    public interface IProxyGenerator
    {
        Type GenerateProxy(Type implType, Type baseType);
    }
}
