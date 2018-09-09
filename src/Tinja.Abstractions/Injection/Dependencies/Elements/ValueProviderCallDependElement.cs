
using System;
using System.Reflection;
using Tinja.Abstractions.Injection.DataAnnotations;

namespace Tinja.Abstractions.Injection.Dependencies.Elements
{
    /// <inheritdoc />
    /// <summary>
    /// public MyClass([ConstructorParameterValuerProvider]string name)
    /// </summary>
    public class ValueProviderCallDependElement : CallDependElement
    {
        public Func<IServiceResolver, object> GetValue { get; set; }

        public override TVisitResult Accept<TVisitResult>(CallDependElementVisitor<TVisitResult> visitor)
        {
            return visitor.VisitValueProvider(this);
        }
    }
}
