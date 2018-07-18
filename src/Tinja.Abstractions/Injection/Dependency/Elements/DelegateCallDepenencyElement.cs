using System;

namespace Tinja.Abstractions.Injection.Dependency.Elements
{
    /// <inheritdoc />
    /// <summary>
    /// AddScoped(typeof(Serice),resolver=&gt;new Service());
    /// </summary>
    public class DelegateCallDepenencyElement : CallDepenencyElement
    {
        public Func<IServiceResolver, object> Delegate { get; set; }

        public override TVisitResult Accept<TVisitResult>(CallDependencyElementVisitor<TVisitResult> visitor)
        {
            return visitor.VisitDelegate(this);
        }
    }
}
