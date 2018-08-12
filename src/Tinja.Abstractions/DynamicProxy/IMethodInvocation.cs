using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IMethodInvocation
    {
        MethodInfo Method { get; }

        object[] Parameters { get; }

        object ProxyInstance { get; }

        MemberInfo TargetMember { get; }

        object ResultValue { get; set; }
    }
}
