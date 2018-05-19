using System;
using System.Collections.Generic;

namespace Tinja.Interception
{
    public interface IInterceptionTargetProvider
    {
        IEnumerable<InterceptionTarget> GetTargets(Type baseType, Type inheriteType);
    }
}
