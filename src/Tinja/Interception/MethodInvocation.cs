using System;
using System.Reflection;

namespace Tinja.Interception
{
    public class MethodInvocation
    {
        internal static readonly ConstructorInfo Constrcutor = typeof(MethodInvocation)
            .GetConstructor(new[]
            {
                typeof(object),
                typeof(MethodInfo),
                typeof(object[])
            });

        public object Target { get; }

        public MethodInfo TargetMethod { get; }

        public object ReturnValue { get; set; }

        public object[] ParameterValues { get; }

        public MethodInvocation(object target, MethodInfo targetMethod, object[] parameterValues)
        {
            Target = target;
            TargetMethod = targetMethod;
            ParameterValues = parameterValues;
        }
    }
}
