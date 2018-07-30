using System;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IInterceptorFactory
    {
         IInterceptor Create(Type interceptorType);
    }
}
