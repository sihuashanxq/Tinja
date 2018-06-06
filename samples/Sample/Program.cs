using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Tinja;
using Tinja.Interception;
using Tinja.Interception.Executors;
using Tinja.Interception.Generators;
using Tinja.Interception.Internal;
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
        public Task InvokeAsync(IMethodInvocation invocation, Func<IMethodInvocation, Task> next)
        {
            return next(invocation);
            Console.WriteLine("brefore InterceptorTest             ");
            var task = next(invocation);
            Console.WriteLine("after InterceptorTest");
            return task;
        }
    }

    public class InterceptorTest2 : IInterceptor
    {
        public Task InvokeAsync(IMethodInvocation invocation, Func<IMethodInvocation, Task> next)
        {
            invocation.ResultValue = 10000;
            //Console.WriteLine("brefore InterceptorTest2222222222222222");
            var task = next(invocation);
            //Console.WriteLine("after InterceptorTest222222222222222222");
            return task;
        }
    }

    public class InterceptorTest3 : IInterceptor
    {
        public Task InvokeAsync(IMethodInvocation invocation, Func<IMethodInvocation, Task> next)
        {
            return next(invocation);
            Console.WriteLine("brefore InterceptorTest2222222222222222");
            var task = next(invocation);
            Console.WriteLine("after InterceptorTest222222222222222222");
            return task;
        }
    }

    public class Abc : IAbc
    {
        public event Action OnOk;

        [Interceptor(typeof(InterceptorTest))]
        public virtual T M<T>() where T : class
        {
            return default(T);
        }

        public void M2()
        {

        }

        public int GetId()
        {
            throw new NotImplementedException();
        }

        [Interceptor(typeof(InterceptorTest))]
        public virtual object Id { get; set; }
    }

    [Interceptor(typeof(InterceptorTest3))]
    public class Abc2 : Abc
    {
        //public override object M()
        //{
        //    Console.WriteLine("方法执行 执行");
        //    return 6;
        //}
    }

    public interface IAbc
    {
        [Interceptor(typeof(InterceptorTest3))]
        T M<T>() where T : class;

        int GetId();
    }

    [Interceptor(typeof(InterceptorTest3))]
    public abstract class A2
    {
        public abstract string M();

        public virtual void SetId(ref int id)
        {
            id = 2;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var watch = new System.Diagnostics.Stopwatch();
            var container = new Container();

            container.AddService(typeof(IServiceA), _ => new ServiceA(), ServiceLifeStyle.Transient);
            container.AddService(typeof(IServiceB), typeof(ServiceB), ServiceLifeStyle.Scoped);
            container.AddService(typeof(IService), typeof(Service), ServiceLifeStyle.Scoped);
            container.AddService(typeof(IServiceXX<>), typeof(ServiceXX<>), ServiceLifeStyle.Scoped);
            container.AddTransient<InterceptorTest, InterceptorTest>();
            container.AddTransient<InterceptorTest2, InterceptorTest2>();
            container.AddTransient<InterceptorTest3, InterceptorTest3>();

            container.AddTransient<A2, A2>();
            var resolver = container.BuildResolver();

            watch.Reset();
            watch.Start();
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            var z = 5;
            var a2 = resolver.Resolve(typeof(A2)) as A2;
            a2.SetId(ref z);

            watch.Reset();
            watch.Start();
            for (var i = 0; i < 1000000; i++)
            {
                resolver.Resolve(typeof(A2));
            }

            watch.Start();
            Console.WriteLine("Inter:" + watch.ElapsedMilliseconds);
            watch.Reset();
            var xxxxxx = new Abc();
            watch.Start();
            for (var i = 0; i < 10000000; i++)
            {
                xxxxxx.M2();
            }

            watch.Start();
            Console.WriteLine("Inter2:" + watch.ElapsedMilliseconds);

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

        private static void ProxyService_OnOk()
        {

        }
    }
}
