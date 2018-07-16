using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception
{
    public interface IInterceptorDescriptorProvider
    {
        IEnumerable<InterceptorDescriptor> GetInterceptors(MemberInfo memberInfo);
    }
}
