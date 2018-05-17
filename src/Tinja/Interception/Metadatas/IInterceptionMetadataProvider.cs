using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Tinja.Interception.TypeMembers;
using System.Reflection;
using System.Linq;

namespace Tinja.Interception
{
    public interface IInterceptionMetadataProvider
    {
        IEnumerable<InterceptionMetadata> GetInterceptionMetadatas(Type serviceType, Type implementionType);
    }
}
