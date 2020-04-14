using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Tinja.Abstractions.Extensions;
using Tinja.Abstractions.Injection;
using Tinja.Core;
using Tinja.Core.Injection;

namespace TinjaBenchmark.Injection
{
    [MemoryDiagnoser]
    [AllStatisticsColumn]
    public class SingletonBenchmark
    {
        private readonly IServiceResolver _serviceResolver;

        public SingletonBenchmark()
        {
            _serviceResolver = new Container()
                .AddSingleton<ISingletonSerivice, SingletonServiceB>()
                .AddSingleton<ISingletonSerivice, SingletonServiceC>()
                .AddSingleton<ISingletonSerivice>(resolver => new SingletonServiceD())
                .AddSingleton<ISingletonSerivice, SingletonServiceA>()
                .AddSingleton<SingletonServiceA>()
                .AddSingleton<SingletonServiceB>()
                .AddSingleton<SingletonServiceC>()
                .AddSingleton<SingletonServiceD>(resolver => new SingletonServiceD())
                .AddSingleton<SingletonServiceE>(new SingletonServiceE())
                .AddSingleton<SingletonServiceF>()
                .AddSingleton<SingletonServiceG>()
                .BuildServiceResolver();
        }

        [Benchmark]
        public object ResolveSerivice()
        {
            return _serviceResolver.ResolveService<SingletonServiceA>();
        }

        [Benchmark]
        public object ResolveOneCtorDependencySerivice()
        {
            return _serviceResolver.ResolveService<SingletonServiceB>();
        }

        [Benchmark]
        public object ResolveTwoCtorDependencySerivice()
        {
            return _serviceResolver.ResolveService<SingletonServiceC>();
        }

        [Benchmark]
        public object ResolveDelegateSerivice()
        {
            return _serviceResolver.ResolveService<SingletonServiceD>();
        }

        [Benchmark]
        public object ResolveInstanceSerivice()
        {
            return _serviceResolver.ResolveService<SingletonServiceE>();
        }

        [Benchmark]
        public object ResolveOnePropertyDependencySerivice()
        {
            return _serviceResolver.ResolveService<SingletonServiceF>();
        }

        [Benchmark]
        public object ResolveTwoPropertyDependencySerivice()
        {
            return _serviceResolver.ResolveService<SingletonServiceG>();
        }

        [Benchmark]
        public object ResolveFourElementsEnumerableSerivice()
        {
            return _serviceResolver.ResolveService<IEnumerable<ISingletonSerivice>>();
        }

        public interface ISingletonSerivice
        {

        }

        public class SingletonServiceA : ISingletonSerivice
        {

        }

        public class SingletonServiceB : ISingletonSerivice
        {
            public SingletonServiceB(SingletonServiceA A)
            {

            }
        }

        public class SingletonServiceC : ISingletonSerivice
        {
            public SingletonServiceC(SingletonServiceA A, SingletonServiceB B)
            {

            }
        }

        public class SingletonServiceD : ISingletonSerivice
        {

        }

        public class SingletonServiceE
        {
   
        }

        public class SingletonServiceF
        {
            [Inject]
            public SingletonServiceD PropertyD { get; set; }
        }

        public class SingletonServiceG
        {
            [Inject]
            public SingletonServiceA PropertyA { get; set; }

            [Inject]
            public SingletonServiceD PropertyD { get; set; }
        }
    }
}
