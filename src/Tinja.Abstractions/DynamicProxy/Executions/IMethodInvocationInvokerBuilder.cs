using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Tinja.Abstractions.DynamicProxy.Executions
{
    public interface IMethodInvocationInvokerBuilder
    {
        IMethodInvocationInvoker Build(IMethodInvocation invocation);
    }
}
