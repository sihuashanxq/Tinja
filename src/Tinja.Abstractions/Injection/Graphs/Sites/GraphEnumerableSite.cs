using System;

namespace Tinja.Abstractions.Injection.Graphs.Sites
{
    /// <inheritdoc />
    /// <summary>
    /// Resolve(typeof(IEnumerable(T)))
    /// </summary>
    public class GraphEnumerableSite : GraphSite
    {
        public Type ElementType { get; set; }

        public GraphSite[] Elements { get; set; }

        public override TVisitResult Accept<TVisitResult>(GraphSiteVisitor<TVisitResult> visitor)
        {
            return visitor.VisitEnumerable(this);
        }
    }
}
