using System.Collections.Generic;

namespace Tinja.Interception.TypeMembers
{
    public interface ITypeMemberCollector
    {
        IEnumerable<TypeMember> Collect();
    }
}
