using System;
using Tinja.Abstractions.Injection.Graphs.Sites;

namespace Tinja.Abstractions.Injection.Graphs
{
    /// <summary>
    /// an interface for build <see cref="GraphSite"/>
    /// </summary>
    public interface IGraphSiteBuilder
    {
        GraphSite Build(Type serviceType, string tag, bool tagOptional);
    }
}
