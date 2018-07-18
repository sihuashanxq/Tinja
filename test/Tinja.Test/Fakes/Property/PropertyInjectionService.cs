﻿using Tinja.Core;
using Tinja.Test.Fakes.Consturctor;

namespace Tinja.Test.Fakes.Property
{
    public interface IPropertyInjectionService
    {
        ITransientServiceB ServiceB { get; set; }
    }

    public class PropertyInjectionService : IPropertyInjectionService
    {
        [Inject]
        public ITransientServiceB ServiceB { get; set; }
    }
}
