using System;
using System.Collections;
using System.Collections.Generic;
using Tinja;
using Tinja.Extensions;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new Container();
            container.AddTransient<IUserService, UserService1>();
            container.AddTransient<IUserService, UserService>();
            container.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            container.AddTransient(typeof(IUserRepository), _ => new UserRepository());
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

            Console.ReadKey();
        }
    }
}
