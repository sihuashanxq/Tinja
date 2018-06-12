using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception
{
    public interface IMemberInterceptionProvider
    {
        IEnumerable<MemberInterception> GetInterceptions(MemberInfo memberInfo);
    }
}
