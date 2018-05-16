using System;
using System.Reflection;
using System.Collections.Concurrent;

namespace Tinja.Interception.Internal
{
    public class ObjectMethodExecutorProvider : IObjectMethodExecutorProvider
    {
        private ConcurrentDictionary<MethodInfo, IObjectMethodExecutor> _executors;

        public ObjectMethodExecutorProvider()
        {
            _executors = new ConcurrentDictionary<MethodInfo, IObjectMethodExecutor>();
        }

        public IObjectMethodExecutor GetExecutor(MethodInfo methodInfo)
        {
            return _executors.GetOrAdd(methodInfo, m => new ObjectMethodExecutor(m));
        }
    }
}
