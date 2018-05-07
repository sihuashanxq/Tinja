using System;
using System.Collections.Generic;

namespace Tinja.Interception.TypeMembers
{
    public interface ITypeMemberCollector
    {
        IEnumerable<TypeMemberMetadata> Collect();
    }
}
