using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Executions;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.DynamicProxy.Executions
{
    [DisableProxy]
    public class MethodInvocationInvokerBuilder
    {
        private bool _initialized;

        private IInterceptorFactory _interceptorFactory;

        private readonly IServiceResolver _servieResolver;

        private Dictionary<Type, InterceptorEntry> _interceptors;

        private IObjectMethodExecutorProvider _methodExecutorProvider;

        private IInterceptorSelectorProvider _interceptorSelectorProvider;

        private IInterceptorMetadataProvider _interceptorMetadataProvider;

        private ConcurrentDictionary<MethodInfo, IMethodInvocationInvoker> _invokerCaches;

        public MethodInvocationInvokerBuilder(IServiceResolver serviceResolver)
        {
            _servieResolver = serviceResolver ?? throw new ArgumentNullException(nameof(serviceResolver));
        }

        private void Prepare()
        {
            if (_initialized)
            {
                return;
            }

            lock (this)
            {
                if (!_initialized)
                {
                    _interceptors = new Dictionary<Type, InterceptorEntry>();
                    _invokerCaches = new ConcurrentDictionary<MethodInfo, IMethodInvocationInvoker>();
                    _interceptorFactory = _servieResolver.ResolveServiceRequired<IInterceptorFactory>();
                    _methodExecutorProvider = _servieResolver.ResolveServiceRequired<IObjectMethodExecutorProvider>();
                    _interceptorSelectorProvider = _servieResolver.ResolveServiceRequired<IInterceptorSelectorProvider>();
                    _interceptorMetadataProvider = _servieResolver.ResolveServiceRequired<IInterceptorMetadataProvider>();
                    _initialized = true;
                }
            }
        }

        /// <summary>
        /// T/void Method();
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="invocation"></param>
        /// <returns></returns>
        public IMethodInvocationInvoker BuildInvoker<TResult>(IMethodInvocation invocation)
        {
            Prepare();

            // ReSharper disable once InconsistentlySynchronizedField
            if (_invokerCaches.TryGetValue(invocation.Method, out var invoker))
            {
                return invoker;
            }

            lock (_invokerCaches)
            {
                if (_invokerCaches.TryGetValue(invocation.Method, out invoker))
                {
                    return invoker;
                }

                return _invokerCaches[invocation.Method] = BuildInvoker(invocation, CreateMethodCallStack<TResult>(invocation.Method));
            }
        }

        /// <summary>
        /// Task Method();
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        public IMethodInvocationInvoker BuildTaskAsyncInvoker(IMethodInvocation invocation)
        {
            Prepare();

            // ReSharper disable once InconsistentlySynchronizedField
            if (_invokerCaches.TryGetValue(invocation.Method, out var invoker))
            {
                return invoker;
            }

            lock (_invokerCaches)
            {
                if (_invokerCaches.TryGetValue(invocation.Method, out invoker))
                {
                    return invoker;
                }

                return _invokerCaches[invocation.Method] = BuildInvoker(invocation, CreateTaskAsyncMethodCallStack(invocation.Method));
            }
        }

        /// <summary>
        /// Task` Method();
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="invocation"></param>
        /// <returns></returns>
        public IMethodInvocationInvoker BuildTaskAsyncInvoker<TResult>(IMethodInvocation invocation)
        {
            Prepare();

            // ReSharper disable once InconsistentlySynchronizedField
            if (_invokerCaches.TryGetValue(invocation.Method, out var invoker))
            {
                return invoker;
            }

            lock (_invokerCaches)
            {
                if (_invokerCaches.TryGetValue(invocation.Method, out invoker))
                {
                    return invoker;
                }

                return _invokerCaches[invocation.Method] = BuildInvoker(invocation, CreateTaskAsyncMethodCallStack<TResult>(invocation.Method));
            }
        }

        /// <summary>
        /// ValueTask Method();
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        public IMethodInvocationInvoker BuildValueTaskAsyncInvoker(IMethodInvocation invocation)
        {
            Prepare();

            // ReSharper disable once InconsistentlySynchronizedField
            if (_invokerCaches.TryGetValue(invocation.Method, out var invoker))
            {
                return invoker;
            }

            lock (_invokerCaches)
            {
                if (_invokerCaches.TryGetValue(invocation.Method, out invoker))
                {
                    return invoker;
                }

                return _invokerCaches[invocation.Method] = BuildInvoker(invocation, CreateValueTaskAsyncMethodCallStack(invocation.Method));
            }
        }

        /// <summary>
        /// ValueTask` Method();
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="invocation"></param>
        /// <returns></returns>
        public IMethodInvocationInvoker BuildValueTaskAsyncInvoker<TResult>(IMethodInvocation invocation)
        {
            Prepare();

            // ReSharper disable once InconsistentlySynchronizedField
            if (_invokerCaches.TryGetValue(invocation.Method, out var invoker))
            {
                return invoker;
            }

            lock (_invokerCaches)
            {
                if (_invokerCaches.TryGetValue(invocation.Method, out invoker))
                {
                    return invoker;
                }

                return _invokerCaches[invocation.Method] = BuildInvoker(invocation, CreateValueTaskAsyncMethodCallStack<TResult>(invocation.Method));
            }
        }

        private IMethodInvocationInvoker BuildInvoker(IMethodInvocation invocation, Stack<Func<IMethodInvocation, Task>> callStack)
        {
            if (!callStack.Any())
            {
                throw new InvalidOperationException("build invoker failed!");
            }

            var interceptors = GetInterceptors(invocation.Target);
            if (interceptors == null || interceptors.Length == 0)
            {
                return new MethodInvocationInvoker(callStack.Pop());
            }

            for (var i = interceptors.Length - 1; i >= 0; i--)
            {
                var next = callStack.Pop();
                var item = interceptors[i];

                callStack.Push(inv => item.InvokeAsync(inv, next));
            }

            return new MethodInvocationInvoker(callStack.Pop());
        }

        private Stack<Func<IMethodInvocation, Task>> CreateMethodCallStack<TResult>(MethodInfo methodInfo)
        {
            if (methodInfo.IsAbstract)
            {
                return new Stack<Func<IMethodInvocation, Task>>().PushContine(_ => Task.CompletedTask);
            }

            var executor = _methodExecutorProvider.GetExecutor(methodInfo);
            if (executor == null)
            {
                throw new ArgumentNullException(nameof(executor));
            }

            if (methodInfo.IsVoidMethod())
            {
                return new Stack<Func<IMethodInvocation, Task>>()
                    .PushContine(inv =>
                    {
                        executor.Execute<TResult>(inv.ProxyInstance, inv.Arguments);
                        return Task.CompletedTask;
                    });
            }

            return new Stack<Func<IMethodInvocation, Task>>()
                .PushContine(inv =>
                {
                    inv.ResultValue = executor.Execute<TResult>(inv.ProxyInstance, inv.Arguments);
                    return Task.CompletedTask;
                });
        }

        private Stack<Func<IMethodInvocation, Task>> CreateTaskAsyncMethodCallStack(MethodInfo methodInfo)
        {
            if (methodInfo.IsAbstract || methodInfo.DeclaringType.IsInterface)
            {
                return new Stack<Func<IMethodInvocation, Task>>().PushContine(_ => Task.CompletedTask);
            }

            var executor = _methodExecutorProvider.GetExecutor(methodInfo);
            if (executor == null)
            {
                throw new ArgumentNullException(nameof(executor));
            }

            return new Stack<Func<IMethodInvocation, Task>>()
                .PushContine(inv => executor.Execute<Task>(inv.ProxyInstance, inv.Arguments));
        }

        private Stack<Func<IMethodInvocation, Task>> CreateTaskAsyncMethodCallStack<TResult>(MethodInfo methodInfo)
        {
            if (methodInfo.IsAbstract || methodInfo.DeclaringType.IsInterface)
            {
                return new Stack<Func<IMethodInvocation, Task>>().PushContine(_ => Task.CompletedTask);
            }

            var executor = _methodExecutorProvider.GetExecutor(methodInfo);
            if (executor == null)
            {
                throw new ArgumentNullException(nameof(executor));
            }

            return new Stack<Func<IMethodInvocation, Task>>()
                .PushContine(inv =>
                    (Task)(inv.ResultValue = executor.Execute<Task<TResult>>(inv.ProxyInstance, inv.Arguments)));
        }

        private Stack<Func<IMethodInvocation, Task>> CreateValueTaskAsyncMethodCallStack(MethodInfo methodInfo)
        {
            if (methodInfo.IsAbstract || methodInfo.DeclaringType.IsInterface)
            {
                return new Stack<Func<IMethodInvocation, Task>>().PushContine(_ => Task.CompletedTask);
            }

            var executor = _methodExecutorProvider.GetExecutor(methodInfo);
            if (executor == null)
            {
                throw new ArgumentNullException(nameof(executor));
            }

            return new Stack<Func<IMethodInvocation, Task>>()
                .PushContine(inv => executor.Execute<ValueTask>(inv.ProxyInstance, inv.Arguments).AsTask());
        }

        private Stack<Func<IMethodInvocation, Task>> CreateValueTaskAsyncMethodCallStack<TResult>(MethodInfo methodInfo)
        {
            if (methodInfo.IsAbstract || methodInfo.DeclaringType.IsInterface)
            {
                return new Stack<Func<IMethodInvocation, Task>>().PushContine(_ => Task.CompletedTask);
            }

            var executor = _methodExecutorProvider.GetExecutor(methodInfo);
            if (executor == null)
            {
                throw new ArgumentNullException(nameof(executor));
            }

            return new Stack<Func<IMethodInvocation, Task>>()
                .PushContine(inv =>
                    (Task<TResult>)(inv.ResultValue =
                        executor.Execute<ValueTask<TResult>>(inv.ProxyInstance, inv.Arguments).AsTask()));
        }

        private IInterceptor[] GetInterceptors(MemberInfo memberInfo)
        {
            var entries = new List<InterceptorEntry>();
            var metadatas = _interceptorMetadataProvider.GetInterceptors(memberInfo) ?? new InterceptorMetadata[0];

            foreach (var metadata in metadatas.Where(item => item != null))
            {
                //may multi instance
                if (metadata.Handler != null)
                {
                    entries.Add(new InterceptorEntry(new DelegateInterceptor(metadata.Handler), metadata));
                    continue;
                }

                //mark sure only one instance
                if (_interceptors.TryGetValue(metadata.InterceptorType, out var entry))
                {
                    entries.Add(entry);
                    continue;
                }

                var interceptor = _interceptorFactory.Create(metadata.InterceptorType);
                if (interceptor == null)
                {
                    throw new ArgumentNullException($"Create interceptor:{metadata.InterceptorType.FullName}");
                }

                entry = new InterceptorEntry(interceptor, metadata);
                entries.Add(entry);

                _interceptors[metadata.InterceptorType] = entry;
            }

            return GetInterceptors(memberInfo, entries);
        }

        private IInterceptor[] GetInterceptors(MemberInfo memberInfo, IEnumerable<InterceptorEntry> entries)
        {
            if (memberInfo == null)
            {
                throw new ArgumentNullException(nameof(memberInfo));
            }

            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            //sort
            var selectors = _interceptorSelectorProvider.GetSelectors(memberInfo);
            if (selectors == null)
            {
                return entries.OrderByDescending(item => item.Metadata.Order).Select(item => item.Interceptor)
                    .ToArray();
            }

            var interceptors = entries
                .OrderByDescending(item => item.Metadata.Order)
                .Select(item => item.Interceptor);

            return selectors
                .Aggregate(interceptors, (current, selector) => selector.Select(memberInfo, current))
                .ToArray();
        }
    }

    internal static class StackExtensions
    {
        internal static Stack<T> PushContine<T>(this Stack<T> stack, T item)
        {
            stack.Push(item);

            return stack;
        }
    }
}
