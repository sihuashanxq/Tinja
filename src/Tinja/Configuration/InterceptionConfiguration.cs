using System.Collections.Generic;
using Tinja.Interception;

namespace Tinja.Configuration
{
    public class InterceptionConfiguration
    {
        public bool EnableInterception { get; set; } = true;

        public List<IInterceptorDescriptorProvider> Providers { get; }

        public InterceptionConfiguration()
        {
            Providers = new List<IInterceptorDescriptorProvider>();
        }
    }
}
