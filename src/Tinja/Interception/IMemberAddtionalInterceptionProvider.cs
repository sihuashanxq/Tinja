using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception
{
    public interface IMemberAddtionalInterceptionProvider
    {
        IEnumerable<MemberInterception> GetInterceptions(MemberInfo memberInfo);
    }
}
