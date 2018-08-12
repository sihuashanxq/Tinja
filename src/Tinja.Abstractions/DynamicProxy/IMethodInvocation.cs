using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IMethodInvocation
    {
        object Result { get; set; }

        object Instance { get; }

        MethodInfo MethodInfo { get; }

        object[] ArgumentValues { get; }

        IInterceptor[] Interceptors { get; }

        MethodInvocationType InvocationType { get; }
    }
}
