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
    public class MethodInvocationInvokerBuilder : IMethodInvocationInvokerBuilder
    {
        private bool _initialized;

        internal IServiceResolver ServieResolver { get; set; }

        internal IInterceptorFactory InterceptorFactory { get; set; }

        internal IObjectMethodExecutorProvider MethodExecutorProvider { get; set; }

        internal IInterceptorSelectorProvider InterceptorSelectorProvider { get; set; }

        internal IInterceptorMetadataProvider InterceptorMetadataProvider { get; set; }

        internal Dictionary<Type, InterceptorEntry> Interceptors { get; set; }

        internal Dictionary<MethodInfo, IMethodInvocationInvoker> Invokers { get; set; }

        public MethodInvocationInvokerBuilder(IServiceResolver serviceResolver)
        {
            ServieResolver = serviceResolver ?? throw new NullReferenceException(nameof(serviceResolver));
        }

        public IMethodInvocationInvoker Build(IMethodInvocation invocation)
        {
            if (invocation == null)
            {
                throw new NullReferenceException(nameof(invocation));
            }

            if (!_initialized)
            {
                lock (this)
                {
                    _initialized = true;
                    Interceptors = new Dictionary<Type, InterceptorEntry>();
                    Invokers = new Dictionary<MethodInfo, IMethodInvocationInvoker>();
                    InterceptorFactory = ServieResolver.ResolveServiceRequired<IInterceptorFactory>();
                    MethodExecutorProvider = ServieResolver.ResolveServiceRequired<IObjectMethodExecutorProvider>();
                    InterceptorSelectorProvider = ServieResolver.ResolveServiceRequired<IInterceptorSelectorProvider>();
                    InterceptorMetadataProvider = ServieResolver.ResolveServiceRequired<IInterceptorMetadataProvider>();
                }
            }

            if (Invokers.TryGetValue(invocation.Method, out var invoker))
            {
                return invoker;
            }

            return Invokers[invocation.Method] = BuildMethodInvocationInvoker(invocation);
        }

        protected virtual IMethodInvocationInvoker BuildMethodInvocationInvoker(IMethodInvocation invocation)
        {
            var executor = MethodExecutorProvider.GetExecutor(invocation.Method);
            if (executor == null)
            {
                throw new NullReferenceException(nameof(executor));
            }

            var callStack = CreateMethodExecuteStack(executor);
            if (!callStack.Any())
            {
                throw new InvalidOperationException("build invoker failed!");
            }

            var interceptors = GetInterceptors(invocation.TargetMember);
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

        protected virtual IInterceptor[] GetInterceptors(MemberInfo memberInfo)
        {
            lock (Interceptors)
            {
                var entries = new List<InterceptorEntry>();
                var metadatas = InterceptorMetadataProvider.GetMetadatas(memberInfo) ?? new InterceptorMetadata[0];

                foreach (var metadata in metadatas.Where(item => item != null))
                {
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
                return entries.OrderByDescending(item => item.Metadata.Order).Select(item => item.Interceptor).ToArray();
            }

            var interceptors = entries.OrderByDescending(item => item.Metadata.Order).Select(item => item.Interceptor);

            return selectors.Aggregate(interceptors, (current, selector) => selector.Select(memberInfo, current)).ToArray();
        }

        private static Stack<Func<IMethodInvocation, Task>> CreateMethodExecuteStack(IObjectMethodExecutor executor)
        {
            var stack = new Stack<Func<IMethodInvocation, Task>>();

            stack.Push(inv =>
            {
                if (inv.Method.IsAbstract || inv.Method.DeclaringType.IsInterface)
                {
                    return Task.CompletedTask;
                }

                return (Task)(inv.ResultValue = executor.ExecuteAsync(inv.ProxyInstance, inv.Parameters));
            });

            return stack;
        }
    }
}
