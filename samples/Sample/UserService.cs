using System;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Core.Injection;

namespace ConsoleApp
{
    //[Interceptor(typeof(UserServiceDataAnnotationInterceptor), Inherited = true)]
    public interface IUserService
    {
        string GetUserName(int id);

        int GetId();

        ValueTask<int> GetIdAsync();
    }

    public class UserService1 : IUserService,IDisposable
    {
        public UserService1()
        {

        }

        public virtual int GetId()
        {
            return 0;
        }

        public ValueTask<int> GetIdAsync()
        {
            return new ValueTask<int>(0);
        }

        public virtual string GetUserName(int id)
        {
            return "UserService1:Name:" + id;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public virtual string GetUserName(int id)
        {
            return "UserService:Name:" + id;
        }

        public virtual int GetId()
        {
            return 1;
        }

        public ValueTask<int> GetIdAsync()
        {
            throw new System.NotImplementedException();
        }
    }

    public interface IUserRepository
    {

    }

    public class UserRepository : IUserRepository
    {

    }

    public class IRepository<T>
    {

    }

    public class Repository<T> : IRepository<T>
    {
        public T Value { get; }

        public Repository(T value)
        {
            Value = value;
        }
    }
}
