using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy.ProxyGenerators
{
    public interface IProxyGenerationReferee
    {
        bool ShouldIntercepted(MemberInfo memberInfo);
    }
}
