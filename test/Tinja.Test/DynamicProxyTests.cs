using System;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.Extensions;
using Tinja.Core;
using Tinja.Core.DynamicProxy;
using Tinja.Core.Extensions;

using Xunit;

namespace Tinja.Test
{
    public class DynamicProxyTests
    {
        [Fact]
        public void ReturnValueRewriteTest()
        {
            var service = new Container()
                .AddTransient(typeof(IDynamicService), typeof(DynamicService))
                .AddTransient(typeof(ReturnRewriteInterceptor))
                .UseDynamicProxy()
                .BuildServiceResolver()
                .ResolveServiceRequired<IDynamicService>();

            Assert.Equal(1024, service.GetInt32());
            Assert.Equal(1024, service.GetInt32Async().Result);
            Assert.Equal(1024, service.GetInt32ValueTaskAsync().Result);
        }

        [Fact]
        public void InterfaceDynamicProxyTest()
        {
            var service = new Container()
                .AddTransient(typeof(IDynamicService))
                .AddTransient(typeof(ReturnRewriteInterceptor))
                .UseDynamicProxy()
                .BuildServiceResolver()
                .ResolveServiceRequired<IDynamicService>();

            Assert.Equal(1024, service.GetInt32());
            Assert.Equal(1024, service.GetInt32Async().Result);
            Assert.Equal(1024, service.GetInt32ValueTaskAsync().Result);
        }

        [Fact]
        public void AbstractClassDynamicProxyTest()
        {
            var service = new Container()
                .AddTransient(typeof(AbstractDynamicService))
                .AddTransient(typeof(ReturnRewriteInterceptor))
                .UseDynamicProxy()
                .BuildServiceResolver()
                .ResolveServiceRequired<AbstractDynamicService>();

            Assert.Equal(1024, service.GetInt32());
            Assert.Equal(1024, service.GetInt32Async().Result);
            Assert.Equal(1024, service.GetInt32ValueTaskAsync().Result);
        }
    }

    [Interceptor(typeof(ReturnRewriteInterceptor))]
    public interface IDynamicService
    {
        int GetInt32();

        Task<int> GetInt32Async();

        ValueTask<int> GetInt32ValueTaskAsync();
    }

    public abstract class AbstractDynamicService : IDynamicService
    {
        public abstract int GetInt32();

        public abstract Task<int> GetInt32Async();

        public abstract ValueTask<int> GetInt32ValueTaskAsync();
    }

    public class DynamicService : AbstractDynamicService
    {
        public override int GetInt32()
        {
            return 0;
        }

        public override Task<int> GetInt32Async()
        {
            return Task.FromResult(0);
        }

        public override ValueTask<int> GetInt32ValueTaskAsync()
        {
            return new ValueTask<int>(0);
        }
    }

    public class ReturnRewriteInterceptor : IInterceptor
    {
        public async Task InvokeAsync(IMethodInvocation invocation, Func<IMethodInvocation, Task> next)
        {
            await next(invocation);

            if (invocation.Method.ReturnType == typeof(int))
            {
                invocation.ResultValue = 1024;
            }
            else if (invocation.Method.ReturnType == typeof(Task<int>))
            {
                invocation.ResultValue = Task.FromResult(1024);
            }
            else if (invocation.Method.ReturnType == typeof(ValueTask<int>))
            {
                invocation.ResultValue = Task.FromResult(1024);
            }
        }
    }
}
