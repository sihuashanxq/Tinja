using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tinja.Interception.Internals
{
    public class DataAnnotationInterceptorDefinitionProvider : IInterceptorDefinitonProvider
    {
        public IEnumerable<InterceptorDefinition> GetDefinitions(MemberInfo memberInfo)
        {
            throw new NotImplementedException();
        }
    }
}
