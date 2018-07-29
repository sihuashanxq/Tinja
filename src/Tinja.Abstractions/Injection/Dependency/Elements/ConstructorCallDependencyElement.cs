using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Abstractions.Injection.Dependency.Elements
{
    /// <inheritdoc />
    /// AddSingleton(typeof(Service),typeof(Service));
    public class ConstructorCallDependencyElement : CallDepenencyElement
    {
        public Type ImplementionType { get; set; }

        public ConstructorInfo ConstructorInfo { get; set; }

        public Dictionary<PropertyInfo, CallDepenencyElement> Properties { get; set; }

        public Dictionary<ParameterInfo, CallDepenencyElement> Parameters { get; set; }

        public ConstructorCallDependencyElement()
        {
            Properties = new Dictionary<PropertyInfo, CallDepenencyElement>();
        }

        public override TVisitResult Accept<TVisitResult>(CallDependencyElementVisitor<TVisitResult> visitor)
        {
            return visitor.VisitConstrcutor(this);
        }
    }
}
