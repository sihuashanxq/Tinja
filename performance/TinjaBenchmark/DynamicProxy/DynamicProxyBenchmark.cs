using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Tinja.Abstractions.DynamicProxy;
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

        [Benchmark]
        public void ExecWithNoneIntercptor()
        {
            _noneInterceptorService.Method();
        }

        [Benchmark]
        public void ExecWithOneIntercptor()
        {
            _oneInterceptorService.Method();
        }

        [Benchmark]
        public void ExecWithTwoIntercptors()
        {
            _twoInterceptorService.Method();
        }

        [Benchmark]
        public void ExecWithThreeIntercptors()
        {
            _threeInterceptorService.Method();
        }

        public class NoneInterceptorService
        {
            public virtual void Method()
            {

            }
        }

        public class OneInterceptorService
        {
            [Interceptor(typeof(InterceptorA))]
            public virtual void Method()
            {

            }
        }

        public class TwoInterceptorService
        {
            [Interceptor(typeof(InterceptorA))]
            [Interceptor(typeof(InterceptorB))]
            public virtual void Method()
            {

            }
        }

        public class ThreeInterceptorService
        {
            [Interceptor(typeof(InterceptorA))]
            [Interceptor(typeof(InterceptorB))]
            [Interceptor(typeof(InterceptorC))]
            public virtual void Method()
            {

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
