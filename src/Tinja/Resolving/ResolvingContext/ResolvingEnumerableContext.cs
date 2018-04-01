using System;
using System.Collections.Generic;

namespace Tinja.Resolving.ReslovingContext
{
    public class ResolvingEnumerableContext : IResolvingContext
    {
        public Type ReslovingType { get; }

        public Component Component { get; }

        public List<IResolvingContext> ElementsResolvingContext { get; }

        public ResolvingEnumerableContext(
            Type resolvingType,
            Component component,
            List<IResolvingContext> elementsResolvingContext)
        {
            ReslovingType = resolvingType;
            Component = component;
            ElementsResolvingContext = elementsResolvingContext;
        }
    }
}
