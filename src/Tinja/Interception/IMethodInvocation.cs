using System.Reflection;

namespace Tinja.Interception
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
