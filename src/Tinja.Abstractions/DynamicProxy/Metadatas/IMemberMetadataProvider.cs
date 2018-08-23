using System;
using System.Collections.Generic;

namespace Tinja.Abstractions.DynamicProxy.Metadatas
{
    public interface IMemberMetadataProvider
    {
        IEnumerable<MemberMetadata> GetMembers(Type typeInfo);
    }
}
