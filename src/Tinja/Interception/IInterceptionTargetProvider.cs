using System;
using System.Collections.Generic;

namespace Tinja.Interception
{
    public interface IInterceptionTargetProvider
    {
        IEnumerable<InterceptionTarget> GetTargets(Type serviceType, Type implementionType);
    }
}
