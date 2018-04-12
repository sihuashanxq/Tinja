using Microsoft.Extensions.DependencyInjection;
using System;
using Tinja;
using Tinja.Annotations;
using Tinja.LifeStyle;

namespace Sample
{
    public interface IServiceA
    {

    }

    public class ServiceA : IServiceA
    {
        //[Inject]
        public IServiceB Service { get; set; }

        public ServiceA()
        {
            Console.WriteLine(GetHashCode());
        }
    }

    public interface IServiceB
    {
        void Up();
    }

    public class ServiceB : IServiceB
    {
        //[Inject]
        public IServiceA Service { get; set; }

        public ServiceB()
        {
            Console.WriteLine(GetHashCode());
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
        public ServiceXX(T t)
        {

        }
    }

    public class Service : IService
    {
        //[Inject]
        public IServiceA ServiceA { get; set; }

        public Service(IServiceB b)
        {
            Console.WriteLine(GetHashCode());
        }

        public void Dispose()
        {

        }

        public void Give()
        {
            Console.WriteLine("Give");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var ioc = new Container();

            ioc.AddService(typeof(IServiceA), typeof(ServiceA), ServiceLifeStyle.Scoped);
            ioc.AddService(typeof(IServiceB), typeof(ServiceB), ServiceLifeStyle.Scoped);
            ioc.AddService(typeof(IService), typeof(Service), ServiceLifeStyle.Scoped);

            var st = new System.Diagnostics.Stopwatch();
            var services = new ServiceCollection();

            services.AddScoped<IServiceA, ServiceA>();
            services.AddScoped<IServiceB, ServiceB>();
            services.AddScoped<IService, Service>();

            var provider = services.BuildServiceProvider();
            //provider.GetService(typeof(IEnumerable<IEnumerable<IService>>));
            //provider.GetService<IService>();

            st.Reset();
            st.Start();

            for (var i = 0; i < 1000_00000; i++)
            {
                provider.GetService(typeof(IService));
            }

            st.Stop();
            Console.WriteLine(st.ElapsedMilliseconds);

            var resolver = ioc.BuildResolver();

            var service = resolver.Resolve(typeof(IService));

            st.Reset();
            st.Start();

            for (var i = 0; i < 1000_00000; i++)
            {
                resolver.Resolve(typeof(IService));
            }

            st.Stop();
            Console.WriteLine(st.ElapsedMilliseconds);

            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }
    }
}
