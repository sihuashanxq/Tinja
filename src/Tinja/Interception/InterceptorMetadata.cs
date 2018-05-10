namespace Tinja.Interception
{
    public class InterceptorMetadata
    {
        public IInterceptor Interceptor { get; }

        public InterceptorBinding Declaration { get; }

        public InterceptorMetadata(IInterceptor interceptor, InterceptorBinding declaration)
        {
            Interceptor = interceptor;
            Declaration = declaration;
        }
    }
}
