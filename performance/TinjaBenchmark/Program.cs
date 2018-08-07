using BenchmarkDotNet.Running;
using TinjaBenchmark.Injection;

namespace TinjaBenchmark
{
    class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<TransientBenchmark>();
            BenchmarkRunner.Run<ScopedBenchmark>();
            BenchmarkRunner.Run<SingletonBenchmark>();
        }
    }
}
