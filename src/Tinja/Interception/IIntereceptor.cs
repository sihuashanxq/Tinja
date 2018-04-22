using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Tinja.Interception
{
    public interface IIntereceptor
    {
        Task IntereceptAsync(MethodInvocationContext context, Func<MethodInvocationContext, Task> next);
    }

    public class MethodInvocationContext
    {
        object Instance { get; }

        object ReturnValue { get; }

        MethodInfo MethodInfo { get; }

        object[] ParamterValues { get; }
    }
}
