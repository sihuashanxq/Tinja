namespace Tinja.Interception
{
    public class InterceptorMetadata
    {
        public IInterceptor Interceptor { get; }

        public InterceptorDeclare Declaration { get; }

        public InterceptorMetadata(IInterceptor interceptor, InterceptorDeclare declaration)
        {
            Interceptor = interceptor;
            Declaration = declaration;
        }
    }
}
