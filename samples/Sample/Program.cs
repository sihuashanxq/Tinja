using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Abstractions.Extensions;
using Tinja.Core;
using Tinja.Core.DynamicProxy;
using Tinja.Core.Extensions;

namespace ConsoleApp
{
    class Program
    {
        static void Main()
        {
            var container = new Container();
            var s = new UserRepository();
            container.AddTransient(typeof(IUserService),(r) => new UserService1());
            container.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            container.AddTransient(typeof(IUserRepository), typeof(UserRepository));
            container.AddTransient<UserServiceDataAnnotationInterceptor, UserServiceDataAnnotationInterceptor>();
            container.AddTransient<UserServiceInterceptor, UserServiceInterceptor>();
            container.AddTransient<IInterceptorMetadataCollector, MemberInterceptionCollector>();

            var resolver = container.UseDynamicProxy().BuildServiceResolver();

            IUserService u1 = null, u2 = null, u3 = null;

            Task.Run(() => { u1 = resolver.Resolve<IUserService>(); });
            Task.Run(() => { u2 = resolver.Resolve<IUserService>(); });
            Task.Run(() => { u3 = resolver.Resolve<IUserService>(); });

            Console.WriteLine(u1 == u2);
            Console.WriteLine(u2 == u3);
            var userServices = resolver.Resolve<IEnumerable<IUserService>>();
            var userService = resolver.ResolveRequired<IUserService>();
            var userRepository = resolver.Resolve<IUserRepository>();
            var name = userService.GetIdAsync();

            Console.WriteLine("name:" + name);

            using (var scope = resolver.CreateScope())
            {
                var repository = resolver.Resolve<IRepository<IUserService>>();
            }

            userService.GetUserName(2);

            var type = typeof(IUserService);
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            for (var i = 0; i < 100000000; i++)
            {
                resolver.Resolve(type);
            }

            stopWatch.Stop();

            Console.WriteLine(stopWatch.ElapsedMilliseconds);

            Console.ReadLine();
        }
    }
}
