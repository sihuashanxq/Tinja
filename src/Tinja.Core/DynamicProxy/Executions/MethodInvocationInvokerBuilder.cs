using System;
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

        internal IServiceResolver ServieResolver { get; set; }

        internal IInterceptorFactory InterceptorFactory { get; set; }

        internal IObjectMethodExecutorProvider MethodExecutorProvider { get; set; }

        internal IInterceptorSelectorProvider InterceptorSelectorProvider { get; set; }

        internal IInterceptorMetadataProvider InterceptorMetadataProvider { get; set; }

        internal Dictionary<Type, InterceptorEntry> Interceptors { get; set; }

        internal Dictionary<MethodInfo, IMethodInvocationInvoker> InvokerCaches { get; set; }

        public MethodInvocationInvokerBuilder(IServiceResolver serviceResolver)
        {
            ServieResolver = serviceResolver ?? throw new NullReferenceException(nameof(serviceResolver));
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
                    _initialized = true;
                    Interceptors = new Dictionary<Type, InterceptorEntry>();
                    InvokerCaches = new Dictionary<MethodInfo, IMethodInvocationInvoker>();
                    InterceptorFactory = ServieResolver.ResolveServiceRequired<IInterceptorFactory>();
                    MethodExecutorProvider = ServieResolver.ResolveServiceRequired<IObjectMethodExecutorProvider>();
                    InterceptorSelectorProvider = ServieResolver.ResolveServiceRequired<IInterceptorSelectorProvider>();
                    InterceptorMetadataProvider = ServieResolver.ResolveServiceRequired<IInterceptorMetadataProvider>();
                }
            }
        }

        public IMethodInvocationInvoker BuildInvoker<TResult>(IMethodInvocation invocation)
        {
            Prepare();

            if (InvokerCaches.TryGetValue(invocation.Method, out var invoker))
            {
                return invoker;
            }

            return InvokerCaches[invocation.Method] = BuildInvoker(invocation, CreateMethodCallStack<TResult>(invocation.Method));
        }

        public IMethodInvocationInvoker BuildTaskAsyncInvoker(IMethodInvocation invocation)
        {
            Prepare();

            if (InvokerCaches.TryGetValue(invocation.Method, out var invoker))
            {
                return invoker;
            }

            return InvokerCaches[invocation.Method] = BuildInvoker(invocation, CreateTaskAsyncMethodCallStack(invocation.Method));
        }

        public IMethodInvocationInvoker BuildTaskAsyncInvoker<TResult>(IMethodInvocation invocation)
        {
            Prepare();

            if (InvokerCaches.TryGetValue(invocation.Method, out var invoker))
            {
                return invoker;
            }

            return InvokerCaches[invocation.Method] = BuildInvoker(invocation, CreateTaskAsyncMethodCallStack<TResult>(invocation.Method));
        }

        public IMethodInvocationInvoker BuildValueTaskAsyncInvoker(IMethodInvocation invocation)
        {
            Prepare();

            if (InvokerCaches.TryGetValue(invocation.Method, out var invoker))
            {
                return invoker;
            }

            return InvokerCaches[invocation.Method] = BuildInvoker(invocation, CreateValueTaskAsyncMethodCallStack(invocation.Method));
        }

        public IMethodInvocationInvoker BuildValueTaskAsyncInvoker<TResult>(IMethodInvocation invocation)
        {
            Prepare();

            if (InvokerCaches.TryGetValue(invocation.Method, out var invoker))
            {
                return invoker;
            }

            return InvokerCaches[invocation.Method] = BuildInvoker(invocation, CreateValueTaskAsyncMethodCallStack<TResult>(invocation.Method));
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
            if (methodInfo.IsAbstract || methodInfo.DeclaringType.IsInterface)
            {
                return new Stack<Func<IMethodInvocation, Task>>().PushContine(_ => Task.CompletedTask);
            }

            var executor = MethodExecutorProvider.GetExecutor(methodInfo);
            if (executor == null)
            {
                throw new NullReferenceException(nameof(executor));
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

            var executor = MethodExecutorProvider.GetExecutor(methodInfo);
            if (executor == null)
            {
                throw new NullReferenceException(nameof(executor));
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

            var executor = MethodExecutorProvider.GetExecutor(methodInfo);
            if (executor == null)
            {
                throw new NullReferenceException(nameof(executor));
            }

            return new Stack<Func<IMethodInvocation, Task>>()
                .PushContine(inv => (Task)(inv.ResultValue = executor.Execute<Task<TResult>>(inv.ProxyInstance, inv.Arguments)));
        }

        private Stack<Func<IMethodInvocation, Task>> CreateValueTaskAsyncMethodCallStack(MethodInfo methodInfo)
        {
            if (methodInfo.IsAbstract || methodInfo.DeclaringType.IsInterface)
            {
                return new Stack<Func<IMethodInvocation, Task>>().PushContine(_ => Task.CompletedTask);
            }

            var executor = MethodExecutorProvider.GetExecutor(methodInfo);
            if (executor == null)
            {
                throw new NullReferenceException(nameof(executor));
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

            var executor = MethodExecutorProvider.GetExecutor(methodInfo);
            if (executor == null)
            {
                throw new NullReferenceException(nameof(executor));
            }

            return new Stack<Func<IMethodInvocation, Task>>()
                .PushContine(inv => (Task<TResult>)(inv.ResultValue = executor.Execute<ValueTask<TResult>>(inv.ProxyInstance, inv.Arguments).AsTask()));
        }

        private IInterceptor[] GetInterceptors(MemberInfo memberInfo)
        {
            lock (Interceptors)
            {
                var entries = new List<InterceptorEntry>();
                var metadatas = InterceptorMetadataProvider.GetInterceptors(memberInfo) ?? new InterceptorMetadata[0];

                foreach (var metadata in metadatas.Where(item => item != null))
                {
                    if (metadata.Handler != null)
                    {
                        entries.Add(new InterceptorEntry(new DelegateInterceptor(metadata.Handler), metadata));
                        continue;
                    }

                    if (!Interceptors.TryGetValue(metadata.InterceptorType, out var entry))
                    {
                        var interceptor = InterceptorFactory.Create(metadata.InterceptorType);
                        if (interceptor == null)
                        {
                            throw new NullReferenceException($"Create interceptor:{metadata.InterceptorType.FullName}");
                        }

                        entry = new InterceptorEntry(interceptor, metadata);
                    }

                    entries.Add(entry);
                }

                return GetInterceptors(memberInfo, entries);
            }
        }

        private IInterceptor[] GetInterceptors(MemberInfo memberInfo, IEnumerable<InterceptorEntry> entries)
        {
            if (memberInfo == null)
            {
                throw new NullReferenceException(nameof(memberInfo));
            }

            if (entries == null)
            {
                throw new NullReferenceException(nameof(entries));
            }

            //sort
            var selectors = InterceptorSelectorProvider.GetSelectors(memberInfo);
            if (selectors == null)
            {
                return entries.OrderByDescending(item => item.Metadata.RankOrder).Select(item => item.Interceptor).ToArray();
            }

            var interceptors = entries.OrderByDescending(item => item.Metadata.RankOrder).Select(item => item.Interceptor);

            return selectors.Aggregate(interceptors, (current, selector) => selector.Select(memberInfo, current)).ToArray();
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
