using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;
using Tinja.Core;
using Tinja.Core.Extensions;
using Tinja.Core.Injection;

namespace TinjaBenchmark.Injection
{
    [MemoryDiagnoser]
    [AllStatisticsColumn]
    public class ScopedBenchmark
    {
        private readonly IServiceResolver _serviceResolver;

        public ScopedBenchmark()
        {
            _serviceResolver = new Container()
                .AddScoped<IScopedSerivice, ScopedServiceB>()
                .AddScoped<IScopedSerivice, ScopedServiceC>()
                .AddScoped<IScopedSerivice>(resolver => new ScopedServiceD())
                .AddScoped<IScopedSerivice, ScopedServiceA>()
                .AddScoped<ScopedServiceA>()
                .AddScoped<ScopedServiceB>()
                .AddScoped<ScopedServiceC>()
                .AddScoped<ScopedServiceD>(resolver => new ScopedServiceD())
                .AddScoped<ScopedServiceE>()
                .AddScoped<ScopedServiceF>()
                .BuildServiceResolver();
        }

        [Benchmark]
        public object ResolveSerivice()
        {
            return _serviceResolver.ResolveService<ScopedServiceA>();
        }

        [Benchmark]
        public object ResolveOneCtorDependencySerivice()
        {
            return _serviceResolver.ResolveService<ScopedServiceB>();
        }

        [Benchmark]
        public object ResolveTwoCtorDependencySerivice()
        {
            return _serviceResolver.ResolveService<ScopedServiceC>();
        }

        [Benchmark]
        public object ResolveDelegateSerivice()
        {
            return _serviceResolver.ResolveService<ScopedServiceD>();
        }

        [Benchmark]
        public object ResolveOnePropertyDependencySerivice()
        {
            return _serviceResolver.ResolveService<ScopedServiceE>();
        }

        [Benchmark]
        public object ResolveTwoPropertyDependencySerivice()
        {
            return _serviceResolver.ResolveService<ScopedServiceF>();
        }

        [Benchmark]
        public object ResolveFourElementsEnumerableSerivice()
        {
            return _serviceResolver.ResolveService<IEnumerable<IScopedSerivice>>();
        }


        public interface IScopedSerivice
        {

        }

        public class ScopedServiceA : IScopedSerivice
        {

        }

        public class ScopedServiceB : IScopedSerivice
        {
            public ScopedServiceB(ScopedServiceA A)
            {

            }
        }

        public class ScopedServiceC : IScopedSerivice
        {
            public ScopedServiceC(ScopedServiceA A, ScopedServiceB B)
            {

            }
        }

        public class ScopedServiceD : IScopedSerivice
        {

        }

        public class ScopedServiceE
        {
            [Inject]
            public ScopedServiceD PropertyD { get; set; }
        }

        public class ScopedServiceF
        {
            [Inject]
            public ScopedServiceA PropertyA { get; set; }

            [Inject]
            public ScopedServiceD PropertyD { get; set; }
        }
    }
}
