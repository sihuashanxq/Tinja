using System;

namespace Tinja.Abstractions.Injection.Dependencies.Elements
{
    /// <inheritdoc />
    /// <summary>
    /// AddScoped(typeof(Service),resolver=&gt;new Service());
    /// </summary>
    public class DelegateCallDependElement : CallDependElement
    {
        public Func<IServiceResolver, object> Delegate { get; set; }

        public override TVisitResult Accept<TVisitResult>(CallDependElementVisitor<TVisitResult> visitor)
        {
            return visitor.VisitDelegate(this);
        }
    }
}
