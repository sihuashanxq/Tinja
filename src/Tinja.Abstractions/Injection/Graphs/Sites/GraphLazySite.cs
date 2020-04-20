using System;
using System.Reflection;

namespace Tinja.Abstractions.Injection.Graphs.Sites
{
    public class GraphLazySite : GraphSite
    {
        public string Tag { get; set; }

        public Type ValueType { get; set; }

        public bool TagOptional { get; set; }

        public Type ImplementationType { get; set; }

        public ConstructorInfo ConstructorInfo { get; set; }

        public override TVisitResult Accept<TVisitResult>(GraphSiteVisitor<TVisitResult> visitor)
        {
            return visitor.VisitLazy(this);
        }
    }
}
