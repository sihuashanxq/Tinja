using System;

namespace Tinja.Abstractions.Injection.Graphs.Sites
{
    /// <summary>
    /// an element that representing an dependency site
    /// </summary>
    public abstract class GraphSite
    {
        /// <summary>
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// the service definition type
        /// </summary>
        public Type ServiceType { get; set; }

        /// <summary>
        /// the service life style
        /// </summary>
        public ServiceLifeStyle LifeStyle { get; set; }

        public virtual TVisitResult Accept<TVisitResult>(GraphSiteVisitor<TVisitResult> visitor)
        {
            throw new NotImplementedException();
        }
    }
}
