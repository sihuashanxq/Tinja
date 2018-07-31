using System;

namespace Tinja.Abstractions.Injection.Dependency.Elements
{
    /// <inheritdoc />
    /// <summary>
    /// Resolve(typeof(IEnumerable(T)))
    /// </summary>
    public class ManyCallDepenencyElement : CallDepenencyElement
    {
        public Type ElementType { get; set; }

        public CallDepenencyElement[] Elements { get; set; }

        public override TVisitResult Accept<TVisitResult>(CallDependencyElementVisitor<TVisitResult> visitor)
        {
            return visitor.VisitMany(this);
        }
    }
}
