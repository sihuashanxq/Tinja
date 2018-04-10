﻿using System;
using Tinja.Resolving;
using Tinja.Resolving.Context;

namespace Tinja.LifeStyle
{
    public interface IServiceLifeStyleScope : IDisposable
    {
        object ApplyInstanceLifeStyle(IResolvingContext context, Func<IServiceResolver, object> factory);
    }
}
