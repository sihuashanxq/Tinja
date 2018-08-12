using System;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy;

namespace ConsoleApp
{
    public class UserServiceInterceptor : IInterceptor
    {
        public async Task InvokeAsync(IMethodInvocation invocation, Func<IMethodInvocation, Task> next)
        {
            Console.WriteLine($"InterceptorType:{GetType().FullName} :::: { invocation.Method.Name }:executing");

            await next(invocation);

            Console.WriteLine($"InterceptorType:{GetType().FullName}::::{invocation.Method.Name }:executed :return{invocation.ResultValue}->{invocation.ResultValue.ToString() + "Interceptor"}");
        }
    }

    public class UserServiceDataAnnotationInterceptor : IInterceptor
    {
        public async Task InvokeAsync(IMethodInvocation invocation, Func<IMethodInvocation, Task> next)
        {
            Console.WriteLine($"InterceptorType:{GetType().FullName} :::: { invocation.Method.Name }:executing");

            await next(invocation);

            Console.WriteLine($"InterceptorType:{GetType().FullName}::::{invocation.Method.Name }:executed");
        }
    }
}
