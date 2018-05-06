using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception
{
    public interface IInterceptorProvider
    {
        IEnumerable<IInterceptor> Get(object target, MethodInfo targetMethod, MethodInfo serviceMethod);
    }
}
