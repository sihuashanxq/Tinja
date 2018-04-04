using System;

namespace Tinja.Resolving.Context
{
    public interface IResolvingContext
    {
        Type ReslovingType { get; }

        Component Component { get; }
    }
}
