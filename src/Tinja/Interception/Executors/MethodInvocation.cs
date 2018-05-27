using System.Reflection;

namespace Tinja.Interception.Executors
{
    public class MethodInvocation : IMethodInvocation
    {
        public object Target { get; }

        public MethodInfo TargetMethod { get; }

        public object ReturnValue { get; set; }

        public object[] ParameterValues { get; }

        public IInterceptor[] Interceptors { get; }

        public MethodInvocation(object target, MethodInfo targetMethod, object[] parameterValues, IInterceptor[] interceptors)
        {
            Target = target;
            TargetMethod = targetMethod;
            ParameterValues = parameterValues;
            Interceptors = interceptors;
        }
    }
}
