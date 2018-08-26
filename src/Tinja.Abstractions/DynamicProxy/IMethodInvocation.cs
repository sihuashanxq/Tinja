using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IMethodInvocation
    {
        MethodInfo Method { get; }

        MemberInfo Target { get; }

        object[] Arguments { get; }

        object ProxyInstance { get; }

        object ResultValue { get; set; }
    }
}
