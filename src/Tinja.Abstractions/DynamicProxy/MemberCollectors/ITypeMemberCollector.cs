using System;
using System.Collections.Generic;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface ITypeMemberCollector
    {
        IEnumerable<MemberMetadata> Collect(Type typeInfo);
    }
}
