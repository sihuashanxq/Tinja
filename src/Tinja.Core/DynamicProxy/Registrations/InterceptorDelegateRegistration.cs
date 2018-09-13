using System;
using System.Threading.Tasks;
using Tinja.Abstractions.DynamicProxy;

namespace Tinja.Core.DynamicProxy.Registrations
{
    internal class InterceptorDelegateRegistration : InterceptorRegistration
    {
        internal Func<IMethodInvocation, Func<IMethodInvocation, Task>, Task> Handler { get; }

        internal InterceptorDelegateRegistration(Func<IMethodInvocation, Func<IMethodInvocation, Task>, Task> handler)
        {
            Handler = handler ?? throw new NullReferenceException(nameof(handler));
        }
    }
}
