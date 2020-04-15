using System;

namespace Tinja.Abstractions.Injection.Graphs.Sites
{
    /// <inheritdoc />
    /// <summary>
    /// AddScoped(typeof(Service),resolver=&gt;new Service());
    /// </summary>
    public class GraphDelegateSite : GraphSite
    {
        public Func<IServiceResolver, object> Delegate { get; set; }

        public override TVisitResult Accept<TVisitResult>(GraphSiteVisitor<TVisitResult> visitor)
        {
            return visitor.VisitDelegate(this);
        }
    }
}
