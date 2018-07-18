using System;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy;

namespace ConsoleApp
{
    public class UserServiceInterceptor : IInterceptor
    {
        public async Task InvokeAsync(IMethodInvocation invocation, Func<IMethodInvocation, Task> next)
        {
            Console.WriteLine($"InterceptorType:{GetType().FullName} :::: { invocation.MethodInfo.Name }:executing");

            await next(invocation);

            Console.WriteLine($"InterceptorType:{GetType().FullName}::::{invocation.MethodInfo.Name }:executed :return{invocation.Result}->{invocation.Result.ToString() + "Interceptor"}");

            invocation.SetResultValue(invocation.Result.ToString() + "Interceptor");
        }
    }

    public class UserServiceDataAnnotationInterceptor : IInterceptor
    {
        public async Task InvokeAsync(IMethodInvocation invocation, Func<IMethodInvocation, Task> next)
        {
            Console.WriteLine($"InterceptorType:{GetType().FullName} :::: { invocation.MethodInfo.Name }:executing");

            await next(invocation);

            Console.WriteLine($"InterceptorType:{GetType().FullName}::::{invocation.MethodInfo.Name }:executed");
        }
    }
}
