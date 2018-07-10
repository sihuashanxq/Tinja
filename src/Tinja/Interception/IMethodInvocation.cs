using System;
using System.Reflection;

namespace Tinja.Interception
{
    public interface IMethodInvocation
    {
        MethodInfo Method { get; }

        object[] Arguments { get; }

        object ReturnValue { get; }

        object ContextObject { get; }

        IInterceptor[] Interceptors { get; }

        bool SetReturnValue(object value);
    }
}
