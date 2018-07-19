using System;
using System.Collections.Generic;
using Tinja.Abstractions.DynamicProxy;

namespace Tinja.Core.DynamicProxy.Internals
{
    public class DataAnnotationInterceptorCollector : IInterceptorCollector
    {
        public IEnumerable<InterceptorDefinition> Collect(MemberMetadata metadata)
        {
            throw new NotImplementedException();
        }
    }
}
