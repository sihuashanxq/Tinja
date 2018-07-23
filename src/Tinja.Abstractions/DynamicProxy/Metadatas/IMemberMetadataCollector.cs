using System;
using System.Collections.Generic;

namespace Tinja.Abstractions.DynamicProxy.Metadatas
{
    public interface IMemberMetadataCollector
    {
        IEnumerable<MemberMetadata> Collect(Type typeInfo);
    }
}
