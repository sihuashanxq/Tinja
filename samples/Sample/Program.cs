using System;
using Tinja;
using Tinja.Extensions;
using Tinja.ServiceLife;

namespace ConsoleApp
{
    public interface IServiceA
    {

    }

    public class ServiceA : IServiceA
    {
        public ServiceA()
        {

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

    public interface IServiceGeneric<T>
    {

    }

    public class ServiceGeneric<T> : IServiceGeneric<T>
    {
        [Inject]
        public IServiceGeneric<T> Instance { get; set; }

        public T t;

        public ServiceGeneric(T t)
        {
            this.t = t;
        }
    }

    public class Service : IService
    {
        public Service(IServiceA b)
        {

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
            var container = new Container();

            container.AddService(typeof(IServiceA), typeof(ServiceA), ServiceLifeStyle.Transient);
            container.AddService(typeof(IServiceB), typeof(ServiceB), ServiceLifeStyle.Transient);
            container.AddService(typeof(IService), typeof(Service), ServiceLifeStyle.Transient);
            container.AddService(typeof(IServiceGeneric<>), typeof(ServiceGeneric<>), ServiceLifeStyle.Scoped);

            var resolver = container.BuildResolver();
            resolver.Resolve(typeof(IService));
            resolver.Resolve<IServiceB>();
            resolver.ResolveRequired(typeof(IService));
            resolver.ResolveRequired<IService>();

            Console.ReadKey();
        }
    }
}
