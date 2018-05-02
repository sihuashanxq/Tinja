using System;

namespace Tinja.Interception
{
    public interface IMethodInvokerBuilder
    {
        Func<object, object[], object> Build(MethodInvocation invocation);
    }
}
