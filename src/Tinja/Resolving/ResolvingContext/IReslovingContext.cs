using System;

namespace Tinja.Resolving.ReslovingContext
{
    public interface IResolvingContext
    {
        Type ReslovingType { get; }

        Component Component { get; }
    }
}
