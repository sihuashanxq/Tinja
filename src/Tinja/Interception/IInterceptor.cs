using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Tinja.Interception
{
    public interface IInterceptor
    {
        Task InterceptAsync(MethodInvocation invocation, Func<MethodInvocation, Task> next);
    }
}
