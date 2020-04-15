using System;

namespace Tinja.Abstractions.Injection.Graphs.Sites
{
    /// <inheritdoc />
    /// <summary>
    /// public MyClass([ConstructorParameterValuerProvider]string name)
    /// </summary>
    public class GraphValueProviderSite : GraphSite
    {
        public Func<IServiceResolver, object> Provider { get; set; }

        public override TVisitResult Accept<TVisitResult>(GraphSiteVisitor<TVisitResult> visitor)
        {
            return visitor.VisitValueProvider(this);
        }
    }
}
