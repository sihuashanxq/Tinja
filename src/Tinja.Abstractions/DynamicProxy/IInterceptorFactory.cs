using System;
using System.Collections.Generic;
using System.Text;

namespace Tinja.Abstractions.DynamicProxy
{
    public interface IInterceptorFactory
    {
         IInterceptor Create(Type interceptorType);
    }
}
