using System;
using Tinja.ServiceLife;

namespace Tinja.Resolving
{
    /// <summary>
    /// an element that representing an dependency point
    /// </summary>
    public abstract class CallDepenencyElement
    {
        /// <summary>
        /// the service definition type
        /// </summary>
        public Type ServiceType { get; set; }

        /// <summary>
        /// the service life style
        /// </summary>
        public ServiceLifeStyle LifeStyle { get; set; }

        protected internal virtual TVisitResult Accept<TVisitResult>(CallDependencyElementVisitor<TVisitResult> visitor)
        {
            throw new NotImplementedException();
        }
    }
}
