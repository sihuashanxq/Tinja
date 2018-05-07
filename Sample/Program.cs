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

    public class InterceptorTest : IInterceptor
    {
        public Task InterceptAsync(MethodInvocation invocation, Func<MethodInvocation, Task> next)
        {
            Console.WriteLine("brefore InterceptorTest             ");
            var task = next(invocation);
            Console.WriteLine("after InterceptorTest");

            return task;
        }
    }

    public class InterceptorTest2 : IInterceptor
    {
        public Task InterceptAsync(MethodInvocation invocation, Func<MethodInvocation, Task> next)
        {
            Console.WriteLine("brefore InterceptorTest2222222222222222");
            var task = next(invocation);
            Console.WriteLine("after InterceptorTest222222222222222222");
            return task;
        }
    }

    [Interceptor(typeof(InterceptorTest))]
    public class Abc:IAbc
    {
        public virtual object M()
        {
            Console.WriteLine("方法执行 执行");
            return 6;
        }
    }

    [Interceptor(typeof(InterceptorTest2))]
    public interface IAbc
    {
        object M();
    }

    class Program
    {
        static void Main(string[] args)
        {
            var x = new Tinja.Interception.TypeMembers.InterfaceTypeMemberCollector(typeof(IAbc), typeof(Abc)).Collect();

            var watch = new System.Diagnostics.Stopwatch();
            var container = new Container();
            var services = new ServiceCollection();

            container.AddService(typeof(IServiceA), _ => new ServiceA(), ServiceLifeStyle.Transient);
            container.AddService(typeof(IServiceB), typeof(ServiceB), ServiceLifeStyle.Scoped);
            container.AddService(typeof(IService), typeof(Service), ServiceLifeStyle.Scoped);
            container.AddService(typeof(IServiceXX<>), typeof(ServiceXX<>), ServiceLifeStyle.Scoped);
            container.AddTransient<InterceptorTest, InterceptorTest>();
            container.AddTransient<InterceptorTest2, InterceptorTest2>();
            container.AddTransient<IMethodInvocationExecutor, MethodInvocationExecutor>();
            container.AddTransient<IMethodInvokerBuilder, MethodInvokerBuilder>();
            container.AddTransient(typeof(Abc), typeof(Abc));
            var proxyType = new ProxyTypeGenerator(typeof(Abc), typeof(Abc)).CreateProxyType();

            container.AddTransient(proxyType, proxyType);

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

            var proxyService = resolver.Resolve(proxyType) as Abc;

            var value = proxyService.M();

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
