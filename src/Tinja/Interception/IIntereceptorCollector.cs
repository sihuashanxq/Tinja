using System.Reflection;

namespace Tinja.Interception
{
    public interface IIntereceptorCollector
    {
        IIntereceptor[] Collect(MethodInfo targetMethod);
    }
}
