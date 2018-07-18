using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IMethodInvocation
    {
        object Result { get; }

        object Instance { get; }

        object[] Arguments { get; }

        MethodInfo MethodInfo { get; }

        bool SetResultValue(object value);

        IInterceptor[] Interceptors { get; }
    }
}
