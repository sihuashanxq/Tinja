using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IProxyTypeGenerationReferee
    {
        bool ShouldProxy(MemberInfo memberInfo);
    }
}
