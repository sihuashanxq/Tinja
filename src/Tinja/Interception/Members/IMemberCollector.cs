using System.Collections.Generic;

namespace Tinja.Interception.Members
{
    public interface IMemberCollector
    {
        IEnumerable<ProxyMember> Collect();
    }
}
