using System;

namespace Tinja.Interception
{
    public interface IMethodInvokerBuilder
    {
        Func<IDynamicProxy, object> Build(MethodInvocation invocation);
    }
}
