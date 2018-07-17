using System;
using System.Collections.Generic;

namespace Tinja.Interception
{
    public interface IInterceptorDefinitionCollector
    {
        IEnumerable<InterceptorDefinition> CollectDefinitions(Type serviceType, Type implementionType);
    }
}
