using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IProxyGenerationReferee
    {
        bool ShouldIntercepted(MemberInfo memberInfo);
    }
}
