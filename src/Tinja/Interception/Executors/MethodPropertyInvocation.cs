using System;
using System.Reflection;

namespace Tinja.Interception.Executors
{
    public class MethodPropertyInvocation : MethodInvocation
    {
        public PropertyInfo Property { get; }

        public MethodPropertyInvocation(object contexObject, MethodInfo method, Type[] genericArguments, object[] arguments, IInterceptor[] interceptors, PropertyInfo propertyInfo)
            : base(contexObject, method, genericArguments, arguments, interceptors)
        {
            Property = propertyInfo;
        }
    }
}
