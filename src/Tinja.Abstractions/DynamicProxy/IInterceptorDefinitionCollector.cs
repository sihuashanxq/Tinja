using System;
using System.Collections.Generic;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IInterceptorDefinitionCollector
    {
        IEnumerable<InterceptorDefinition> CollectDefinitions(Type serviceType, Type implementionType);
    }
}
