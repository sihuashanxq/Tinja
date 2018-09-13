using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Abstractions.Injection.Dependencies.Elements
{
    /// <inheritdoc />
    /// AddSingleton(typeof(Service),typeof(Service));
    public class TypeCallDependElement : CallDependElement
    {
        public Type ImplementationType { get; set; }

        public ConstructorInfo ConstructorInfo { get; set; }

        public Dictionary<PropertyInfo, CallDependElement> Properties { get; set; }

        public Dictionary<ParameterInfo, CallDependElement> Parameters { get; set; }

        public TypeCallDependElement()
        {
            Properties = new Dictionary<PropertyInfo, CallDependElement>();
        }

        public override TVisitResult Accept<TVisitResult>(CallDependElementVisitor<TVisitResult> visitor)
        {
            return visitor.VisitType(this);
        }
    }
}
