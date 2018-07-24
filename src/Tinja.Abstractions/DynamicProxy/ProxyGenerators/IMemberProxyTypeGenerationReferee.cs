using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy.ProxyGenerators
{
    public interface IMemberProxyTypeGenerationReferee
    {
        bool ShouldProxy(MemberInfo memberInfo);
    }
}
