using System;

namespace Tinja.Abstractions.Injection.Dependency.Elements
{
    /// <inheritdoc />
    /// <summary>
    /// Resolve(typeof(IEnumerable(T)))
    /// </summary>
    public class EnumerableCallDependElement : CallDependElement
    {
        public Type ItemType { get; set; }

        public CallDependElement[] Items { get; set; }

        public override TVisitResult Accept<TVisitResult>(CallDependElementVisitor<TVisitResult> visitor)
        {
            return visitor.VisitEnumerable(this);
        }
    }
}
