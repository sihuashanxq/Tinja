using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IMethodInvocation
    {
        object Result { get; }

        bool SetResultValue(object value);

        object Instance { get; }

        MethodInfo MethodInfo { get; }

        object[] ArgumentValues { get; }

        IInterceptor[] Interceptors { get; }

        MethodInvocationType InvocationType { get; }
    }
}
