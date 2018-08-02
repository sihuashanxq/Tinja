using System.Collections.Concurrent;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy.Executions;

namespace Tinja.Core.DynamicProxy.Executions
{
    public class ObjectMethodExecutorProvider : IObjectMethodExecutorProvider
    {
        private readonly ConcurrentDictionary<MethodInfo, IObjectMethodExecutor> _executors;

        public ObjectMethodExecutorProvider()
        {
            _executors = new ConcurrentDictionary<MethodInfo, IObjectMethodExecutor>();
        }

        public IObjectMethodExecutor GetExecutor(MethodInfo methodInfo)
        {
            if (_executors.TryGetValue(methodInfo, out var executor))
            {
                return executor;
            }

            return _executors[methodInfo] = new ObjectMethodExecutor(methodInfo);
        }
    }
}
