using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Registrations;
using Tinja.Abstractions.Injection;
using Tinja.Core;
using Tinja.Core.Extensions;
using Tinja.Abstractions.Extensions;
using Tinja.Core.DynamicProxy;

namespace TinjaBenchmark.DynamicProxy
{
    [MemoryDiagnoser]
    [AllStatisticsColumn]
    public class DynamicProxyBenchmark
    {
        private readonly IServiceResolver _serviceResolver;

        private readonly NoneInterceptorService _noneInterceptorService;

        private readonly OneInterceptorService _oneInterceptorService;

        private readonly TwoInterceptorService _twoInterceptorService;

        private readonly ThreeInterceptorService _threeInterceptorService;

        public DynamicProxyBenchmark()
        {
            _serviceResolver = new Container()
                .AddTransient<NoneInterceptorService>()
                .AddTransient<OneInterceptorService>()
                .AddTransient<TwoInterceptorService>()
                .AddTransient<ThreeInterceptorService>()
                .AddTransient<InterceptorA>()
                .AddTransient<InterceptorB>()
                .AddTransient<InterceptorC>()
                .UseDynamicProxy()
                .BuildServiceResolver();

            _noneInterceptorService = _serviceResolver.ResolveService<NoneInterceptorService>();
            _oneInterceptorService = _serviceResolver.ResolveService<OneInterceptorService>();
            _twoInterceptorService = _serviceResolver.ResolveService<TwoInterceptorService>();
            _threeInterceptorService = _serviceResolver.ResolveService<ThreeInterceptorService>();
        }

        [Benchmark(Description = "未拦截")]
        public void None()
        {

        }

        [Benchmark]
        public void ExecWithNoneIntercptor()
        {
            _noneInterceptorService.Method();
        }

        [Benchmark]
        public Task ExecWithNoneIntercptorAsync_Task()
        {
            return _noneInterceptorService.MethodAsync();
        }

        [Benchmark]
        public Task<int> ExecWithNoneIntercptorAsync_Task_Int32()
        {
            return _noneInterceptorService.MethodAsyncT();
        }

        [Benchmark]
        public ValueTask ExecWithNoneIntercptorAsync_ValueTask()
        {
            return _noneInterceptorService.MethodValueTaskAsync();
        }

        [Benchmark]
        public ValueTask<int> ExecWithNoneIntercptorAsync_ValueTask_Int32()
        {
            return _noneInterceptorService.MethodValueTaskAsyncT();
        }

        [Benchmark(Description = "1个拦截器")]
        public void One()
        {

        }

        [Benchmark]
        public void ExecWithOneIntercptor()
        {
            _oneInterceptorService.Method();
        }

        [Benchmark]
        public Task ExecWithOneIntercptorAsync_Task()
        {
            return _oneInterceptorService.MethodAsync();
        }

        [Benchmark]
        public Task<int> ExecWithOneIntercptorAsync_Task_Int32()
        {
            return _oneInterceptorService.MethodAsyncT();
        }

        [Benchmark]
        public ValueTask ExecWithOneIntercptorAsync_ValueTask()
        {
            return _oneInterceptorService.MethodValueTaskAsync();
        }

        [Benchmark]
        public ValueTask<int> ExecWithOneIntercptorAsync_ValueTask_Int32()
        {
            return _oneInterceptorService.MethodValueTaskAsyncT();
        }

        [Benchmark(Description = "2个拦截器")]
        public void Two()
        {

        }

        [Benchmark]
        public void ExecWithTwoIntercptors()
        {
            _twoInterceptorService.Method();
        }

        [Benchmark]
        public Task ExecWithTwoIntercptorsAsync_Task()
        {
            return _twoInterceptorService.MethodAsync();
        }

        [Benchmark]
        public Task<int> ExecWithTwoIntercptorsAsync_Task_Int32()
        {
            return _twoInterceptorService.MethodAsyncT();
        }

        [Benchmark]
        public ValueTask ExecWithTwoIntercptorsAsync_ValueTask()
        {
            return _twoInterceptorService.MethodValueTaskAsync();
        }


        [Benchmark]
        public ValueTask<int> ExecWithTwoIntercptorsAsync_ValueTask_Int32()
        {
            return _twoInterceptorService.MethodValueTaskAsyncT();
        }

        [Benchmark(Description = "3个拦截器")]
        public void Three()
        {

        }

        [Benchmark]
        public void ExecWithThreeIntercptors()
        {
            _threeInterceptorService.Method();
        }

        [Benchmark]
        public Task ExecWithThreeIntercptorsAsync_Task()
        {
            return _threeInterceptorService.MethodAsync();
        }

        [Benchmark]
        public Task<int> ExecWithThreeIntercptorsAsync_Task_Int32()
        {
            return _threeInterceptorService.MethodAsyncT();
        }

        [Benchmark]
        public ValueTask ExecWithThreeIntercptorsAsync_ValueTask()
        {
            return _threeInterceptorService.MethodValueTaskAsync();
        }

        [Benchmark]
        public ValueTask<int> ExecWithThreeIntercptorsAsync_ValueTask_Int32()
        {
            return _threeInterceptorService.MethodValueTaskAsyncT();
        }

        public class NoneInterceptorService
        {
            public virtual void Method()
            {

            }

            public virtual Task MethodAsync()
            {
                return Task.CompletedTask;
            }

            public virtual Task<int> MethodAsyncT()
            {
                return Task.FromResult(1);
            }

            public virtual ValueTask MethodValueTaskAsync()
            {
                return new ValueTask(Task.CompletedTask);
            }

            public virtual ValueTask<int> MethodValueTaskAsyncT()
            {
                return new ValueTask<int>(1);
            }
        }

        [Interceptor(typeof(InterceptorA))]
        public class OneInterceptorService
        {
            public virtual void Method()
            {

            }

            public virtual Task MethodAsync()
            {
                return Task.CompletedTask;
            }

            public virtual Task<int> MethodAsyncT()
            {
                return Task.FromResult(1);
            }

            public virtual ValueTask MethodValueTaskAsync()
            {
                return new ValueTask(Task.CompletedTask);
            }

            public virtual ValueTask<int> MethodValueTaskAsyncT()
            {
                return new ValueTask<int>(1);
            }
        }

        [Interceptor(typeof(InterceptorA))]
        [Interceptor(typeof(InterceptorB))]
        public class TwoInterceptorService
        {
            public virtual void Method()
            {

            }

            public virtual Task MethodAsync()
            {
                return Task.CompletedTask;
            }

            public virtual Task<int> MethodAsyncT()
            {
                return Task.FromResult(1);
            }

            public virtual ValueTask MethodValueTaskAsync()
            {
                return new ValueTask(Task.CompletedTask);
            }

            public virtual ValueTask<int> MethodValueTaskAsyncT()
            {
                return new ValueTask<int>(1);
            }
        }

        [Interceptor(typeof(InterceptorA))]
        [Interceptor(typeof(InterceptorB))]
        [Interceptor(typeof(InterceptorC))]
        public class ThreeInterceptorService
        {
            public virtual void Method()
            {

            }

            public virtual Task MethodAsync()
            {
                return Task.CompletedTask;
            }

            public virtual Task<int> MethodAsyncT()
            {
                return Task.FromResult(1);
            }

            public virtual ValueTask MethodValueTaskAsync()
            {
                return new ValueTask(Task.CompletedTask);
            }

            public virtual ValueTask<int> MethodValueTaskAsyncT()
            {
                return new ValueTask<int>(1);
            }
        }

        public class InterceptorA : IInterceptor
        {
            public Task InvokeAsync(IMethodInvocation invocation, Func<IMethodInvocation, Task> next)
            {
                return next(invocation);
            }
        }

        public class InterceptorB : IInterceptor
        {
            public Task InvokeAsync(IMethodInvocation invocation, Func<IMethodInvocation, Task> next)
            {
                return next(invocation);
            }
        }

        public class InterceptorC : IInterceptor
        {
            public Task InvokeAsync(IMethodInvocation invocation, Func<IMethodInvocation, Task> next)
            {
                return next(invocation);
            }
        }
    }
}
