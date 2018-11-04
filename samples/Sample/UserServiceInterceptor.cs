using System;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Registrations;

namespace ConsoleApp
{
    public class QueryAttribute : InterceptorAttribute, IInterceptor
    {
        public QueryAttribute() : base(typeof(QueryAttribute))
        {

        }

        public Task InvokeAsync(IMethodInvocation invocation, Func<IMethodInvocation, Task> next)
        {
            Console.WriteLine("ok");
            return next(invocation);
        }
    }

    public class UserServiceInterceptor : IInterceptor
    {
        public async Task InvokeAsync(IMethodInvocation invocation, Func<IMethodInvocation, Task> next)
        {
            Console.WriteLine($"InterceptorType:{GetType().FullName} :::: { invocation.Method.Name }:executing");

            await next(invocation);

            Console.WriteLine($"InterceptorType:{GetType().FullName}::::{invocation.Method.Name }:executed");
        }
    }

    public class UserServiceInterceptor2 : IInterceptor
    {
        public async Task InvokeAsync(IMethodInvocation invocation, Func<IMethodInvocation, Task> next)
        {
            Console.WriteLine($"InterceptorType:{GetType().FullName} :::: { invocation.Method.Name }:executing");

            await next(invocation);

            Console.WriteLine($"InterceptorType:{GetType().FullName}::::{invocation.Method.Name }:executed");
        }
    }
}
