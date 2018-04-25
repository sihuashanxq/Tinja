using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Tinja.Interception
{
    public interface IIntereceptor
    {
        Task IntereceptAsync(MethodInvocation invocation, Func<MethodInvocation, Task> next);
    }
}
