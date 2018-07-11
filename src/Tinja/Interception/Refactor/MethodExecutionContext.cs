using System.Reflection;

namespace Tinja.Interception.Refactor
{
    public class MethodExecutionContext
    {
        public object Result { get; set; }

        public object[] Arguments { get; set; }

        public MethodInfo MethodInfo { get; set; }

        public InterceptorEntryCollection Interceptors { get; set; }
    }
}
