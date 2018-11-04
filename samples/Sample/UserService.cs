using System;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Registrations;

namespace ConsoleApp
{
    [Interceptor(typeof(UserServiceInterceptor))]
    [Query]
    public interface IUserService
    {
        string GetString(int id);
    }

    public class UserService : IUserService, IDisposable
    {
        public virtual string GetString(int id)
        {
            Console.WriteLine("Return:" + GetType().Name + "Id");
            return GetType().Name + id;
        }

        public void Dispose()
        {
            Console.WriteLine(GetType().FullName + " Disposed!");
        }
    }
}
