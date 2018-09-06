using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Abstractions.Extensions;
using Tinja.Core;
using Tinja.Core.DynamicProxy;
using Tinja.Core.DynamicProxy.Extensions;
using Tinja.Core.Extensions;

namespace ConsoleApp
{
    class Program
    {
        static void Main()
        {
            var serviceResolver = new Container()
                .AddScoped<IUserService, UserService>()
                .AddTransient<UserServiceInterceptor, UserServiceInterceptor>()
                .AddTransient<UserServiceInterceptor2, UserServiceInterceptor2>()
                .AddTransient<IInterceptorMetadataCollector, InterceptorMetadataCollector>()
                .AddDynamicProxy()
                .BuildServiceResolver();

            using (var scopeResolver = serviceResolver.CreateScope())
            {
                var userService = scopeResolver.ServiceResolver.ResolveService<IUserService>();
                if (userService == null)
                {
                    throw new NullReferenceException(nameof(userService));
                }

                Console.WriteLine(userService.GetString(2));
            }

            Console.ReadLine();
        }
    }
}
