using Tinja.Resolving.Builder;
using Tinja.Resolving.ReslovingContext;

namespace Tinja.Resolving
{
    public interface IServiceNodeBuilder
    {
        IServiceNode Build(IResolvingContext resolvingContext);
    }
}
