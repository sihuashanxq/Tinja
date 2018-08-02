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

            container.AddTransient(typeof(IUserService), (r) => new UserService1());
            container.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            container.AddTransient(typeof(IUserRepository), typeof(UserRepository));
            container.AddTransient<UserServiceDataAnnotationInterceptor, UserServiceDataAnnotationInterceptor>();
            container.AddTransient<UserServiceInterceptor, UserServiceInterceptor>();
            container.AddTransient<IInterceptorMetadataCollector, MemberInterceptionCollector>();

            var resolver = container.UseDynamicProxy().BuildServiceResolver();

            IUserService u1 = null, u2 = null, u3 = null;

            Task.Run(() => { u1 = resolver.ResolveService<IUserService>(); });
            Task.Run(() => { u2 = resolver.ResolveService<IUserService>(); });
            Task.Run(() => { u3 = resolver.ResolveService<IUserService>(); });

            Console.WriteLine(u1 == u2);
            Console.WriteLine(u2 == u3);
            var userServices = resolver.ResolveService<IEnumerable<IUserService>>();
            var userService = resolver.ResolveServiceRequired<IUserService>();
            var userRepository = resolver.ResolveService<IUserRepository>();
            var name = userService.GetIdAsync();

            Console.WriteLine("name:" + name);

            using (var scope = resolver.CreateScope())
            {
                var repository = resolver.ResolveService<IRepository<IUserService>>();
            }

            userService.GetUserName(2);

            var type = typeof(IUserRepository);
            var stopWatch = new Stopwatch();
            for (var n = 0; n < 10; n++)
            {
                stopWatch.Restart();
                for (var i = 0; i < 100000000; i++)
                {
                    resolver.ResolveService(type);
                }

                stopWatch.Stop();

                Console.WriteLine(stopWatch.ElapsedMilliseconds);
            }

            Console.ReadLine();
        }
    }
}
