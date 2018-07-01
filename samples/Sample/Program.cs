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

            var builder = new ExpressionActivatorBuilder();
            var element = new ConstructorCallDependencyElement()
            {
                ServiceType = typeof(IRepository<>).MakeGenericType(typeof(IUserRepository)),
                ConstructorInfo = typeof(Repository<>).MakeGenericType(typeof(IUserRepository)).GetConstructors()[0],
                ImplementionType = typeof(Repository<>).MakeGenericType(typeof(IUserRepository)),
                LifeStyle = ServiceLifeStyle.Transient
            };

            element.Parameters = new Dictionary<ParameterInfo, CallDepenencyElement>()
            {
                [element.ConstructorInfo.GetParameters()[0]] = new ConstructorCallDependencyElement()
                {
                    ServiceType = typeof(IUserRepository),
                    ConstructorInfo = typeof(UserRepository).GetConstructor(Type.EmptyTypes),
                    ImplementionType = typeof(UserRepository),
                    LifeStyle = ServiceLifeStyle.Transient,
                    Parameters = new Dictionary<ParameterInfo, CallDepenencyElement>()
                }
            };

            var func = builder.Build(element);
            var resp = func(resolver, resolver.ServiceLifeScope);

            Console.ReadKey();
        }
    }
}
