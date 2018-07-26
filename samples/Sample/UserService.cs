using Tinja;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Core.DynamicProxy;

namespace ConsoleApp
{
    [Interceptor(typeof(UserServiceDataAnnotationInterceptor), Inherited = true)]
    public interface IUserService
    {
        string GetUserName(int id);
    }

    public class UserService1 : IUserService
    {
        public UserService1()
        {

        }

        public virtual string GetUserName(int id)
        {
            return "UserService1:Name:" + id;
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
