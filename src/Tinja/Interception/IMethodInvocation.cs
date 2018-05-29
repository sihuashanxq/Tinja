using System.Reflection;

namespace Tinja.Interception
{
    public interface IMethodInvocation
    {
        object Target { get; }

        object ResultValue { get; set; }

        MethodInfo TargetMethod { get; }

        object[] ParameterValues { get; }

        IInterceptor[] Interceptors { get; }
    }
}
