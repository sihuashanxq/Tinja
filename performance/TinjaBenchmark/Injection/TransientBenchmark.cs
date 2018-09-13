using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Extensions;
using Tinja.Core;
using Tinja.Core.Extensions;
using Tinja.Core.Injection;

namespace TinjaBenchmark.Injection
{
    [MemoryDiagnoser]
    [AllStatisticsColumn]
    public class TransientBenchmark
    {
        private readonly IServiceResolver _serviceResolver;

        public TransientBenchmark()
        {
            _serviceResolver = new Container()
                .AddTransient<ITransientService, TransientServiceB>()
                .AddTransient<ITransientService, TransientServiceC>()
                .AddTransient<ITransientService>(resolver => new TransientServiceD())
                .AddTransient<ITransientService, TransientServiceA>()
                .AddTransient<TransientServiceA>()
                .AddTransient<TransientServiceB>()
                .AddTransient<TransientServiceC>()
                .AddTransient<TransientServiceD>(resolver => new TransientServiceD())
                .AddTransient<TransientServiceE>()
                .AddTransient<TransientServiceF>()
                .AddTransient<TransientServiceG>()
                .BuildServiceResolver();
        }

        [Benchmark]
        public object ResolveSerivice()
        {
            return _serviceResolver.ResolveService<TransientServiceA>();
        }

        [Benchmark]
        public object ResolveOneCtorDependencySerivice()
        {
            return _serviceResolver.ResolveService<TransientServiceB>();
        }

        [Benchmark]
        public object ResolveTwoCtorDependencySerivice()
        {
            return _serviceResolver.ResolveService<TransientServiceC>();
        }

        [Benchmark]
        public object ResolveDelegateSerivice()
        {
            return _serviceResolver.ResolveService<TransientServiceD>();
        }


        public interface ITransientService
        {

        }

        public class TransientServiceA : ITransientService
        {

        }

        public class TransientServiceB : ITransientService
        {
            public TransientServiceB(TransientServiceA A)
            {

            }
        }

        public class TransientServiceC : ITransientService
        {
            public TransientServiceC(TransientServiceA A, TransientServiceB B)
            {

            }
        }

        public class TransientServiceD : ITransientService
        {

        }

        public class TransientServiceE
        {
            [Inject]
            public TransientServiceA Property { get; set; }
        }

        public class TransientServiceF
        {
            [Inject]
            public TransientServiceA PropertyA { get; set; }

            [Inject]
            public TransientServiceA PropertyA1 { get; set; }
        }

        public class TransientServiceG
        {
            [Inject]
            public TransientServiceA PropertyA { get; set; }

            [Inject]
            public TransientServiceA PropertyA1 { get; set; }

            [Inject]
            public TransientServiceA PropertyA2 { get; set; }
        }
    }
}
