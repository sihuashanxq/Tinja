using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Tinja;
using Tinja.Interception;
using Tinja.ServiceLife;

namespace Sample
{
    public interface IServiceA
    {

    }

    public class ServiceA : IServiceA
    {
        public ServiceA()
        {
            //Console.WriteLine("A" + GetHashCode());
        }
    }

    public interface IServiceB
    {
        void Up();
    }

    public class ServiceB : IServiceB
    {
        [Inject]
        public IServiceA Service { get; set; }

        public ServiceB()
        {
            Console.WriteLine("B" + GetHashCode());
        }

        public void Up()
        {
            Console.WriteLine("Up");
        }
    }

    public interface IService
    {
        void Give();
    }

    public interface IServiceXX<T>
    {

    }

    public class ServiceXX<T> : IServiceXX<T>
    {
        [Inject]
        public IServiceXX<T> Instance { get; set; }

        public T t;

        public ServiceXX(T t)
        {
            this.t = t;
        }
    }

    public class Service : IService
    {
        public Service(IServiceA b)
        {
            //B = b;
            //Console.WriteLine("A" + b.GetHashCode());
            ////Console.WriteLine("A" + serviceA.GetHashCode());
            //Console.WriteLine(GetHashCode());
        }

        public void Dispose()
        {

        }

        public void Give()
        {
            Console.WriteLine("Give");
        }
    }

    public class A
    {
        [Inject]
        public A A2 { get; set; }
    }

    public class InterceptorTest : IIntereceptor
    {
        public Task IntereceptAsync(MethodInvocationContext context, Func<MethodInvocationContext, Task> next)
        {
            throw new NotImplementedException();
        }
    }

    [Interceptor(typeof(InterceptorTest))]
    public class Abc
    {

    }

    class Program
    {
        static void Main(string[] args)
        {
            var watch = new System.Diagnostics.Stopwatch();
            var container = new Container();
            var services = new ServiceCollection();

            container.AddService(typeof(IServiceA), _ => new ServiceA(), ServiceLifeStyle.Transient);
            container.AddService(typeof(IServiceB), typeof(ServiceB), ServiceLifeStyle.Scoped);
            container.AddService(typeof(IService), typeof(Service), ServiceLifeStyle.Scoped);
            container.AddService(typeof(IServiceXX<>), typeof(ServiceXX<>), ServiceLifeStyle.Scoped);
            container.AddTransient<InterceptorTest, InterceptorTest>();
            container.AddTransient(typeof(Abc), ProxyUtil.GenerateProxyType(typeof(Abc), typeof(Abc)));


            services.AddScoped<IServiceA, ServiceA>();
            services.AddTransient<IServiceB, ServiceB>();
            services.AddTransient<IService, Service>();

            var provider = services.BuildServiceProvider();
            var resolver = container.BuildResolver();

            watch.Reset();
            watch.Start();

            //for (var i = 0; i < 1000_0000; i++)
            //{
            //    provider.GetService(typeof(IService));
            //}

            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            var xx = resolver.Resolve<Abc>();
            var y = resolver.Resolve(typeof(IServiceA));
            var b = resolver.Resolve(typeof(IServiceB));
            var service = resolver.Resolve(typeof(IService));

            watch.Reset();
            watch.Start();

            for (var i = 0; i < 10000_000; i++)
            {
                service = resolver.Resolve(typeof(IService));
            }

            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);

            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }
    }
}
