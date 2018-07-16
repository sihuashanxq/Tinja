using System;
using System.Reflection;

namespace Tinja.Interception.Executors
{
    public class MethodPropertyInvocation : MethodInvocation
    {
        public PropertyInfo Property { get; }

        public MethodPropertyInvocation(object instance, MethodInfo methodInfo, Type[] genericArguments, object[] arguments, IInterceptor[] interceptors, PropertyInfo propertyInfo)
            : base(instance, methodInfo, genericArguments, arguments, interceptors)
        {
            Property = propertyInfo;
        }
    }
}
