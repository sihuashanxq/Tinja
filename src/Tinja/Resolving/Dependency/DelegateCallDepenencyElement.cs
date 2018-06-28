using System;

namespace Tinja.Resolving.Dependency
{
    /// <summary>
    /// AddScoped(typeof(Serice),resolver=>new Service());
    /// </summary>
    public class DelegateCallDepenencyElement : CallDepenencyElement
    {
        public Func<IServiceResolver, object> Delegate { get; set; }

        protected internal override TVisitResult Accept<TVisitResult>(CallDependencyElementVisitor<TVisitResult> visitor)
        {
            return visitor.VisitDelegate(this);
        }
    }
}
