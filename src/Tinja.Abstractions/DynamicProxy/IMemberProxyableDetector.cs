using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IMemberProxyableDetector
    {
        bool IsProxyable(MemberInfo memberInfo);
    }
}
