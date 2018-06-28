using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Resolving.Dependency
{
    /// AddSingleton(typeof(Serice),typeof(Service));
    public class ConstructorCallDependencyElement : CallDepenencyElement
    {
        public Type ImplementionType { get; set; }

        public ConstructorInfo ConstructorInfo { get; set; }

        public Dictionary<PropertyInfo, CallDepenencyElement> Properties { get; set; }

        public Dictionary<ParameterInfo, CallDepenencyElement> Parameters { get; set; }

        protected internal override TVisitResult Accept<TVisitResult>(CallDependencyElementVisitor<TVisitResult> visitor)
        {
            return visitor.VisitConstrcutor(this);
        }
    }
}
