using System;

namespace Tinja.Interception
{
    public class MethodInvocationExecutor : IMethodInvocationExecutor
    {
        protected IMethodInvokerBuilder Builder { get; }

        public MethodInvocationExecutor(IMethodInvokerBuilder builder)
        {
            Builder = builder;
        }

        public object Execute(MethodInvocation invocation)
        {
            throw new NotImplementedException();
        }
    }
}
