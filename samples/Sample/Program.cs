using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Tinja;
using Tinja.Extensions;
using Tinja.Resolving;
using Tinja.Resolving.Activation.Builder;
using Tinja.Resolving.Dependency;
using Tinja.ServiceLife;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new Container();
            var s = new UserRepository();
            container.AddTransient<IUserService, UserService1>();
            container.AddTransient<IUserService, UserService>();
            container.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            container.AddSingleton<IUserRepository>(s);
            container.AddTransient<UserServiceDataAnnotationInterceptor, UserServiceDataAnnotationInterceptor>();
            container.AddTransient<UserServiceInterceptor, UserServiceInterceptor>();

            container.Configure(config => config.Interception.Providers.Add(new MemberInterceptionProvider()));
            container.Configure(config => config.Interception.EnableInterception = true);

            var resolver = container.BuildResolver();

            var userServices = resolver.Resolve<IEnumerable<IUserService>>();
            var userService = resolver.ResolveRequired<IUserService>();
            var userRepository = resolver.Resolve<IUserRepository>();
            var name = userService.GetUserName(2);

            Console.WriteLine("name:" + name);

            using (var scope = resolver.CreateScope())
            {
                var repository = resolver.Resolve<IRepository<IUserService>>();
            }

            var stopWatch = new Stopwatch();
            var type = typeof(IUserRepository);
            stopWatch.Start();

            for (var i = 0; i < 100000000; i++)
            {
                resolver.Resolve(type);
            }

            stopWatch.Stop();
            Console.WriteLine(stopWatch.ElapsedMilliseconds);
            Console.ReadKey();
        }
    }
}
