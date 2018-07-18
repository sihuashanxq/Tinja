using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Tinja.Abstractions.DynamicProxy.Executors
{
    public interface IMethodInvokerBuilder
    {
        Func<IMethodInvocation, Task> Build(MethodInfo methodInfo);
    }
}
