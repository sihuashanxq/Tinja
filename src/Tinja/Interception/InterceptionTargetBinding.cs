namespace Tinja.Interception
{
    public class InterceptionTargetBinding
    {
        public IInterceptor Interceptor { get; }

        public InterceptionTarget Target { get; }

        public InterceptionTargetBinding(IInterceptor interceptor, InterceptionTarget target)
        {
            Target = target;
            Interceptor = interceptor;
        }
    }
}
