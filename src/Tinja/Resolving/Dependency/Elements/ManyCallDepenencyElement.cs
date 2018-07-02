using System;
using System.Reflection;

namespace Tinja.Resolving.Dependency.Elements
{
    /// <inheritdoc />
    /// <summary>
    /// Resolve(typeof(IEnumerable(T)))
    /// </summary>
    public class ManyCallDepenencyElement : CallDepenencyElement
    {
        public Type ImplementionType { get; set; }

        public ConstructorInfo ConstructorInfo { get; set; }

        public CallDepenencyElement[] Elements { get; set; }

        protected internal override TVisitResult Accept<TVisitResult>(CallDependencyElementVisitor<TVisitResult> visitor)
        {
            return visitor.VisitMany(this);
        }
    }
}
