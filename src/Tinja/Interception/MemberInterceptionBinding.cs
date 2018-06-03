namespace Tinja.Interception
{
    public class MemberInterceptionBinding
    {
        public IInterceptor Interceptor { get; }

        public MemberInterception MemberInterception { get; }

        public MemberInterceptionBinding(IInterceptor interceptor, MemberInterception memberInterception)
        {
            Interceptor = interceptor;
            MemberInterception = memberInterception;
        }
    }
}
