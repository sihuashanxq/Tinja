using System.Collections.Concurrent;
using System.Reflection;

namespace Tinja.Interception.Executors.Internal
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
