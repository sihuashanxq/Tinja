using System;
using System.Reflection;

namespace Tinja.Interception
{
    public interface IMethodInvocation
    {
        object Object { get; }

        Type TargetType { get; }

        MethodInfo Method { get; }

        object ResultValue { get; set; }

        object[] ParameterValues { get; }

        IInterceptor[] Interceptors { get; }
    }
}
