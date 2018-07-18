using System;
using System.Collections.Generic;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;

namespace Tinja.Core.DynamicProxy.Internals
{
    public class DataAnnotationInterceptorDefinitionProvider : IInterceptorDefinitonProvider
    {
        public IEnumerable<InterceptorDefinition> GetDefinitions(MemberInfo memberInfo)
        {
            throw new NotImplementedException();
        }
    }
}
